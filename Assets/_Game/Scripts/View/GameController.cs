using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace KadakChai
{
    /// <summary>
    /// The whole game: builds the portrait UI in code, then runs the 11-day lease as one coroutine.
    /// Put this on an empty GameObject in an otherwise empty scene (Kadak Chai > Build Main Scene does it).
    /// </summary>
    public class GameController : MonoBehaviour
    {
        RectTransform phone;
        Text dayText, aqiText;
        Image aqiBox, zoneIcon;
        Text khatraText;
        readonly Image[] khatraSigns = new Image[GameData.MaxSpice];
        PhoneView phoneView;
        BattleView battle;
        StoryLog log;
        VNPlayer vn;
        Button mainButton, choiceA, choiceB, shopButton;
        GameObject titlePanel;

        Run run;
        int clicked = -1;

        [Tooltip("Debug: start the run on this day (1 = normal). In a WebGL build, add ?day=N to the URL instead.")]
        [SerializeField] int debugStartDay = 1;

        /// <summary>
        /// Press Play in ANY scene (even Unity's SampleScene) and the game starts: if no GameController
        /// is in the scene, one is created. The UI is built in code, so the scene itself can stay empty.
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void AutoStart()
        {
            if (FindAnyObjectByType<GameController>() == null)
                new GameObject("Game (auto-created)").AddComponent<GameController>();
        }

        public static void LoadData()
        {
            GameData.Load(
                file => Resources.Load<TextAsset>("Data/" + Path.GetFileNameWithoutExtension(file))?.text,
                (json, type) => JsonUtility.FromJson(json, type));
        }

        void Start()
        {
            LoadData();
            BuildUI();
            ShowTitle();
        }

        // ============================================================== UI construction

        void BuildUI()
        {
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var es = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
                es.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }

            var canvasGo = new GameObject("Canvas", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = canvasGo.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasGo.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(UiKit.W, UiKit.H);
            scaler.matchWidthOrHeight = 1f;

            var letterbox = UiKit.Panel(canvasGo.transform, "Letterbox", new Color(0.07f, 0.07f, 0.1f));
            letterbox.rectTransform.Fill();

            // The "phone": always 9:16, letterboxed on desktop.
            phone = UiKit.Make("Phone", canvasGo.transform);
            phone.Fill();
            var fitter = phone.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            fitter.aspectRatio = UiKit.W / UiKit.H;
            UiKit.Panel(phone, "Bg", UiKit.Night).rectTransform.Fill();

            // Top bar: zone icon, zone name + day, AQI. (The day timeline was cut: change log 18:39.)
            zoneIcon = UiKit.Panel(phone, "ZoneIcon", Color.white);
            zoneIcon.preserveAspect = true;
            zoneIcon.raycastTarget = false;
            zoneIcon.rectTransform.Place(18, 8, 88, 88);
            dayText = UiKit.Label(phone, "Day", "", 38, Color.white, TextAnchor.MiddleLeft, FontStyle.Bold);
            dayText.rectTransform.Place(116, 8, 450, 88);

            // KHATRA meter (danger): Balatro-style ante, 1/8 to 8/8. Each level makes enemies hit harder.
            var khatra = UiKit.Panel(phone, "Khatra", new Color(0, 0, 0, 0.35f), rounded: true);
            khatra.rectTransform.Place(566, 10, 216, 84);
            khatraText = UiKit.Label(khatra.transform, "T", "", 24, UiKit.Hex("#FFD600"), TextAnchor.UpperCenter, FontStyle.Bold);
            khatraText.rectTransform.Place(0, 2, 216, 34, 216, 84);
            for (int i = 0; i < khatraSigns.Length; i++)
            {
                khatraSigns[i] = UiKit.Panel(khatra.transform, "Sign" + i, Color.white);
                khatraSigns[i].sprite = Art.UI("khatra");
                khatraSigns[i].preserveAspect = true;
                khatraSigns[i].raycastTarget = false;
                khatraSigns[i].rectTransform.Place(6 + i * 25.5f, 38, 28, 40, 216, 84);
            }
            dayText.lineSpacing = 0.85f;
            aqiBox = UiKit.Panel(phone, "Aqi", UiKit.Orange, rounded: true);
            aqiBox.rectTransform.Place(790, 12, 266, 80);
            aqiText = UiKit.Label(aqiBox.transform, "Text", "", 28, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            aqiText.rectTransform.Fill(4);
            UiKit.Outline(aqiText, new Color(0, 0, 0, 0.4f), 2);

            battle = BattleView.Build(phone);

            // The hero's phone (bottom half): status bar, stats, apps; the story log lives in its Messages app.
            phoneView = PhoneView.Build(phone);
            phoneView.OpenShop = () => clicked = 2;
            log = phoneView.Log;

            // Bottom buttons
            mainButton = UiKit.Button(phone, "Main", "", UiKit.Green, 48, () => clicked = 0);
            mainButton.GetComponent<RectTransform>().Place(190, 1736, 700, 150);
            choiceA = UiKit.Button(phone, "ChoiceA", "", UiKit.Purple, 36, () => clicked = 0);
            choiceA.GetComponent<RectTransform>().Place(30, 1736, 500, 150);
            choiceB = UiKit.Button(phone, "ChoiceB", "", UiKit.Orange, 36, () => clicked = 1);
            choiceB.GetComponent<RectTransform>().Place(550, 1736, 500, 150);
            shopButton = UiKit.Button(phone, "JhatPat", "JhatPat", ShopView.Brand, 40, () => clicked = 2);
            shopButton.GetComponent<RectTransform>().Place(30, 1736, 330, 150);
            var shopLabel = shopButton.GetComponentInChildren<Text>();
            shopLabel.color = UiKit.Ink;
            Destroy(shopLabel.GetComponent<Outline>());
            HideButtons();

            // Options: a gear, top-left of the battle area, always reachable during a run.
            var opt = GearButton(phone, "Options");
            opt.GetComponent<RectTransform>().Place(14, 112, 92, 92);

            vn = VNPlayer.Build(phone);
            vn.keepOnTop = opt.transform;
        }

        void ShowTitle()
        {
            // A Bollywood-poster title: pink sky, a spinning sunburst, a chunky KADAK logo with a steaming chai,
            // and the Dilli street below with Bablu facing down the Babu over a No Bachelors board.
            var p = UiKit.Panel(phone, "Title", UiKit.Hex("#C2185B"));
            p.rectTransform.Fill();
            titlePanel = p.gameObject;
            Image Sprite(string name, Sprite sprite, float x, float y, float w, float h, Color? tint = null)
            {
                var img = UiKit.Panel(p.transform, name, tint ?? Color.white);
                img.sprite = sprite;
                img.preserveAspect = true;
                img.raycastTarget = false;
                img.rectTransform.Place(x, y, w, h);
                return img;
            }
            TitleFx Fx(Component c) => c.gameObject.AddComponent<TitleFx>();

            var burst = Sprite("Burst", Art.UI("burst"), -110, -140, 1300, 1300, new Color(1f, 0.82f, 0.2f, 0.5f));
            Fx(burst).spin = 9f;

            var street = Sprite("Street", Art.Background("dilli").sprite, -345, 700, 1771, 1100);
            street.preserveAspect = false;
            var streetFx = Fx(street); streetFx.drift = 140f; streetFx.driftSpeed = 0.12f;

            foreach (var (x, y, size, ph) in new[] { (120f, 250f, 70f, 0f), (900f, 210f, 90f, 0.8f), (160f, 650f, 60f, 1.6f), (930f, 640f, 70f, 2.4f), (520f, 160f, 50f, 1.1f) })
            {
                var star = Sprite("Star", Art.UI("star"), x, y, size, size, new Color(1f, 0.95f, 0.6f, 1f));
                var fx = Fx(star); fx.twinkle = 2.2f; fx.phase = ph; fx.spin = 30f;
            }

            var kadak = UiKit.Label(p.transform, "Kadak", "KADAK", 250, UiKit.Hex("#FFE14D"), TextAnchor.MiddleCenter, FontStyle.Bold);
            kadak.rectTransform.Place(20, 250, 1040, 330);
            kadak.rectTransform.localRotation = Quaternion.Euler(0, 0, 4f);
            var shadow = kadak.gameObject.AddComponent<Shadow>();
            shadow.effectColor = UiKit.Hex("#311B92"); shadow.effectDistance = new Vector2(14, -14);
            UiKit.Outline(kadak, Color.black, 7);
            kadak.gameObject.AddComponent<TextFx>().wave = 7f;
            var kadakFx = Fx(kadak); kadakFx.popDelay = 0.15f; kadakFx.sway = 1.5f;

            var badge = UiKit.Panel(p.transform, "ChaiBadge", UiKit.Hex("#00897B"), rounded: true);
            badge.rectTransform.Place(300, 575, 420, 160);
            badge.rectTransform.localRotation = Quaternion.Euler(0, 0, -5f);
            UiKit.Outline(badge, Color.black, 5);
            var chai = UiKit.Label(badge.transform, "Chai", "Chai", 120, UiKit.Cream, TextAnchor.MiddleCenter, FontStyle.BoldAndItalic);
            chai.rectTransform.Fill(4);
            UiKit.Outline(chai, Color.black, 4);
            var badgeFx = Fx(badge); badgeFx.popDelay = 0.45f; badgeFx.sway = 3f; badgeFx.swaySpeed = 2.2f;

            var cup = Sprite("Cup", Art.UI("item_cutting_chai"), 735, 545, 210, 210);
            var cupFx = Fx(cup); cupFx.popDelay = 0.7f; cupFx.sway = 7f; cupFx.swaySpeed = 2.6f;
            for (int i = 0; i < 3; i++)
            {
                var steam = Sprite("Steam", Art.Dot, 805 + i * 28, 575, 36, 36, new Color(1, 1, 1, 0.85f));
                var fx = Fx(steam); fx.rise = 60f; fx.riseTime = 2.1f; fx.phase = i * 0.7f; fx.drift = 10f; fx.driftSpeed = 3f;
            }

            var ribbon = UiKit.Panel(p.transform, "Ribbon", new Color(0.07f, 0.05f, 0.12f, 0.88f), rounded: true);
            ribbon.rectTransform.Place(80, 775, 920, 170);
            var tag = UiKit.Label(ribbon.transform, "Tag", "Bablu. 11 days. One stamp. <b>No Bachelors.</b>\n<color=#FFE14D><i>Everything is temporary.</i></color>", 44, Color.white);
            tag.rectTransform.Fill(10);

            var hero = Sprite("Bablu", Art.Character("new_kid").sprite, 40, 1035, 350, 440);
            var heroFx = Fx(hero); heroFx.bob = 10f; heroFx.bobSpeed = 3f;
            var babu = Sprite("Babu", Art.Character("babu").sprite, 690, 1035, 350, 440);
            babu.rectTransform.localScale = new Vector3(-1, 1, 1);   // face Bablu
            var babuFx = Fx(babu); babuFx.bob = 6f; babuFx.bobSpeed = 2f; babuFx.phase = 1f;

            var sign = UiKit.Panel(p.transform, "NoBachelors", UiKit.Red, rounded: true);
            sign.rectTransform.Place(395, 1110, 290, 190);
            sign.rectTransform.localRotation = Quaternion.Euler(0, 0, -7f);
            UiKit.Outline(sign, Color.black, 5);
            var signText = UiKit.Label(sign.transform, "T", "NO\nBACHELORS", 50, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            signText.rectTransform.Fill(8);
            var signFx = Fx(sign); signFx.sway = 4f; signFx.swaySpeed = 1.3f;

            var floor = UiKit.Panel(p.transform, "Floor", new Color(0.07f, 0.05f, 0.12f, 0.92f));
            floor.rectTransform.Place(0, 1530, 1080, 390);

            var start = UiKit.Button(p.transform, "Start", "Sign the Agreement", UiKit.Green, 56, () =>
            {
                Destroy(titlePanel);
                StartCoroutine(RunLoop());
            });
            start.GetComponent<RectTransform>().Place(140, 1575, 800, 175);
            UiKit.Outline(start.targetGraphic, Color.black, 5);
            var startFx = Fx(start); startFx.pulse = 0.035f; startFx.pulseSpeed = 4f;

            var metro = UiKit.Label(p.transform, "Metro", "\"Agla station: Temporary. Darwaze baayi taraf khulenge.\"", 32, new Color(1, 1, 1, 0.75f));
            metro.rectTransform.Place(60, 1770, 960, 70);

            var info = Resources.Load<TextAsset>("build_info");
            var ver = UiKit.Label(p.transform, "Build", info != null ? info.text : "editor", 24, new Color(1, 1, 1, 0.35f));
            ver.rectTransform.Place(60, 1850, 960, 50);

            var titleOpt = GearButton(p.transform, "TitleOptions");
            titleOpt.GetComponent<RectTransform>().Place(960, 30, 96, 96);
        }

        // ============================================================== Flow

        IEnumerator RunLoop()
        {
            run = new Run(System.Environment.TickCount);
            phoneView.Bind(run);
            phoneView.ShowApp("messages");
            var oldStamp = phone.Find("Stamp");
            if (oldStamp != null) Destroy(oldStamp.gameObject);
            log.Clear();
            battle.ClearEnemies();
            AudioKit.Music("day");
            RefreshHud();
            battle.SetZone(run.Day.zone, 0);
            battle.SetAqi(run.S.aqi);
            battle.ShowParty(run.S);
            battle.ReviveHero(run.S);
            battle.SetTicker(GameData.RandomCard("news_", run.Rng)?.text ?? "");

            foreach (var c in DebugStatuses()) run.S.conditions.Add(c); // ?status=shaved,confused (testing)
            int startDay = StartDay();
            if (startDay > 1)
            {
                run.DebugSkipTo(startDay);
#if UNITY_EDITOR
                if (File.Exists("Temp/kc_god")) { run.S.atk *= 6; run.S.maxHp *= 4; run.S.hp = run.S.maxHp; } // auto-test only
#endif
                log.Add($"[debug] Skipped to day {startDay}.", CardStyle.Note);
                battle.ShowParty(run.S);
                RefreshHud();
            }
            else
            {
                yield return vn.Play(GameData.Scene("intro"));
                log.Add(CardText("intro"));
                yield return WaitMain("Meet the Astrologer");
                log.Add(CardText("astro_intro"), CardStyle.Event);
                yield return SkillPick();
                log.Add(CardText("jhatpat_intro"), CardStyle.Event);
                run.RestockShop();
                yield return WaitNext("Start Day 1  >");
            }

            while (true)
            {
                yield return PlayDay();
                if (run.Fainted) { yield return NursingHome(); continue; }   // back to yesterday, as a zombie

                bool lastDay = run.DayNumber >= run.TotalDays;
                if (lastDay) { run.EndDay(); break; }

                run.RestockShop();
                var tomorrow = GameData.Days[run.S.dayIndex + 1];
                if (tomorrow.type == "boss")
                {
                    var names = (tomorrow.enemies ?? new string[0]).Select(id => GameData.Combatants.TryGetValue(id, out var c) ? c.name : id);
                    log.Add($"*Tomorrow: BOSS FIGHT* with *{string.Join(", ", names)}*. Heal up and gear up at *JhatPat* now: there is no shop between you and the boss.", CardStyle.Bad);
                    phoneView.ShowApp("messages");
                    yield return WaitNext("Next Day: BOSS  >", UiKit.Red);
                }
                else yield return WaitNext("Next Day  >");
                var expired = run.EndDay();
                foreach (var s in expired)
                {
                    AudioKit.Sfx("expire");
                    log.Add(CardText("expire_", s.name), CardStyle.Bad);
                }
                if (expired.Count > 0)
                    battle.Banner(string.Join(", ", expired.ConvertAll(s => s.shortName)) + (expired.Count > 1 ? " have" : " has") + " *expired*. Everything is temporary.");
                if (run.LeftCompanion != null) { log.Add(CardText("leave_" + run.LeftCompanion), CardStyle.Note); battle.ShowParty(run.S); }
                if (run.Border is { } border)
                {
                    yield return MapView.Cross(phone, border.from, border.to, run.UsedFastag);
                    log.Add(CardText(run.UsedFastag ? "border_fastag" : $"border_{border.from}_{border.to}"), run.UsedFastag ? CardStyle.Good : CardStyle.Bad);
                }
                RefreshHud();
                yield return battle.Walk(run.DayInZone);
            }

            yield return Ending();
        }

        IEnumerator PlayDay()
        {
            bool stageChanged = run.BeginDay();
            phoneView.NewDay();
            phoneView.ShowApp("messages");   // the day's story lands in Messages; don't hide it behind another app
            var stage = Aqi.StageFor(run.S.aqi);
            battle.SetZone(string.IsNullOrEmpty(run.Day.bg) ? run.Day.zone : run.Day.bg, run.DayInZone);
            battle.SetAqi(run.S.aqi);
            RefreshHud();

            log.Add($"Day {run.DayNumber} of {run.TotalDays}  ·  {GameData.ZoneName(run.Day.zone)}  ·  Khatra {run.Spice}/8", CardStyle.Header);
            if (run.JoinedCompanion != null)
            {
                battle.ShowParty(run.S);
                yield return vn.Play(GameData.Scene(run.JoinedCompanion + "_join"));
                log.Add(CardText("join_" + run.JoinedCompanion), CardStyle.Good);
            }
            if (run.DayNumber > 1 && run.Spice > GameData.Days[run.S.dayIndex - 1].spice)
            {
                AudioKit.Sfx("boom", 0.6f);
                battle.Banner($"*KHATRA {run.Spice}/8: {GameData.SpiceName(run.Spice).ToUpper()}*  Enemies hit harder.");
            }
            if (run.RobbedLoss > 0) { AudioKit.Sfx("coin", 0.5f); log.Add($"*Robbed*: ₹{run.RobbedLoss} missing from your wallet this morning. Cure it with *Pudin Hara* at JhatPat.", CardStyle.Bad); }
            if (run.Day.rent > 0)
            {
                AudioKit.Sfx("coin");
                log.Add(CardText("rent_day").Replace("{rs}", run.RentPaid.ToString()), CardStyle.Bad);
            }
            if (stageChanged) battle.SetTicker(GameData.RandomCard("news_", run.Rng)?.text ?? "");

            var card = run.IntroCard;
            if (run.IsFightDay)
            {
                var enemies = run.Enemies();
                yield return battle.ShowEnemies(enemies);
                foreach (var e in enemies) AudioKit.Bark(e.id);
                yield return vn.Play(GameData.Scene(run.Day.scene));
                yield return battle.VsIntro(enemies, run.Kind == DayKind.Boss);
                log.Add(card?.text);
                if (stageChanged || stage >= GrapStage.Grap3) log.Add($"AQI {run.S.aqi}. {Aqi.RuleText(stage)}", CardStyle.Note);

                string label = run.Kind == DayKind.Boss ? "Face the Boss" : "Fight!";
                while (true)
                {
                    yield return WaitMain(label, run.Kind == DayKind.Boss ? UiKit.Red : UiKit.Green);
                    AudioKit.Music(run.S.hp < run.S.maxHp * 0.5f ? "techno" : "battle");
                    int opener = 0;
                    yield return KadakMeter(v => opener = v);
                    var result = run.Fight(opener);
                    battle.HeroHp = phoneView.ShowLiveHp;
                    yield return battle.Play(result);
                    battle.HeroHp = null;
                    foreach (var c in result.inflicted)
                        log.Add($"*{GameData.HeroName}* is now *{Conditions.Name(c)}*: {Conditions.All[c].desc} It lasts until you drink *Pudin Hara* (JhatPat).", CardStyle.Bad);
                    AudioKit.Music("day");

                    if (result.won)
                    {
                        AudioKit.Sfx("win");
                        var win = GameData.CardOrNull(run.WinCardId) ?? GameData.RandomCard("win_", run.Rng);
                        log.Add(win?.text, CardStyle.Good);
                        if (result.xpGained > 0 || result.rupeesGained > 0) { AudioKit.Sfx("coin", 0.7f); log.Add($"+{result.xpGained} EXP   ·   +₹{result.rupeesGained}", CardStyle.Note); }
                        foreach (var it in result.broken)
                        {
                            AudioKit.Sfx("expire");
                            log.Add(it.breakText, CardStyle.Bad);
                            battle.Banner($"*{it.name}* broke. Everything is temporary.");
                        }
                        if (result.broken.Count > 0) battle.SetHeld(run.S);
                        if (run.DayNumber < run.TotalDays) yield return vn.Play(GameData.Scene(run.Day.outro));
                        battle.ClearEnemies();
                        break;
                    }

                    if (result.revived)
                    {
                        // Health Insurance: one revive at 50% HP, then the same fight again.
                        AudioKit.Sfx("heal");
                        log.Add(CardText("revive"), CardStyle.Good);
                        battle.Banner("*Health Insurance claimed!* Back at 50% HP.");
                        RefreshHud();
                        yield return battle.ResetForRetry(run.S, enemies);
                        label = "Fight again";
                        continue;
                    }
                    // Dead. The run is over.
                    AudioKit.Music(null);
                    AudioKit.Sfx("lose");
                    log.Add(CardText("lose_"), CardStyle.Bad);
                    RefreshHud();
                    yield break;
                }
            }
            else
            {
                yield return vn.Play(GameData.Scene(run.Day.scene));
                log.Add(card?.text, CardStyle.Event);
                if (card != null && card.HasChoices)
                {
                    yield return WaitChoice(card.choiceA, card.choiceB);
                    var (text, notes) = run.Choose(clicked);
                    log.Add(text, CardStyle.Good);
                    if (notes.Count > 0) log.Add(string.Join("   ·   ", notes), CardStyle.Note);
                    battle.ShowParty(run.S);
                    battle.SetAqi(run.S.aqi);
                }
                else yield return WaitMain("Continue");
            }

            RefreshHud();
            for (int n = run.ConsumeLevelUps(); n > 0; n--)
            {
                AudioKit.Sfx("levelup");
                battle.Banner($"*LEVEL UP!*  Lv {run.S.level - n + 1}");
                battle.Celebrate();
                log.Add($"Level up! You are now Lv {run.S.level - n + 1}.", CardStyle.Note);
                log.Add(CardText("baba_"), CardStyle.Event);
                RefreshHud();
                yield return new WaitForSecondsRealtime(1.2f);   // let the level-up banner and confetti finish
                yield return SkillPick();
            }
            battle.RefreshHero(run.S);
        }

        IEnumerator Ending()
        {
            HideButtons();
            yield return Stamp();
            yield return vn.Play(GameData.Scene("ending"));
            log.Add(CardText("delivery_end"));
            yield return WaitMain("Read the Challan");
            ShowCredits();
        }

        // ============================================================== Overlays

        /// <summary>
        /// Kadak opener: one tap before every fight. Stop the needle in the dark middle for KADAK chai
        /// (+40% ATK for 3 rounds), near it for theek-thaak (+15% for 2), anywhere else for pheeka (nothing).
        /// Reports 2, 1 or 0. No tap within 4 seconds counts as pheeka, so it can never block the game.
        /// </summary>
        IEnumerator KadakMeter(System.Action<int> done)
        {
            HideButtons();
            bool tapped = false;
            var overlay = UiKit.Panel(phone, "KadakMeter", new Color(0, 0, 0, 0.45f));
            overlay.rectTransform.Fill();
            var catcher = overlay.gameObject.AddComponent<Button>();
            catcher.transition = Selectable.Transition.None;
            catcher.onClick.AddListener(() => tapped = true);

            var card = UiKit.Panel(overlay.transform, "Card", UiKit.Cream, rounded: true);
            card.rectTransform.Place(60, 1150, 960, 560);
            card.raycastTarget = false;
            UiKit.Outline(card, Color.black, 5);
            var head = UiKit.Label(card.transform, "Head", "Brew it <color=#6D3B1F>KADAK!</color>", 64, UiKit.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            head.rectTransform.Place(20, 24, 920, 90, 960, 560);
            var sub = UiKit.Label(card.transform, "Sub", "Tap anywhere when the glass hits the dark middle", 34, UiKit.Ink);
            sub.rectTransform.Place(20, 112, 920, 60, 960, 560);

            var bar = UiKit.Panel(card.transform, "Bar", UiKit.Hex("#F3E1B9"), rounded: true);
            bar.rectTransform.Place(70, 220, 820, 110, 960, 560);
            bar.raycastTarget = false;
            UiKit.Outline(bar, Color.black, 4);
            Image Zone(string name, float half, Color c)
            {
                var z = UiKit.Panel(bar.transform, name, c);
                z.raycastTarget = false;
                z.rectTransform.anchorMin = new Vector2(0.5f - half, 0.08f);
                z.rectTransform.anchorMax = new Vector2(0.5f + half, 0.92f);
                z.rectTransform.offsetMin = z.rectTransform.offsetMax = Vector2.zero;
                return z;
            }
            Zone("Theek", 0.2f, UiKit.Hex("#D9A066"));
            Zone("Kadak", 0.08f, UiKit.Hex("#6D3B1F"));
            var tick = UiKit.Panel(bar.transform, "Glass", Color.white);
            tick.sprite = Art.UI("item_cutting_chai");
            tick.preserveAspect = true;
            tick.raycastTarget = false;
            var trt = tick.rectTransform;
            trt.sizeDelta = new Vector2(120, 150);
            var labels = UiKit.Label(card.transform, "Zones", "<color=#A1887F>pheeka</color>          <color=#B26A2E>theek</color>   <color=#6D3B1F><b>KADAK</b></color>   <color=#B26A2E>theek</color>          <color=#A1887F>pheeka</color>", 30, UiKit.Ink);
            labels.rectTransform.Place(40, 350, 880, 50, 960, 560);
            var result = UiKit.Label(card.transform, "Result", "", 72, UiKit.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            result.rectTransform.Place(20, 420, 920, 110, 960, 560);

            float t = 0, x = 0;
            while (!tapped && t < 4f)
            {
                t += Time.unscaledDeltaTime;
                x = Mathf.PingPong(t * 1.15f, 1f);
                trt.anchorMin = trt.anchorMax = new Vector2(x, 0.5f);
                trt.anchoredPosition = Vector2.zero;
                yield return null;
            }
            float off = Mathf.Abs(x - 0.5f);
            int v = !tapped ? 0 : off <= 0.08f ? 2 : off <= 0.2f ? 1 : 0;
            result.text = v == 2 ? "<color=#6D3B1F>KADAK!</color>" : v == 1 ? "<color=#B26A2E>Theek hai.</color>" : tapped ? "<color=#8D6E63>Pheeka...</color>" : "<color=#8D6E63>Chai got cold.</color>";
            AudioKit.Sfx(v == 2 ? "levelup" : v == 1 ? "coin" : "click", 0.8f);
            if (v == 2) { battle.Celebrate(); battle.Banner("*KADAK!*  +40% ATK for 3 rounds"); }
            yield return new WaitForSecondsRealtime(0.8f);
            Destroy(overlay.gameObject);
            done(v);
        }

        IEnumerator SkillPick()
        {
            var offer = run.S.OfferSkills(3, run.Rng);
            if (offer.Count == 0) yield break;
            HideButtons();

            var overlay = UiKit.Panel(phone, "SkillPick", new Color(0, 0, 0, 0.8f));
            overlay.rectTransform.Fill();
            var head = UiKit.Label(overlay.transform, "Head", "Astrologer Baba offers a blessing.\n<size=34>Pick fast. Blessings are temporary. Some are fake.</size>", 50, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            head.rectTransform.Place(40, 230, 1000, 220);

            // 10-second timer: Baba doesn't wait (change log).
            const float Patience = 10f;
            var barBg = UiKit.Panel(overlay.transform, "TimerBg", new Color(1, 1, 1, 0.15f), rounded: true);
            barBg.rectTransform.Place(140, 470, 800, 34);
            var bar = UiKit.Panel(barBg.transform, "Timer", UiKit.Orange, rounded: true);
            bar.rectTransform.anchorMin = Vector2.zero; bar.rectTransform.anchorMax = Vector2.one;
            bar.rectTransform.offsetMin = bar.rectTransform.offsetMax = Vector2.zero;
            var timerText = UiKit.Label(overlay.transform, "TimerText", "", 30, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            timerText.rectTransform.Place(140, 504, 800, 40);

            int picked = -1;
            for (int i = 0; i < offer.Count; i++)
            {
                int idx = i;
                var s = offer[i];
                var b = UiKit.Button(overlay.transform, "Skill" + i, "", UiKit.Cream, 34, () => picked = idx);
                b.GetComponent<RectTransform>().Place(60, 570 + i * 330, 960, 300);
                var icon = UiKit.Panel(b.transform, "Icon", Color.white);
                icon.sprite = Art.UI("skill_" + s.id);
                icon.preserveAspect = true;
                icon.raycastTarget = false;
                icon.rectTransform.Place(24, 40, 220, 220, 960, 300);
                if (icon.sprite == null) icon.enabled = false;
                var label = b.GetComponentInChildren<Text>();
                label.color = UiKit.Ink;
                label.fontStyle = FontStyle.Normal;
                label.alignment = TextAnchor.MiddleLeft;
                label.rectTransform.offsetMin = new Vector2(icon.enabled ? 262 : 30, 12);
                Destroy(label.GetComponent<Outline>());
                label.text = $"<b><size=44>{s.name}</size></b>\n{s.desc}\n<color=#E8590C><b>Lease: {s.lease} days</b></color>";
            }
            // pop the cards in one after another
            for (int i = 0; i < offer.Count; i++)
            {
                var rt = overlay.transform.Find("Skill" + i);
                rt.localScale = Vector3.zero;
            }
            for (int i = 0; i < offer.Count; i++)
            {
                var rt = overlay.transform.Find("Skill" + i);
                for (float t = 0; t < 1; t += Time.unscaledDeltaTime * 7f)
                {
                    rt.localScale = Vector3.one * (t < 0.7f ? Mathf.Lerp(0f, 1.08f, t / 0.7f) : Mathf.Lerp(1.08f, 1f, (t - 0.7f) / 0.3f));
                    yield return null;
                }
                rt.localScale = Vector3.one;
                AudioKit.Sfx("card", 0.6f);
            }
            // A beat to read the cards before Baba's patience starts running out.
            timerText.text = "Baba is reading your palm...";
            for (float g = 0; g < 1.5f && picked < 0; g += Time.unscaledDeltaTime) yield return null;
            float left = Patience;
            int lastTick = -1;
            while (picked < 0 && left > 0)
            {
                left -= Time.unscaledDeltaTime;
                bar.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(left / Patience), 1);
                bar.color = left > 3 ? UiKit.Orange : UiKit.Red;
                int sec = Mathf.CeilToInt(left);
                timerText.text = $"Baba leaves in {sec}";
                if (sec != lastTick && sec <= 3) { lastTick = sec; AudioKit.Sfx("blip_hi", 0.7f); }
                yield return null;
            }

            if (picked < 0)
            {
                AudioKit.Sfx("expire");
                yield return BlessingReveal(overlay.rectTransform, null, "GONE!", new Color(0.7f, 0.7f, 0.75f));
                log.Add("*Astrologer Baba* got bored and left. \"Destiny waits for no one, beta.\" No blessing.", CardStyle.Bad);
            }
            else
            {
                var skill = offer[picked];
                var card = overlay.transform.Find("Skill" + picked) as RectTransform;
                switch (run.TakeBlessing(skill))
                {
                    case Blessing.Granted:
                        AudioKit.Sfx("levelup", 0.7f);
                        yield return BlessingReveal(overlay.rectTransform, card, "BLESSED!", UiKit.Green);
                        log.Add($"New skill: *{skill.name}* (lease: {skill.lease} days)", CardStyle.Good);
                        break;
                    case Blessing.Fizzled:
                        AudioKit.Sfx("lose", 0.6f);
                        yield return BlessingReveal(overlay.rectTransform, card, "FIZZLED...", new Color(0.7f, 0.7f, 0.75f));
                        log.Add($"The *{skill.name}* blessing fizzles. Baba: \"Stars are cloudy today. No refunds.\"", CardStyle.Bad);
                        break;
                    case Blessing.Backfired:
                        AudioKit.Sfx("boom");
                        battle.Shake(18, 0.3f);
                        yield return BlessingReveal(overlay.rectTransform, card, "BACKFIRED!", UiKit.Red);
                        log.Add($"The *{skill.name}* blessing *backfires*. -{Mathf.RoundToInt(Run.BackfireHp * 100)}% HP, and Baba keeps ₹{Run.Dakshina} as dakshina.", CardStyle.Bad);
                        break;
                }
            }
            Destroy(overlay.gameObject);
            RefreshHud();
        }

        /// <summary>Big comic word over the picked card (or the middle of the screen).</summary>
        IEnumerator BlessingReveal(RectTransform overlay, RectTransform card, string word, Color color)
        {
            var t = UiKit.Label(overlay, "Reveal", word, 120, color, TextAnchor.MiddleCenter, FontStyle.Bold);
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            UiKit.Outline(t, UiKit.Ink, 6);
            t.gameObject.AddComponent<TextFx>().wave = 6f;
            if (card != null) t.rectTransform.Place(0, (1 - card.anchorMax.y) * UiKit.H + 50, 1080, 200); // over the picked card
            else t.rectTransform.Place(0, 900, 1080, 200);
            t.rectTransform.localRotation = Quaternion.Euler(0, 0, -6);
            for (float k = 0; k < 1; k += Time.unscaledDeltaTime * 5f) { t.rectTransform.localScale = Vector3.one * Mathf.Lerp(2.5f, 1f, k); yield return null; }
            t.rectTransform.localScale = Vector3.one;
            yield return new WaitForSecondsRealtime(0.9f);
        }

        IEnumerator Stamp()
        {
            AudioKit.Sfx("stamp");
            var box = UiKit.Panel(phone, "Stamp", new Color(0, 0, 0, 0), rounded: true);
            box.rectTransform.Place(140, 420, 800, 360);
            var frame = UiKit.Panel(box.transform, "Frame", new Color(0.8f, 0.1f, 0.1f, 0.15f), rounded: true);
            frame.rectTransform.Fill();
            var t = UiKit.Label(box.transform, "Text", "VALID FOR\n11 MONTHS", 96, new Color(0.85f, 0.1f, 0.1f), TextAnchor.MiddleCenter, FontStyle.Bold);
            t.rectTransform.Fill(10);
            UiKit.Outline(t, new Color(0.4f, 0, 0, 0.6f), 4);
            box.rectTransform.localRotation = Quaternion.Euler(0, 0, -12);
            for (float k = 0; k < 1; k += Time.deltaTime * 5f)
            {
                box.rectTransform.localScale = Vector3.one * Mathf.Lerp(3f, 1f, k);
                yield return null;
            }
            box.rectTransform.localScale = Vector3.one;
            yield return new WaitForSeconds(1.5f);
        }

        /// <summary>
        /// Bablu is dead. The whole screen goes dark like his phone battery just died (change log: "phone died"),
        /// then one readable card: who killed him, where, how far he got. Back to Day 1.
        /// </summary>
        IEnumerator GameOver()
        {
            HideButtons();
            yield return new WaitForSeconds(0.8f);

            // 1. The phone dies: screen collapses to a line, then black.
            var p = UiKit.Panel(phone, "GameOver", Color.black);
            p.rectTransform.Fill();
            var group = p.gameObject.AddComponent<CanvasGroup>();
            var line = UiKit.Panel(p.transform, "ScreenOff", Color.white);
            line.raycastTarget = false;
            AudioKit.Sfx("expire");
            for (float k = 0; k < 1; k += Time.deltaTime * 3f)
            {
                group.alpha = Mathf.Clamp01(k * 2f);
                float h = Mathf.Lerp(1920, 6, Mathf.SmoothStep(0, 1, k));
                line.rectTransform.Place(0, 960 - h / 2, 1080, h);
                line.color = new Color(1, 1, 1, 1 - k * 0.6f);
                yield return null;
            }
            group.alpha = 1;
            for (float k = 0; k < 1; k += Time.deltaTime * 4f) { line.rectTransform.Place(540 * k, 957, 1080 * (1 - k), 6); yield return null; }
            Destroy(line.gameObject);

            // 2. Empty battery, blinking.
            var batt = UiKit.Panel(p.transform, "Battery", Color.clear);
            batt.rectTransform.Place(390, 180, 260, 120);
            UiKit.Outline(batt, new Color(1, 1, 1, 0.85f), 8);
            var nub = UiKit.Panel(p.transform, "Nub", new Color(1, 1, 1, 0.85f));
            nub.rectTransform.Place(656, 215, 22, 50);
            var sliver = UiKit.Panel(batt.transform, "Sliver", UiKit.Red);
            sliver.rectTransform.Place(14, 14, 26, 92, 260, 120);
            var pct = UiKit.Label(p.transform, "Pct", "0%", 44, UiKit.Red, TextAnchor.MiddleCenter, FontStyle.Bold);
            pct.rectTransform.Place(340, 310, 400, 60);
            for (int i = 0; i < 3; i++)
            {
                sliver.enabled = false; yield return new WaitForSeconds(0.18f);
                sliver.enabled = true; AudioKit.Sfx("blip_lo", 0.6f); yield return new WaitForSeconds(0.22f);
            }

            // 3. The card.
            var card = UiKit.Panel(p.transform, "Card", UiKit.Cream, rounded: true);
            card.rectTransform.Place(70, 420, 940, 1060);
            UiKit.Outline(card, UiKit.Hex("#5D4037"), 5);
            const float CW = 940, CH = 1060;
            var stamp = UiKit.Panel(card.transform, "Stamp", new Color(0.78f, 0.08f, 0.08f, 1f), rounded: true);
            stamp.rectTransform.Place(110, 40, 720, 190, CW, CH);
            stamp.rectTransform.localRotation = Quaternion.Euler(0, 0, -4);
            var st = UiKit.Label(stamp.transform, "T", "LEASE TERMINATED", 72, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            st.rectTransform.Fill(10);
            UiKit.Outline(st, new Color(0.3f, 0, 0, 0.8f), 4);

            var s = run.S;
            var lines = new (string text, int size, Color color)[]
            {
                ($"{GameData.HeroName}'s phone has died.", 46, UiKit.Ink),
                ($"Day {run.DayNumber} of {run.TotalDays}  ·  {GameData.ZoneName(run.Day.zone)}", 38, UiKit.Hex("#5D4037")),
                ($"Khatra {run.Spice}/8 ({GameData.SpiceName(run.Spice)})", 38, UiKit.Hex("#E8590C")),
                ($"Killed by <b>{run.Killer}</b>", 44, UiKit.Red),
                ($"Level {s.level}   ·   ₹{s.earned} earned   ·   {s.skills.Count} skill{(s.skills.Count == 1 ? "" : "s")} on lease", 32, UiKit.Hex("#6D4C41")),
            };
            float y = 270;
            foreach (var (text, size, color) in lines)
            {
                var t = UiKit.Label(card.transform, "Line", text, size, color, TextAnchor.MiddleCenter, size >= 44 ? FontStyle.Bold : FontStyle.Normal);
                t.rectTransform.Place(40, y, CW - 80, size + 30, CW, CH);
                y += size + 40;
            }
            var quote = UiKit.Label(card.transform, "Quote", "<i>" + UiKit.Rich(CardText("gameover_")) + "</i>", 36, UiKit.Ink, TextAnchor.MiddleCenter);
            quote.rectTransform.Place(60, y + 10, CW - 120, 160, CW, CH);
            quote.lineSpacing = 1.15f;

            AudioKit.Sfx("stamp");
            for (float k = 0; k < 1; k += Time.deltaTime * 4f)
            {
                card.rectTransform.anchoredPosition = new Vector2(0, -300 * (1 - k) * (1 - k));
                stamp.rectTransform.localScale = Vector3.one * Mathf.Lerp(2.2f, 1f, k);
                yield return null;
            }
            card.rectTransform.anchoredPosition = Vector2.zero;
            stamp.rectTransform.localScale = Vector3.one;

            bool again = false;
            UiKit.Button(p.transform, "TryAgain", "Charge phone & sign a new lease", UiKit.Green, 44, () => again = true)
                .GetComponent<RectTransform>().Place(110, 1560, 860, 160);
            while (!again) yield return null;
            Destroy(p.gameObject);
            StartCoroutine(RunLoop());
        }

        /// <summary>
        /// Bablu fainted (change log 04 Oct): he wakes at Amma's Nursing Home, the Ayurvedic sadhu gives him
        /// jadi-booti (Zombie buff for the next fight), Amma takes ₹100, and he goes back to yesterday morning.
        /// </summary>
        IEnumerator NursingHome()
        {
            HideButtons();
            yield return new WaitForSeconds(1.2f);
            string killer = run.Killer;
            run.WakeAtNursingHome();
            yield return vn.Play(GameData.Scene("nursing_home"));
            AudioKit.Sfx("heal");
            AudioKit.Music("day");
            log.Add($"Knocked out by *{killer}*. *{GameData.HeroName}* wakes up at *Amma's Nursing Home*. Amma's bill: *₹{Run.NursingHomeBill}*. The Vaidya's jadi-booti cures every status and makes him a *Zombie* for the next fight.", CardStyle.Bad);
            log.Add($"*Amma's blessing:* enemies are *{run.MercyPercent}% weaker* until {GameData.HeroName} gets past Day {run.S.furthestDay + 1}. \"Kha le beta, phir ladna.\"", CardStyle.Good);
            battle.ClearEnemies();
            battle.ReviveHero(run.S);
            battle.ShowParty(run.S);
            battle.SetZone(string.IsNullOrEmpty(run.Day.bg) ? run.Day.zone : run.Day.bg, run.DayInZone);
            RefreshHud();
            battle.Banner($"*JADI-BOOTI ZOMBIE!* Back to Day {run.DayNumber}.");
            yield return WaitMain($"Back to Day {run.DayNumber}  >", UiKit.Hex("#7CB342"));
        }

        void ShowCredits()
        {
            var p = UiKit.Panel(phone, "Credits", new Color(0, 0, 0, 0.85f));
            p.rectTransform.Fill();
            var paper = UiKit.Panel(p.transform, "Challan", UiKit.Cream, rounded: true);
            paper.rectTransform.Place(80, 240, 920, 1150);
            var head = UiKit.Label(paper.transform, "Head", "TRAFFIC CHALLAN", 64, UiKit.Red, TextAnchor.MiddleCenter, FontStyle.Bold);
            head.rectTransform.Place(20, 30, 880, 110, 920, 1150);
            var body = UiKit.Label(paper.transform, "Body", UiKit.Rich(CardText("credits")), 38, UiKit.Ink, TextAnchor.UpperLeft);
            body.rectTransform.Place(60, 170, 800, 900, 920, 1150);
            body.lineSpacing = 1.3f;

            UiKit.Button(p.transform, "Again", "Renew Lease (Play Again)", UiKit.Green, 44, () =>
            {
                Destroy(p.gameObject);
                StartCoroutine(RunLoop());
            }).GetComponent<RectTransform>().Place(140, 1500, 800, 160);
        }

        Button GearButton(Transform parent, string name)
        {
            var b = UiKit.Button(parent, name, "", new Color(0, 0, 0, 0.45f), 30, ShowOptions);
            var icon = UiKit.Panel(b.transform, "Gear", Color.white);
            icon.sprite = Art.UI("gear");
            icon.preserveAspect = true;
            icon.raycastTarget = false;
            icon.rectTransform.Fill(10);
            if (icon.sprite == null) b.SetText("Options");
            return b;
        }

        /// <summary>Options panel: Sound FX and Music on/off (saved between visits).</summary>
        void ShowOptions()
        {
            if (phone.Find("OptionsPanel") != null) return;
            var dim = UiKit.Panel(phone, "OptionsPanel", new Color(0, 0, 0, 0.75f));
            dim.rectTransform.Fill();
            dim.transform.SetAsLastSibling();

            var box = UiKit.Panel(dim.transform, "Box", UiKit.Night, rounded: true);
            box.rectTransform.Place(140, 560, 800, 760);
            UiKit.Outline(box, UiKit.Orange, 5);
            var title = UiKit.Label(box.transform, "Title", "Options", 64, UiKit.Orange, TextAnchor.MiddleCenter, FontStyle.Bold);
            title.rectTransform.Place(0, 40, 800, 100, 800, 760);

            Button sfxBtn = null, musicBtn = null;
            void Refresh()
            {
                sfxBtn.SetText(AudioKit.SfxOn ? "Sound FX: ON" : "Sound FX: OFF");
                sfxBtn.targetGraphic.color = AudioKit.SfxOn ? UiKit.Green : new Color(0.4f, 0.4f, 0.45f);
                musicBtn.SetText(AudioKit.MusicOn ? "Music: ON" : "Music: OFF");
                musicBtn.targetGraphic.color = AudioKit.MusicOn ? UiKit.Green : new Color(0.4f, 0.4f, 0.45f);
            }
            sfxBtn = UiKit.Button(box.transform, "SfxToggle", "", UiKit.Green, 46, () => { AudioKit.SfxOn = !AudioKit.SfxOn; Refresh(); AudioKit.Sfx("click"); });
            sfxBtn.GetComponent<RectTransform>().Place(100, 190, 600, 140, 800, 760);
            musicBtn = UiKit.Button(box.transform, "MusicToggle", "", UiKit.Green, 46, () => { AudioKit.MusicOn = !AudioKit.MusicOn; Refresh(); });
            musicBtn.GetComponent<RectTransform>().Place(100, 360, 600, 140, 800, 760);
            var close = UiKit.Button(box.transform, "OptionsClose", "Close", UiKit.Orange, 46, () => Destroy(dim.gameObject));
            close.GetComponent<RectTransform>().Place(200, 560, 400, 130, 800, 760);
            Refresh();
        }

        // ============================================================== HUD

        void RefreshHud()
        {
            var s = run.S;
            var stage = Aqi.StageFor(s.aqi);
            dayText.text = run.Finished ? "Lease complete" : $"{GameData.ZoneShort(run.Day.zone)}\n<size=28><color=#FFFFFFAA>Day {run.DayNumber} of {run.TotalDays}</color></size>";
            var zi = run.Finished ? null : Art.UI("zone_" + run.Day.zone);
            zoneIcon.sprite = zi;
            zoneIcon.enabled = zi != null;
            aqiText.text = $"AQI {s.aqi}\n<size=22>{Aqi.StageName(stage)}</size>";
            int spice = run.Finished ? GameData.MaxSpice : run.Spice;
            khatraText.text = $"KHATRA {spice}/8";
            for (int i = 0; i < khatraSigns.Length; i++) khatraSigns[i].color = i < spice ? Color.white : new Color(0.22f, 0.22f, 0.26f, 0.6f);
            var (r, g, b) = Aqi.Color(stage);
            aqiBox.color = new Color(r, g, b);
            phoneView.Refresh();
            battle.SetConditions(s);
            battle.SetZombie(s.zombieFights > 0);
            battle.RefreshHero(s);
            battle.SetHeld(s);
        }

        // ============================================================== Buttons

        void HideButtons()
        {
            mainButton.gameObject.SetActive(false);
            choiceA.gameObject.SetActive(false);
            choiceB.gameObject.SetActive(false);
            shopButton.gameObject.SetActive(false);
            mainButton.GetComponent<RectTransform>().Place(190, 1736, 700, 150);
        }

        /// <summary>Main button plus the JhatPat button beside it. Opening the shop doesn't advance the day.</summary>
        IEnumerator WaitNext(string label, Color? color = null)
        {
            while (true)
            {
                HideButtons();
                mainButton.SetText(label);
                mainButton.targetGraphic.color = color ?? UiKit.Green;
                mainButton.GetComponent<RectTransform>().Place(380, 1736, 670, 150);
                mainButton.gameObject.SetActive(true);
                shopButton.SetText($"JhatPat\n<size=30>₹{run.S.rupees}</size>");
                shopButton.gameObject.SetActive(true);
                clicked = -1;
                phoneView.ShopReady = true;
                phoneView.Refresh();
                while (clicked < 0) yield return null;
                phoneView.ShopReady = false;
                phoneView.Refresh();
                HideButtons();
                if (clicked != 2) yield break;
                yield return ShopView.Open(phone, run, () => { RefreshHud(); battle.ShowParty(run.S); }, (t, st) => log.Add(t, st));
            }
        }

        IEnumerator WaitMain(string label, Color? color = null)
        {
            HideButtons();
            mainButton.SetText(label);
            mainButton.targetGraphic.color = color ?? UiKit.Green;
            mainButton.gameObject.SetActive(true);
            clicked = -1;
            while (clicked < 0) yield return null;
            HideButtons();
        }

        IEnumerator WaitChoice(string a, string b)
        {
            HideButtons();
            choiceA.SetText(a);
            choiceB.SetText(b);
            choiceA.gameObject.SetActive(true);
            choiceB.gameObject.SetActive(!string.IsNullOrEmpty(b));
            clicked = -1;
            while (clicked < 0) yield return null;
            HideButtons();
        }

        /// <summary>Testing: ?status=shaved,confused in the WebGL URL starts the run with those statuses.</summary>
        static IEnumerable<string> DebugStatuses()
        {
#if UNITY_WEBGL && !UNITY_EDITOR
            var m = System.Text.RegularExpressions.Regex.Match(Application.absoluteURL ?? "", @"[?&]status=([a-z_,]+)");
            if (m.Success) foreach (var id in m.Groups[1].Value.Split(',')) if (Conditions.All.ContainsKey(id)) yield return id;
#endif
            yield break;
        }

        int StartDay()
        {
            int d = debugStartDay;
#if UNITY_EDITOR
            // Editor auto-tests can pick the start day without touching the scene.
            if (File.Exists("Temp/kc_start_day") && int.TryParse(File.ReadAllText("Temp/kc_start_day").Trim(), out var fd)) d = fd;
#endif
#if UNITY_WEBGL && !UNITY_EDITOR
            var m = System.Text.RegularExpressions.Regex.Match(Application.absoluteURL ?? "", @"[?&]day=(\d+)");
            if (m.Success) d = int.Parse(m.Groups[1].Value);
#endif
            return Mathf.Clamp(d, 1, GameData.Days.Count);
        }

        /// <summary>Card text by exact id, or a random card when the id ends with "_" (a pool prefix).</summary>
        string CardText(string idOrPrefix, string skill = null)
        {
            var c = idOrPrefix.EndsWith("_") ? GameData.RandomCard(idOrPrefix, run?.Rng ?? new System.Random()) : GameData.CardOrNull(idOrPrefix);
            var text = c?.text ?? "";
            return skill != null ? text.Replace("{skill}", skill) : text;
        }
    }
}
