using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KadakChai
{
    /// <summary>
    /// The hero's phone, "bitterChai" (GDD: Phone UI; reference: the bitterSweet phone in the Edits tab).
    /// Bottom half of the screen: status bar (battery = HP, in-game clock, signal = AQI, rupees), a stats line,
    /// the open app's title, the app itself, and a dock of six apps with badges.
    /// Apps: Messages (the story log as chat bubbles), JhatPat, Skills, Profile, Map, Settings.
    /// </summary>
    public class PhoneView : MonoBehaviour
    {
        // Screen layout, in pixels of the 1008x700 screen.
        const float SW = 1008, SH = 762, StatusH = 62, StatsH = 42, TitleH = 54, DockH = 112;
        const float ContentY = StatusH + StatsH + TitleH, ContentH = SH - ContentY - DockH;

        public StoryLog Log { get; private set; }
        /// <summary>True while the shop can be opened (between days). Set by the GameController.</summary>
        public bool ShopReady { get; set; }
        /// <summary>Called when the player opens JhatPat from the dock while the shop is ready.</summary>
        public Action OpenShop;
        public Action OpenStickers; // the sticker sheet (Profile > Stickers)
        public Action<string> OnLog; // mirrors log lines (for unread counts)

        static readonly (string id, string title)[] Apps =
        {
            ("messages", "Messages"), ("jhatpat", "JhatPat"), ("skills", "Skills"),
            ("profile", "Profile"), ("map", "Map"), ("settings", "Settings"),
        };

        Run run;
        RectTransform screen, content, chips;
        readonly Dictionary<string, RectTransform> pages = new();
        readonly Dictionary<string, (Image glow, Image badge, Text badgeText)> dock = new();
        Text clock, dayLabel, money, statsLine, title, unreadText;
        Image batteryFill;
        readonly Image[] bars = new Image[4];
        string current = "messages";
        int unread;
        float dayStartedAt;

        public static readonly Color Screen1 = UiKit.Hex("#2E7D32"), Screen2 = UiKit.Hex("#0B2A0E");
        public static readonly Color Glow = UiKit.Hex("#43C24F");

        public static PhoneView Build(RectTransform phone)
        {
            // Bezel and brand
            var bezel = UiKit.Panel(phone, "PhoneBezel", UiKit.Hex("#141814"), rounded: true);
            bezel.rectTransform.Place(14, 918, 1052, 812);
            UiKit.Outline(bezel, UiKit.Hex("#2F3A2F"), 4);
            var brand = UiKit.Label(bezel.transform, "Brand", "<i>bitter</i><b>Chai</b>", 26, new Color(1, 1, 1, 0.45f));
            brand.rectTransform.Place(0, 0, 1052, 34, 1052, 812);

            var v = bezel.gameObject.AddComponent<PhoneView>();
            v.screen = UiKit.Panel(bezel.transform, "Screen", Color.white).rectTransform;
            ((Image)v.screen.GetComponent<Image>()).sprite = ScreenSprite();
            v.screen.Place(22, 34, SW, SH, 1052, 812);
            v.screen.gameObject.AddComponent<RectMask2D>();

            v.BuildStatusBar();
            v.BuildDock();
            v.content = UiKit.Make("Content", v.screen).Place(0, ContentY, SW, ContentH, SW, SH);
            foreach (var (id, _) in Apps) v.pages[id] = UiKit.Make("App_" + id, v.content);
            foreach (var p in v.pages.Values) p.Fill();

            // Messages: pinned chips row (swipe it sideways when it overflows) + the story log as chat bubbles
            var msg = v.pages["messages"];
            var chipView = UiKit.Make("ChipsView", msg).Place(16, 4, SW - 32, 52, SW, ContentH);
            chipView.gameObject.AddComponent<RectMask2D>();
            chipView.gameObject.AddComponent<Image>().color = Color.clear;   // catches the swipe
            v.chips = UiKit.Make("Chips", chipView);
            v.chips.anchorMin = Vector2.zero; v.chips.anchorMax = new Vector2(0, 1); v.chips.pivot = new Vector2(0, 0.5f);
            v.chips.sizeDelta = Vector2.zero; v.chips.anchoredPosition = Vector2.zero;
            var ch = v.chips.gameObject.AddComponent<HorizontalLayoutGroup>();
            ch.spacing = 8; ch.childForceExpandWidth = false; ch.childControlWidth = true; ch.childControlHeight = true;
            ch.childAlignment = TextAnchor.MiddleLeft;
            v.chips.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            var chipScroll = chipView.gameObject.AddComponent<ScrollRect>();
            chipScroll.content = v.chips; chipScroll.viewport = chipView; chipScroll.horizontal = true; chipScroll.vertical = false;
            chipScroll.movementType = ScrollRect.MovementType.Clamped; chipScroll.scrollSensitivity = 30;
            var logArea = UiKit.Make("LogArea", msg).Place(6, 60, SW - 12, ContentH - 62, SW, ContentH);
            v.Log = StoryLog.Build(logArea);
            v.Log.Added += v.OnLogAdded;

            v.ShowApp("messages");
            return v;
        }

        public void Bind(Run r) { run = r; unread = 0; Refresh(); }
        public void NewDay() => dayStartedAt = Time.time;
        int skillsSeenDay = -1;   // the Skills "!" badge (something expires) stops nagging once opened that day

        // ------------------------------------------------------------------ status bar

        void BuildStatusBar()
        {
            var bar = UiKit.Panel(screen, "StatusBar", new Color(0, 0, 0, 0.45f));
            bar.rectTransform.Place(0, 0, SW, StatusH, SW, SH);

            // battery (HP)
            var batt = UiKit.Panel(bar.transform, "Battery", Color.clear);
            batt.rectTransform.Place(18, 16, 62, 30, SW, StatusH);
            UiKit.Outline(batt, Color.white, 3);
            var shell = UiKit.Panel(batt.transform, "Shell", new Color(1, 1, 1, 0.15f));
            shell.rectTransform.Fill();
            batteryFill = UiKit.Panel(batt.transform, "Fill", Color.green);
            batteryFill.rectTransform.anchorMin = new Vector2(0, 0); batteryFill.rectTransform.anchorMax = new Vector2(1, 1);
            batteryFill.rectTransform.offsetMin = new Vector2(4, 4); batteryFill.rectTransform.offsetMax = new Vector2(-4, -4);
            var nub = UiKit.Panel(bar.transform, "Nub", Color.white);
            nub.rectTransform.Place(82, 25, 6, 12, SW, StatusH);

            dayLabel = UiKit.Label(bar.transform, "Day", "", 30, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
            dayLabel.rectTransform.Place(100, 0, 240, StatusH, SW, StatusH);
            clock = UiKit.Label(bar.transform, "Clock", "09:00", 48, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            clock.rectTransform.Place(354, -2, 300, StatusH, SW, StatusH);
            money = UiKit.Label(bar.transform, "Money", "", 34, ShopView.Brand, TextAnchor.MiddleRight, FontStyle.Bold);
            money.rectTransform.Place(640, 0, 230, StatusH, SW, StatusH);
            for (int i = 0; i < 4; i++)
            {
                bars[i] = UiKit.Panel(bar.transform, "Bar" + i, Color.white);
                float h = 10 + i * 8;
                bars[i].rectTransform.Place(900 + i * 20, 46 - h, 13, h, SW, StatusH);
            }

            // stats line with the unread count, then the app title (bitterSweet's "2 [mail] 0 [chat]" + "Contacts")
            var mail = UiKit.Panel(screen, "MailIcon", Color.white);
            mail.sprite = Art.UI("app_messages");
            mail.preserveAspect = true;
            mail.rectTransform.Place(14, StatusH + 2, 40, 38, SW, SH);
            unreadText = UiKit.Label(screen, "Unread", "0", 28, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
            unreadText.rectTransform.Place(58, StatusH, 80, StatsH, SW, SH);
            statsLine = UiKit.Label(screen, "Stats", "", 28, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            statsLine.rectTransform.Place(120, StatusH, SW - 240, StatsH, SW, SH);
            UiKit.Outline(statsLine, new Color(0, 0, 0, 0.5f), 2);
            title = UiKit.Label(screen, "Title", "Messages", 44, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            title.rectTransform.Place(0, StatusH + StatsH, SW, TitleH, SW, SH);
            UiKit.Outline(title, new Color(0, 0, 0, 0.6f), 3);
        }

        void BuildDock()
        {
            var bg = UiKit.Panel(screen, "Dock", new Color(0, 0, 0, 0.55f));
            bg.rectTransform.Place(0, SH - DockH, SW, DockH, SW, SH);
            float slot = SW / Apps.Length;
            for (int i = 0; i < Apps.Length; i++)
            {
                var (id, _) = Apps[i];
                var glow = UiKit.Panel(bg.transform, "Glow_" + id, Glow, rounded: true);
                glow.rectTransform.Place(i * slot + slot / 2 - 50, 6, 100, 100, SW, DockH);
                var btn = UiKit.Button(bg.transform, "App_" + id, "", Color.clear, 20, () => ShowApp(id));
                btn.GetComponent<RectTransform>().Place(i * slot + slot / 2 - 46, 10, 92, 92, SW, DockH);
                var icon = UiKit.Panel(btn.transform, "Icon", Color.white);
                icon.sprite = Art.UI("app_" + id);
                icon.preserveAspect = true;
                icon.raycastTarget = false;
                icon.rectTransform.Fill();
                if (icon.sprite == null) btn.SetText(id.Substring(0, 3));
                var badge = UiKit.Panel(bg.transform, "Badge_" + id, UiKit.Red);
                badge.sprite = Art.Dot;
                badge.raycastTarget = false;
                badge.rectTransform.Place(i * slot + slot / 2 + 22, 4, 40, 40, SW, DockH);
                var bt = UiKit.Label(badge.transform, "T", "", 24, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
                bt.rectTransform.Fill();
                badge.gameObject.SetActive(false);
                dock[id] = (glow, badge, bt);
            }
        }

        /// <summary>Green screen with a radial glow and a dot grid, like the reference phone.</summary>
        static Sprite screenSprite;
        static Sprite ScreenSprite()
        {
            if (screenSprite != null) return screenSprite;
            const int w = 256, h = 178;
            var tex = new Texture2D(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp };
            var px = new Color32[w * h];
            for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    float dx = (x - w / 2f) / (w / 2f), dy = (y - h * 0.45f) / (h / 2f);
                    float k = Mathf.Clamp01(Mathf.Sqrt(dx * dx * 0.7f + dy * dy));
                    var c = Color.Lerp(Screen1, Screen2, k);
                    if (x % 6 < 2 && y % 6 < 2) c = Color.Lerp(c, Color.black, 0.35f); // dot grid
                    px[y * w + x] = c;
                }
            tex.SetPixels32(px);
            tex.Apply();
            screenSprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
            return screenSprite;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => screenSprite = null;

        // ------------------------------------------------------------------ apps

        public void ShowApp(string id)
        {
            if (id == "jhatpat")
            {
                if (ShopReady && OpenShop != null) { OpenShop(); return; }
                BuildJhatPatClosed();
            }
            current = id;
            if (id == "skills" && run != null) skillsSeenDay = run.DayNumber;
            foreach (var kv in pages) kv.Value.gameObject.SetActive(kv.Key == id);
            foreach (var kv in dock) kv.Value.glow.enabled = kv.Key == id;
            foreach (var (aid, t) in Apps) if (aid == id) title.text = t;
            if (id == "messages") { unread = 0; Log.ScrollToEnd(); }
            Refresh();
        }

        void OnLogAdded()
        {
            if (current != "messages") unread++;
            if (unreadText != null) RefreshBadges();
        }

        void Update()
        {
            // In-game clock: each day starts at 09:00 and runs one game-minute per real second.
            int minutes = 9 * 60 + Mathf.Min((int)(Time.time - dayStartedAt), 14 * 60 + 59);
            clock.text = $"{minutes / 60:00}:{minutes % 60:00}";
        }

        /// <summary>During a fight: show Bablu's live HP in the header without touching the run state.</summary>
        public void ShowLiveHp(int hp)
        {
            if (run == null) return;
            var s = run.S;
            float f = s.maxHp > 0 ? (float)hp / s.maxHp : 0;
            batteryFill.rectTransform.anchorMax = new Vector2(Mathf.Max(0.05f, f), 1);
            batteryFill.color = f > 0.5f ? UiKit.Hex("#66BB6A") : f > 0.25f ? UiKit.Hex("#FFCA28") : UiKit.Red;
            string toll = s.tollDays > 0 ? "<color=#FF8A80>" : "";
            string end = s.tollDays > 0 ? "</color>" : "";
            statsLine.text = $"Lv {s.level}   HP {Mathf.Max(0, hp)}/{s.maxHp}   {toll}ATK {s.AtkTotal}   DEF {s.DefTotal}{end}";
        }

        public void Refresh()
        {
            if (run == null) return;
            var s = run.S;
            dayLabel.text = run.Finished ? "Done" : $"Day {run.DayNumber}";
            money.text = "₹" + s.rupees;
            float hp = s.maxHp > 0 ? (float)s.hp / s.maxHp : 0;
            batteryFill.rectTransform.anchorMax = new Vector2(Mathf.Max(0.05f, hp), 1);
            batteryFill.color = hp > 0.5f ? UiKit.Hex("#66BB6A") : hp > 0.25f ? UiKit.Hex("#FFCA28") : UiKit.Red;
            int lit = 4 - (int)Aqi.StageFor(s.aqi); // signal drops as the AQI rises
            for (int i = 0; i < 4; i++) bars[i].color = i < lit ? Color.white : new Color(1, 1, 1, 0.2f);

            string toll = s.tollDays > 0 ? "<color=#FF8A80>" : "";
            string end = s.tollDays > 0 ? "</color>" : "";
            statsLine.text = $"Lv {s.level}   HP {s.hp}/{s.maxHp}   {toll}ATK {s.AtkTotal}   DEF {s.DefTotal}{end}";

            RefreshChips(s);
            RefreshBadges();
            switch (current)
            {
                case "skills": BuildSkills(s); break;
                case "profile": BuildProfile(s); break;
                case "map": BuildMap(); break;
                case "settings": BuildSettings(); break;
            }
        }

        void RefreshBadges()
        {
            unreadText.text = unread.ToString();
            SetBadge("messages", unread > 0 ? (unread > 9 ? "9+" : unread.ToString()) : null);
            SetBadge("jhatpat", ShopReady ? "!" : null);
            bool expiring = run != null && (run.S.skills.Exists(o => o.IsLastDay) || run.S.weapon?.IsLastFight == true);
            SetBadge("skills", expiring && skillsSeenDay != run.DayNumber ? "!" : null);   // clears once you've looked today
        }

        void SetBadge(string id, string text)
        {
            var (_, badge, t) = dock[id];
            badge.gameObject.SetActive(text != null);
            if (text != null) t.text = text;
        }

        void RefreshChips(RunState s)
        {
            foreach (Transform c in chips) Destroy(c.gameObject);
            chips.anchoredPosition = Vector2.zero;
            if (s.weapon != null) Chip($"{s.weapon.data.name} <b>{s.weapon.fightsLeft}f</b>", s.weapon.IsLastFight ? UiKit.Red : UiKit.Hex(s.Ranged ? "#1E88E5" : "#C62828"));
            if (s.armour != null) Chip($"{s.armour.data.name} <b>{s.armour.fightsLeft}f</b>", s.armour.IsLastFight ? UiKit.Red : UiKit.Hex("#546E7A"));
            if (s.fastags > 0) Chip(s.fastags > 1 ? $"FASTag x{s.fastags}" : "FASTag", UiKit.Hex("#00897B"));
            if (s.tollDays > 0) Chip($"Toll Tax <b>{s.tollDays}d</b>", UiKit.Hex("#B71C1C"));
            if (s.insuranceDays > 0) Chip($"Insured <b>{s.insuranceDays}d</b>", UiKit.Hex("#1565C0"));
            if (s.zombieFights > 0) Chip("Zombie <b>1 fight</b>", UiKit.Hex("#558B2F"));
            if (s.conditions.Count > 2) Chip($"<b>{s.conditions.Count}</b> statuses", UiKit.Hex("#8E24AA")); // full list in the Skills app
            else foreach (var c in s.conditions) Chip(Conditions.Name(c), UiKit.Hex("#8E24AA"));
            foreach (var o in s.skills) Chip($"{o.data.shortName} <b>{o.daysLeft}d</b>", o.IsLastDay ? UiKit.Red : new Color(1, 1, 1, 0.18f));
            if (chips.childCount == 0)
            {
                var none = UiKit.Label(chips, "None", "No skills. Everything is temporary.", 24, new Color(1, 1, 1, 0.6f), TextAnchor.MiddleLeft);
                none.gameObject.AddComponent<LayoutElement>().preferredWidth = 900;
            }
        }

        void Chip(string text, Color color)
        {
            var chip = UiKit.Panel(chips, "Chip", color, rounded: true);
            var lg = chip.gameObject.AddComponent<HorizontalLayoutGroup>();
            lg.padding = new RectOffset(14, 14, 4, 4);
            lg.childControlWidth = true; lg.childControlHeight = true;
            lg.childForceExpandWidth = false; lg.childForceExpandHeight = false;
            var t = UiKit.Label(chip.transform, "T", text, 24, Color.white);
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
        }

        // Each app page is rebuilt on Refresh while it is open; they are small.
        RectTransform ClearPage(string id)
        {
            var p = pages[id];
            foreach (Transform c in p) Destroy(c.gameObject);
            return p;
        }

        Text Row(RectTransform page, float y, string text, int size = 36, Color? color = null, TextAnchor anchor = TextAnchor.MiddleLeft, float ph = ContentH)
        {
            var t = UiKit.Label(page, "Row", UiKit.Rich(text), size, color ?? Color.white, anchor);
            t.rectTransform.Place(30, y, SW - 60, size + 18, SW, ph);
            UiKit.Outline(t, new Color(0, 0, 0, 0.45f), 2);
            return t;
        }

        void Card(RectTransform page, float y, float h, Color color, float ph = ContentH)
        {
            var c = UiKit.Panel(page, "Card", color, rounded: true);
            c.rectTransform.Place(20, y, SW - 40, h, SW, ph);
            c.transform.SetAsFirstSibling();
        }

        void BuildSkills(RunState s)
        {
            var p = ClearPage("skills");
            var items = new List<(string head, string sub, bool warn)>();
            if (s.weapon != null) items.Add(($"<b>{s.weapon.data.name}</b>  ·  {s.weapon.fightsLeft} fight{(s.weapon.fightsLeft > 1 ? "s" : "")} left", s.weapon.data.desc, s.weapon.IsLastFight));
            if (s.armour != null) items.Add(($"<b>{s.armour.data.name}</b>  ·  {s.armour.fightsLeft} fights left", s.armour.data.desc, s.armour.IsLastFight));
            foreach (var o in s.skills) items.Add(($"<b>{o.data.name}</b>  ·  lease: {o.daysLeft} day{(o.daysLeft > 1 ? "s" : "")}", o.data.desc, o.IsLastDay));
            foreach (var c in s.conditions)
                items.Add(($"<b>{Conditions.Name(c)}</b>  ·  until cured", Conditions.All[c].desc + " Cure: Pudin Hara at JhatPat.", true));
            if (s.insuranceDays > 0) items.Add(($"<b>Health Insurance</b>  ·  {s.insuranceDays} day{(s.insuranceDays > 1 ? "s" : "")} left", "If Bablu dies, AIIMS revives him once at 50% HP.", s.insuranceDays == 1));
            if (s.tollDays > 0) items.Add(($"<b>Toll Tax</b>  ·  {s.tollDays} day{(s.tollDays > 1 ? "s" : "")} left", "All stats -15%. A FASTag skips the next one.", true));

            // The list can outgrow the screen (gear + leases + statuses + toll), so it scrolls inside the page
            // instead of running over the dock (feedback log, 04 Oct 03:06).
            const float RowH = 112;
            float listH = Mathf.Max(ContentH, 6 + items.Count * RowH);
            var view = UiKit.Make("SkillsView", p);
            view.Fill();
            view.gameObject.AddComponent<RectMask2D>();
            view.gameObject.AddComponent<Image>().color = Color.clear;   // catches the swipe
            var list = UiKit.Make("List", view);
            list.anchorMin = new Vector2(0, 1); list.anchorMax = new Vector2(1, 1); list.pivot = new Vector2(0.5f, 1);
            float unit = p.rect.height > 1 ? p.rect.height / ContentH : 1f;   // local units per reference pixel
            list.sizeDelta = new Vector2(0, listH * unit);
            list.anchoredPosition = Vector2.zero;
            var sr = view.gameObject.AddComponent<ScrollRect>();
            sr.content = list; sr.viewport = view; sr.horizontal = false; sr.vertical = true;
            sr.movementType = ScrollRect.MovementType.Clamped; sr.scrollSensitivity = 30;

            float y = 6;
            foreach (var (head, sub, warn) in items)
            {
                Card(list, y, 104, warn ? new Color(0.8f, 0.15f, 0.15f, 0.55f) : new Color(0, 0, 0, 0.35f), listH);
                Row(list, y + 4, head, 38, ph: listH);
                var t = Row(list, y + 52, sub, 30, new Color(1, 1, 1, 0.85f), ph: listH);
                t.rectTransform.Place(30, y + 52, SW - 60, 46, SW, listH);
                y += RowH;
            }
            if (items.Count == 0)
            {
                var t = Row(p, 90, "No skills yet.\n<size=34>Astrologer Baba hands them out when Bablu levels up. Every skill has a lease.</size>", 46, null, TextAnchor.MiddleCenter);
                t.rectTransform.Place(40, 60, SW - 80, 300, SW, ContentH);
            }
            if (listH > ContentH)
            {
                var more = UiKit.Label(p, "More", "swipe up for more", 26, new Color(1, 1, 1, 0.8f), TextAnchor.MiddleRight, FontStyle.Bold);
                more.rectTransform.Place(SW - 330, ContentH - 40, 300, 36, SW, ContentH);
                UiKit.Outline(more, Color.black, 2);
            }
        }

        /// <summary>
        /// Profile: Bablu's college ID card (Jaideep's design, 04 Oct): a tilted paper card over the page with a
        /// paper-clipped sepia photo, his signature, and stats with the gear bonuses in green and penalties in red.
        /// Drop Resources/Art/UI/id_card.png in to replace the drawn card background.
        /// </summary>
        void BuildProfile(RunState s)
        {
            var p = ClearPage("profile");
            const float CW = 948, CH = 590;
            var cardArt = Art.UI("id_card");
            var card = UiKit.Panel(p, "IdCard", cardArt != null ? Color.white : UiKit.Hex("#F3E3C3"), rounded: cardArt == null);
            if (cardArt != null) card.sprite = cardArt;
            card.rectTransform.Place(30, -100, CW, CH, SW, ContentH);   // covers the page title: the card is the page
            card.rectTransform.localRotation = Quaternion.Euler(0, 0, 1.5f);
            if (cardArt == null)
            {
                UiKit.Outline(card, UiKit.Hex("#3E2C1C"), 5);
                var sh = card.gameObject.AddComponent<Shadow>();
                sh.effectColor = new Color(0, 0, 0, 0.45f); sh.effectDistance = new Vector2(10, -12);
            }
            Text Txt(string text, float x, float y, float w, float h, int size, Color color, FontStyle style = FontStyle.Normal, TextAnchor anchor = TextAnchor.MiddleLeft)
            {
                var t = UiKit.Label(card.transform, "T", text, size, color, anchor, style);
                t.rectTransform.Place(x, y, w, h, CW, CH);
                return t;
            }
            var ink = UiKit.Hex("#2B2118");

            // The photo: white frame, sepia Bablu (hurt face below a third of his HP, bald once shaved), a paperclip.
            float hpr = s.maxHp > 0 ? (float)s.hp / s.maxHp : 1;
            string who = s.conditions.Contains("shaved") ? "new_kid_shaved" : "new_kid";
            var frame = UiKit.Panel(card.transform, "Photo", Color.white);
            frame.raycastTarget = false;
            frame.rectTransform.Place(36, 40, 300, 330, CW, CH);
            frame.rectTransform.localRotation = Quaternion.Euler(0, 0, -4f);
            UiKit.Outline(frame, new Color(0, 0, 0, 0.35f), 3);
            var window = UiKit.Make("Window", frame.transform);
            window.Fill(14);
            window.gameObject.AddComponent<RectMask2D>();
            var bg = UiKit.Panel(window, "Bg", UiKit.Hex("#E9DCC1"));
            bg.raycastTarget = false; bg.rectTransform.Fill();
            var photo = UiKit.Panel(window, "Bablu", Color.white);
            photo.sprite = (hpr < 0.33f ? Art.UI("id_" + who + "_hit") : null) ?? Art.UI("id_" + who) ?? Art.Character(who).sprite;
            photo.preserveAspect = true;
            photo.raycastTarget = false;
            photo.rectTransform.anchorMin = new Vector2(-0.2f, -0.75f);   // larger than the window and pinned high: head and shoulders
            photo.rectTransform.anchorMax = new Vector2(1.2f, 1.0f);
            photo.rectTransform.offsetMin = photo.rectTransform.offsetMax = Vector2.zero;
            var clip = UiKit.Panel(card.transform, "Paperclip", Color.white);
            clip.sprite = Art.UI("paperclip");
            clip.preserveAspect = true;
            clip.raycastTarget = false;
            clip.rectTransform.Place(14, 4, 110, 170, CW, CH);
            if (clip.sprite == null) clip.enabled = false;
            var sign = Txt(GameData.HeroName, 50, 300, 320, 100, 72, UiKit.Hex("#6A1B9A"), FontStyle.BoldAndItalic, TextAnchor.MiddleCenter);
            sign.rectTransform.localRotation = Quaternion.Euler(0, 0, 12f);
            var stroke = UiKit.Panel(card.transform, "SignStroke", UiKit.Hex("#6A1B9A"));
            stroke.raycastTarget = false;
            stroke.rectTransform.Place(60, 370, 300, 7, CW, CH);
            stroke.rectTransform.localRotation = Quaternion.Euler(0, 0, 9f);

            // Who he is
            Txt("Student", 40, 410, 300, 60, 46, ink, FontStyle.Bold);
            Txt("B. Tech  <size=28>(4 backlogs)</size>", 80, 464, 320, 56, 40, ink, FontStyle.Italic);

            // Stats: labels bold, values handwritten-ish; bonuses green, penalties red
            Txt("Profile", 380, 34, 400, 84, 68, ink, FontStyle.Bold);
            var lv = UiKit.Panel(card.transform, "LvStamp", new Color(0.83f, 0.18f, 0.18f, 0.9f), rounded: true);
            lv.raycastTarget = false;
            lv.rectTransform.Place(770, 40, 140, 70, CW, CH);
            lv.rectTransform.localRotation = Quaternion.Euler(0, 0, -10f);
            var lvt = UiKit.Label(lv.transform, "T", "LV " + s.level, 40, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            lvt.rectTransform.Fill();
            string Mod(int v) => v == 0 ? "" : v > 0 ? $" <color=#2E7D32>(+{v})</color>" : $" <color=#C62828>({v})</color>";
            float toll = s.tollDays > 0 ? 1f - RunState.TollPenalty : 1f;
            int atkGear = s.weapon?.data.atk ?? 0, defGear = s.armour?.data.def ?? 0;
            int atkPen = Mathf.RoundToInt(s.AtkTotal * toll) - s.AtkTotal - (s.conditions.Contains("cancelled") ? Mathf.RoundToInt(s.AtkTotal * (1 - Conditions.CancelledAtk)) : 0);
            int defPen = Mathf.RoundToInt(s.DefTotal * toll) - s.DefTotal - (s.conditions.Contains("shaved") ? Mathf.RoundToInt(s.DefTotal * (1 - Conditions.ShavedDef)) : 0);
            Txt("EXP", 390, 130, 200, 46, 36, ink, FontStyle.Bold);
            Txt($"{s.xp}/{s.XpToNext}", 390, 170, 220, 50, 38, ink, FontStyle.Italic);
            Txt("HP", 630, 130, 200, 46, 36, ink, FontStyle.Bold);
            Txt($"{s.hp}/{s.maxHp}", 630, 170, 260, 50, 38, hpr < 0.33f ? UiKit.Hex("#C62828") : ink, FontStyle.Italic);
            Txt("ATK", 390, 236, 200, 46, 36, ink, FontStyle.Bold);
            Txt($"{s.atk}{Mod(atkGear)}{Mod(atkPen)}", 390, 276, 240, 50, 38, ink, FontStyle.Italic);
            Txt("DEF", 630, 236, 200, 46, 36, ink, FontStyle.Bold);
            Txt($"{s.def}{Mod(defGear)}{Mod(defPen)}", 630, 276, 260, 50, 38, ink, FontStyle.Italic);
            int party = 1 + (s.HasCompanion ? 1 : 0);
            Txt($"₹{s.rupees}", 600, 360, 320, 70, 60, ink, FontStyle.Bold, TextAnchor.MiddleRight);
            Txt($"Party: {party:00}" + (s.HasCompanion ? $"  <size=24>(+{GameData.SpeakerName(s.companion)})</size>" : ""), 470, 428, 450, 46, 32, ink, FontStyle.Italic, TextAnchor.MiddleRight);

            var stickers = UiKit.Button(card.transform, "Stickers", $"Stickers  {Album.Count}/{Album.Total}", UiKit.Hex("#FFB300"), 32, () => OpenStickers?.Invoke());
            stickers.GetComponent<RectTransform>().Place(620, 490, 300, 70, CW, CH);
        }

        void BuildMap()
        {
            var p = ClearPage("map");
            float size = ContentH - 10;
            float mx = (SW - size) / 2, my = 4;
            var map = UiKit.Panel(p, "Map", Color.white);
            map.sprite = Art.UI("map");
            map.preserveAspect = true;
            map.rectTransform.Place(mx, my, size, size, SW, ContentH);
            var tap = map.gameObject.AddComponent<Button>();
            tap.onClick.AddListener(OpenBigMap);
            var hint = UiKit.Label(p, "Hint", "Tap the map\nto zoom", 30, new Color(1, 1, 1, 0.8f), TextAnchor.LowerRight, FontStyle.Bold);
            hint.rectTransform.Place(SW - mx + 10, ContentH - 110, mx - 30, 100, SW, ContentH);
            if (run == null || run.Finished) return;
            if (!MapView.Cities.TryGetValue(run.Day.zone, out var c)) return;
            float k = size / 1000f, m = 70;
            var marker = UiKit.Panel(p, "Marker", Color.white);
            marker.sprite = Art.Character("new_kid").sprite;
            marker.preserveAspect = true;
            marker.rectTransform.Place(mx + c.x * k - m / 2, my + c.y * k - m * 0.9f, m, m, SW, ContentH);
            var day = UiKit.Label(p, "Where", $"Day {run.DayNumber} of {run.TotalDays}\n{GameData.ZoneName(run.Day.zone)}", 32, Color.white, TextAnchor.UpperLeft, FontStyle.Bold);
            day.rectTransform.Place(20, 10, mx - 30, 120, SW, ContentH);
            UiKit.Outline(day, new Color(0, 0, 0, 0.6f), 2);
        }

        System.Collections.IEnumerator Bob(RectTransform rt)
        {
            Vector2 home = rt.anchoredPosition;
            while (rt != null && rt.gameObject.activeInHierarchy)
            {
                rt.anchoredPosition = home + new Vector2(Mathf.Sin(Time.time * 9f) * 4f, Mathf.Abs(Mathf.Sin(Time.time * 18f)) * 6f);
                yield return null;
            }
        }

        /// <summary>Full-screen NCR map: drag to pan, +/- to zoom (change log 19:21).</summary>
        void OpenBigMap()
        {
            var root = (RectTransform)transform.parent;
            var dim = UiKit.Panel(root, "BigMap", new Color(0.04f, 0.05f, 0.08f, 0.96f));
            dim.rectTransform.Fill();
            var view = UiKit.Make("Viewport", dim.transform).Place(20, 120, 1040, 1560);
            view.gameObject.AddComponent<RectMask2D>();
            view.gameObject.AddComponent<Image>().color = new Color(0, 0, 0, 0.01f); // catches drags
            var content = UiKit.Make("Content", view);
            content.anchorMin = content.anchorMax = new Vector2(0.5f, 0.5f);
            content.sizeDelta = new Vector2(1040, 1040);
            var img = UiKit.Panel(content, "Map", Color.white);
            img.sprite = Art.UI("map");
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.rectTransform.Fill();
            if (run != null && !run.Finished && MapView.Cities.TryGetValue(run.Day.zone, out var c))
            {
                var m = UiKit.Panel(content, "Marker", Color.white);
                m.sprite = Art.Character("new_kid").sprite;
                m.preserveAspect = true;
                m.raycastTarget = false;
                m.rectTransform.anchorMin = m.rectTransform.anchorMax = new Vector2(c.x / 1000f, 1 - c.y / 1000f);
                m.rectTransform.pivot = new Vector2(0.5f, 0.1f);
                m.rectTransform.sizeDelta = new Vector2(130, 130);
            }
            var sr = view.gameObject.AddComponent<ScrollRect>();
            sr.content = content; sr.viewport = view; sr.movementType = ScrollRect.MovementType.Clamped; sr.scrollSensitivity = 30;
            float zoom = 1f;
            void Zoom(float z)
            {
                zoom = Mathf.Clamp(z, 1f, 3f);
                content.localScale = Vector3.one * zoom;
                sr.horizontal = sr.vertical = zoom > 1.01f;
                if (zoom <= 1.01f) content.anchoredPosition = Vector2.zero;
            }
            Zoom(1f);
            var title = UiKit.Label(dim.transform, "Title", "NCR  ·  drag to look around", 40, ShopView.Brand, TextAnchor.MiddleCenter, FontStyle.Bold);
            title.rectTransform.Place(20, 30, 1040, 80);
            UiKit.Button(dim.transform, "ZoomIn", "+", UiKit.Green, 70, () => Zoom(zoom + 0.5f)).GetComponent<RectTransform>().Place(40, 1736, 200, 150);
            UiKit.Button(dim.transform, "ZoomOut", "–", UiKit.Purple, 70, () => Zoom(zoom - 0.5f)).GetComponent<RectTransform>().Place(260, 1736, 200, 150);
            UiKit.Button(dim.transform, "MapClose", "Back to phone", UiKit.Orange, 44, () => Destroy(dim.gameObject)).GetComponent<RectTransform>().Place(480, 1736, 560, 150);
        }

        void BuildSettings()
        {
            var p = ClearPage("settings");
            Button sfx = null, music = null;
            void Sync()
            {
                sfx.SetText(AudioKit.SfxOn ? "Sound FX: ON" : "Sound FX: OFF");
                sfx.targetGraphic.color = AudioKit.SfxOn ? UiKit.Green : new Color(0.4f, 0.4f, 0.45f);
                music.SetText(AudioKit.MusicOn ? "Music: ON" : "Music: OFF");
                music.targetGraphic.color = AudioKit.MusicOn ? UiKit.Green : new Color(0.4f, 0.4f, 0.45f);
            }
            sfx = UiKit.Button(p, "PhoneSfx", "", UiKit.Green, 38, () => { AudioKit.SfxOn = !AudioKit.SfxOn; Sync(); AudioKit.Sfx("click"); });
            sfx.GetComponent<RectTransform>().Place(80, 20, 400, 110, SW, ContentH);
            music = UiKit.Button(p, "PhoneMusic", "", UiKit.Green, 38, () => { AudioKit.MusicOn = !AudioKit.MusicOn; Sync(); });
            music.GetComponent<RectTransform>().Place(528, 20, 400, 110, SW, ContentH);
            Sync();
            Row(p, 160, "Battle speed: tap the yellow button in the top-right of the fight.", 32, new Color(1, 1, 1, 0.85f), TextAnchor.MiddleCenter);
            Row(p, 230, "<b>bitterChai OS 11.0</b>  ·  valid for 11 months", 32, new Color(1, 1, 1, 0.65f), TextAnchor.MiddleCenter);
            Row(p, 290, "Made by Siddharth Gupta, Dhivesh, Jaideep and Amolika", 32, new Color(1, 1, 1, 0.65f), TextAnchor.MiddleCenter);
        }

        void BuildJhatPatClosed()
        {
            var p = ClearPage("jhatpat");
            var icon = UiKit.Panel(p, "Scooty", Color.white);
            icon.sprite = Art.UI("scooty") ?? Art.UI("mystery_box");
            icon.preserveAspect = true;
            icon.rectTransform.Place(SW / 2 - 120, 10, 240, 240, SW, ContentH);
            StartCoroutine(Bob(icon.rectTransform));
            Row(p, 262, "Your Delivery Wale Bhaiya is running late.", 40, ShopView.Brand, TextAnchor.MiddleCenter);
            Row(p, 322, "JhatPat opens after today's event. \"2 minute, sir.\"", 32, new Color(1, 1, 1, 0.85f), TextAnchor.MiddleCenter);
        }
    }
}
