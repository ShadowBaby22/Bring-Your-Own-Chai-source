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

            // Messages: pinned chips row + the story log as chat bubbles
            var msg = v.pages["messages"];
            v.chips = UiKit.Make("Chips", msg).Place(16, 4, SW - 32, 52, SW, ContentH);
            var ch = v.chips.gameObject.AddComponent<HorizontalLayoutGroup>();
            ch.spacing = 8; ch.childForceExpandWidth = false; ch.childControlWidth = true; ch.childControlHeight = true;
            ch.childAlignment = TextAnchor.MiddleLeft;
            var logArea = UiKit.Make("LogArea", msg).Place(6, 60, SW - 12, ContentH - 62, SW, ContentH);
            v.Log = StoryLog.Build(logArea);
            v.Log.Added += v.OnLogAdded;

            v.ShowApp("messages");
            return v;
        }

        public void Bind(Run r) { run = r; unread = 0; Refresh(); }
        public void NewDay() => dayStartedAt = Time.time;

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
            SetBadge("skills", expiring ? "!" : null);
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
            if (s.weapon != null) Chip($"{s.weapon.data.name} <b>{s.weapon.fightsLeft}f</b>", s.weapon.IsLastFight ? UiKit.Red : UiKit.Hex(s.Ranged ? "#1E88E5" : "#C62828"));
            if (s.armour != null) Chip($"{s.armour.data.name} <b>{s.armour.fightsLeft}f</b>", s.armour.IsLastFight ? UiKit.Red : UiKit.Hex("#546E7A"));
            if (s.fastags > 0) Chip(s.fastags > 1 ? $"FASTag x{s.fastags}" : "FASTag", UiKit.Hex("#00897B"));
            if (s.tollDays > 0) Chip($"Toll Tax <b>{s.tollDays}d</b>", UiKit.Hex("#B71C1C"));
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

        Text Row(RectTransform page, float y, string text, int size = 30, Color? color = null, TextAnchor anchor = TextAnchor.MiddleLeft)
        {
            var t = UiKit.Label(page, "Row", UiKit.Rich(text), size, color ?? Color.white, anchor);
            t.rectTransform.Place(30, y, SW - 60, size + 18, SW, ContentH);
            UiKit.Outline(t, new Color(0, 0, 0, 0.45f), 2);
            return t;
        }

        void Card(RectTransform page, float y, float h, Color color)
        {
            var c = UiKit.Panel(page, "Card", color, rounded: true);
            c.rectTransform.Place(20, y, SW - 40, h, SW, ContentH);
            c.transform.SetAsFirstSibling();
        }

        void BuildSkills(RunState s)
        {
            var p = ClearPage("skills");
            float y = 6;
            void Item(string head, string sub, bool warn)
            {
                Card(p, y, 86, warn ? new Color(0.8f, 0.15f, 0.15f, 0.55f) : new Color(0, 0, 0, 0.35f));
                Row(p, y + 4, head, 32);
                var t = Row(p, y + 44, sub, 26, new Color(1, 1, 1, 0.85f));
                t.rectTransform.Place(30, y + 44, SW - 60, 38, SW, ContentH);
                y += 94;
            }
            if (s.weapon != null) Item($"<b>{s.weapon.data.name}</b>  ·  {s.weapon.fightsLeft} fight{(s.weapon.fightsLeft > 1 ? "s" : "")} left", s.weapon.data.desc, s.weapon.IsLastFight);
            if (s.armour != null) Item($"<b>{s.armour.data.name}</b>  ·  {s.armour.fightsLeft} fights left", s.armour.data.desc, s.armour.IsLastFight);
            foreach (var o in s.skills) Item($"<b>{o.data.name}</b>  ·  lease: {o.daysLeft} day{(o.daysLeft > 1 ? "s" : "")}", o.data.desc, o.IsLastDay);
            if (s.tollDays > 0) Item($"<b>Toll Tax</b>  ·  {s.tollDays} day{(s.tollDays > 1 ? "s" : "")} left", "All stats -15%. A FASTag skips the next one.", true);
            if (y < 10) Row(p, 140, "Nothing yet. Astrologer Baba hands out skills on level up.", 30, null, TextAnchor.MiddleCenter);
        }

        void BuildProfile(RunState s)
        {
            var p = ClearPage("profile");
            Card(p, 8, ContentH - 16, new Color(0, 0, 0, 0.35f));
            var face = UiKit.Panel(p, "Face", Color.white);
            face.sprite = Art.Character("new_kid").sprite;
            face.preserveAspect = true;
            face.rectTransform.Place(40, 20, 200, 250, SW, ContentH);
            float x = 270;
            Text Stat(float y, string label)
            {
                var t = UiKit.Label(p, "Stat", label, 32, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
                t.rectTransform.Place(x, y, SW - x - 30, 44, SW, ContentH);
                return t;
            }
            Stat(20, "New Kid  ·  <color=#FFD23F>Lv " + s.level + "</color>");
            Stat(66, $"EXP {s.xp}/{s.XpToNext}");
            Stat(112, $"HP {s.hp}/{s.maxHp}");
            Stat(158, $"ATK {s.AtkTotal}  <size=24>({s.atk} base{(s.weapon != null ? $" + {s.weapon.data.atk} {s.weapon.data.name}" : "")})</size>");
            Stat(204, $"DEF {s.DefTotal}  <size=24>({s.def} base{(s.armour != null ? $" + {s.armour.data.def} {s.armour.data.name}" : "")})</size>");
            Stat(250, $"<color=#FFD23F>₹{s.rupees}</color>  <size=24>earned ₹{s.earned}, spent ₹{s.spent}</size>");
            var foot = UiKit.Label(p, "Foot", s.HasAunty ? "Party: Pinky Aunty (forwards messages every 3 rounds)" : "Party: just you. For now.", 26, new Color(1, 1, 1, 0.75f), TextAnchor.MiddleLeft);
            foot.rectTransform.Place(40, ContentH - 66, SW - 80, 44, SW, ContentH);
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
            if (run == null || run.Finished) return;
            if (!MapView.Cities.TryGetValue(run.Day.zone, out var c)) return;
            float k = size / 1000f, m = 70;
            var marker = UiKit.Panel(p, "Marker", Color.white);
            marker.sprite = Art.Character("new_kid").sprite;
            marker.preserveAspect = true;
            marker.rectTransform.Place(mx + c.x * k - m / 2, my + c.y * k - m * 0.9f, m, m, SW, ContentH);
            var day = UiKit.Label(p, "Where", $"Day {run.DayNumber} of {run.TotalDays}\n{GameData.ZoneName(run.Day.zone)}", 26, Color.white, TextAnchor.UpperLeft, FontStyle.Bold);
            day.rectTransform.Place(20, 10, mx - 30, 120, SW, ContentH);
            UiKit.Outline(day, new Color(0, 0, 0, 0.6f), 2);
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
            Row(p, 160, "Battle speed: tap the yellow button in the top-right of the fight.", 26, new Color(1, 1, 1, 0.8f), TextAnchor.MiddleCenter);
            Row(p, 220, "<b>bitterChai OS 11.0</b>  ·  valid for 11 months", 26, new Color(1, 1, 1, 0.6f), TextAnchor.MiddleCenter);
            Row(p, 262, "Made by Siddharth Gupta, Dhivesh, Jaideep and Amolika", 26, new Color(1, 1, 1, 0.6f), TextAnchor.MiddleCenter);
        }

        void BuildJhatPatClosed()
        {
            var p = ClearPage("jhatpat");
            var icon = UiKit.Panel(p, "Bhaiya", Color.white);
            icon.sprite = Art.UI("mystery_box");
            icon.preserveAspect = true;
            icon.rectTransform.Place(SW / 2 - 90, 20, 180, 180, SW, ContentH);
            Row(p, 214, "JhatPat opens after today's event.", 34, ShopView.Brand, TextAnchor.MiddleCenter);
            Row(p, 264, "Delivery Wale Bhaiya: \"2 minute, sir.\"", 28, new Color(1, 1, 1, 0.8f), TextAnchor.MiddleCenter);
        }
    }
}
