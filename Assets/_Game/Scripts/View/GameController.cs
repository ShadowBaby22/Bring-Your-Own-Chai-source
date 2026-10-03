using System.Collections;
using System.Collections.Generic;
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
        Text theekaText;
        readonly Image[] chilies = new Image[GameData.MaxSpice];
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

            // Theeka meter: Balatro-style ante, 1/8 to 8/8. Each level makes enemies hit harder.
            var theeka = UiKit.Panel(phone, "Theeka", new Color(0, 0, 0, 0.35f), rounded: true);
            theeka.rectTransform.Place(566, 10, 216, 84);
            theekaText = UiKit.Label(theeka.transform, "T", "", 24, UiKit.Hex("#FF8A65"), TextAnchor.UpperCenter, FontStyle.Bold);
            theekaText.rectTransform.Place(0, 2, 216, 34, 216, 84);
            for (int i = 0; i < chilies.Length; i++)
            {
                chilies[i] = UiKit.Panel(theeka.transform, "Chili" + i, Color.white);
                chilies[i].sprite = Art.UI("chili");
                chilies[i].preserveAspect = true;
                chilies[i].raycastTarget = false;
                chilies[i].rectTransform.Place(8 + i * 25, 36, 26, 42, 216, 84);
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
            var p = UiKit.Panel(phone, "Title", new Color(0.08f, 0.08f, 0.12f, 1f));
            p.rectTransform.Fill();
            titlePanel = p.gameObject;

            var title = UiKit.Label(p.transform, "Title", "GRAP STAGE 4", 120, UiKit.Orange, TextAnchor.MiddleCenter, FontStyle.Bold);
            title.rectTransform.Place(40, 420, 1000, 300);
            title.gameObject.AddComponent<TextFx>().wave = 6f;
            UiKit.Outline(title, Color.black, 5);
            var sub = UiKit.Label(p.transform, "Sub", "Bablu. 11 days. One stamp. No Bachelors.\n<i>Everything is temporary.</i>", 44, Color.white);
            sub.rectTransform.Place(60, 740, 960, 200);
            var metro = UiKit.Label(p.transform, "Metro", "\"Agla station: Temporary. Darwaze baayi taraf khulenge.\"", 32, new Color(1, 1, 1, 0.6f));
            metro.rectTransform.Place(60, 1000, 960, 120);

            var info = Resources.Load<TextAsset>("build_info");
            var ver = UiKit.Label(p.transform, "Build", info != null ? info.text : "editor", 26, new Color(1, 1, 1, 0.35f));
            ver.rectTransform.Place(60, 1840, 960, 50);

            var titleOpt = GearButton(p.transform, "TitleOptions");
            titleOpt.GetComponent<RectTransform>().Place(960, 30, 96, 96);

            var start = UiKit.Button(p.transform, "Start", "Sign the Agreement", UiKit.Green, 52, () =>
            {
                Destroy(titlePanel);
                StartCoroutine(RunLoop());
            });
            start.GetComponent<RectTransform>().Place(190, 1400, 700, 170);
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
                if (run.Dead) { yield return GameOver(); yield break; }

                bool lastDay = run.DayNumber >= run.TotalDays;
                if (lastDay) { run.EndDay(); break; }

                run.RestockShop();
                yield return WaitNext("Next Day  >");
                var expired = run.EndDay();
                foreach (var s in expired)
                {
                    AudioKit.Sfx("expire");
                    log.Add(CardText("expire_", s.name), CardStyle.Bad);
                }
                if (expired.Count > 0)
                    battle.Banner(string.Join(", ", expired.ConvertAll(s => s.shortName)) + (expired.Count > 1 ? " have" : " has") + " *expired*. Everything is temporary.");
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
            var stage = Aqi.StageFor(run.S.aqi);
            battle.SetZone(string.IsNullOrEmpty(run.Day.bg) ? run.Day.zone : run.Day.bg, run.DayInZone);
            battle.SetAqi(run.S.aqi);
            RefreshHud();

            log.Add($"Day {run.DayNumber} of {run.TotalDays}  ·  {GameData.ZoneName(run.Day.zone)}  ·  Theeka {run.Spice}/8", CardStyle.Header);
            if (run.DayNumber > 1 && run.Spice > GameData.Days[run.S.dayIndex - 1].spice)
            {
                AudioKit.Sfx("boom", 0.6f);
                battle.Banner($"*THEEKA {run.Spice}/8: {GameData.SpiceName(run.Spice).ToUpper()}*  Enemies hit harder.");
            }
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
                    AudioKit.Music("battle");
                    var result = run.Fight();
                    yield return battle.Play(result);
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

        IEnumerator SkillPick()
        {
            var offer = run.S.OfferSkills(3, run.Rng);
            if (offer.Count == 0) yield break;
            HideButtons();

            var overlay = UiKit.Panel(phone, "SkillPick", new Color(0, 0, 0, 0.8f));
            overlay.rectTransform.Fill();
            var head = UiKit.Label(overlay.transform, "Head", "Astrologer Baba offers a blessing.\n<size=34>Every blessing is temporary.</size>", 50, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            head.rectTransform.Place(40, 260, 1000, 220);

            int picked = -1;
            for (int i = 0; i < offer.Count; i++)
            {
                int idx = i;
                var s = offer[i];
                var b = UiKit.Button(overlay.transform, "Skill" + i, "", UiKit.Cream, 34, () => picked = idx);
                b.GetComponent<RectTransform>().Place(80, 540 + i * 330, 920, 290);
                var label = b.GetComponentInChildren<Text>();
                label.color = UiKit.Ink;
                label.fontStyle = FontStyle.Normal;
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
            while (picked < 0) yield return null;

            run.S.AddSkill(offer[picked]);
            log.Add($"New skill: *{offer[picked].name}* (lease: {offer[picked].lease} days)", CardStyle.Good);
            Destroy(overlay.gameObject);
            RefreshHud();
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

        /// <summary>Bablu is dead: the lease is terminated and the run starts over from Day 1.</summary>
        IEnumerator GameOver()
        {
            HideButtons();
            yield return new WaitForSeconds(0.8f);
            AudioKit.Sfx("stamp");
            var p = UiKit.Panel(phone, "GameOver", new Color(0.05f, 0, 0, 0.88f));
            p.rectTransform.Fill();
            var stamp = UiKit.Panel(p.transform, "Stamp", new Color(0.75f, 0.06f, 0.06f, 0.95f), rounded: true);
            stamp.rectTransform.Place(90, 300, 900, 300);
            stamp.rectTransform.localRotation = Quaternion.Euler(0, 0, -8);
            var st = UiKit.Label(stamp.transform, "T", "LEASE\nTERMINATED", 100, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            st.rectTransform.Fill(10);
            UiKit.Outline(st, new Color(0.3f, 0, 0, 0.8f), 5);
            for (float k = 0; k < 1; k += Time.deltaTime * 5f) { stamp.rectTransform.localScale = Vector3.one * Mathf.Lerp(3f, 1f, k); yield return null; }
            stamp.rectTransform.localScale = Vector3.one;
            battle.Shake(20, 0.4f);

            var s = run.S;
            string body = $"*{GameData.HeroName}* died on *Day {run.DayNumber}* ({GameData.ZoneName(run.Day.zone)}), Theeka {run.Spice}/8.\n" +
                          $"Killed by *{run.Killer}*.\n\n" +
                          $"Level {s.level}  ·  ₹{s.earned} earned  ·  {s.skills.Count} skill{(s.skills.Count == 1 ? "" : "s")} on lease\n\n" +
                          UiKit.Rich(CardText("gameover_"));
            var t = UiKit.Label(p.transform, "Body", UiKit.Rich(body), 40, Color.white, TextAnchor.UpperCenter);
            t.rectTransform.Place(80, 680, 920, 700);
            t.lineSpacing = 1.2f;

            bool again = false;
            UiKit.Button(p.transform, "TryAgain", "Sign a New Lease (Day 1)", UiKit.Green, 46, () => again = true)
                .GetComponent<RectTransform>().Place(140, 1500, 800, 160);
            while (!again) yield return null;
            Destroy(p.gameObject);
            StartCoroutine(RunLoop());
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
            dayText.text = run.Finished ? "Lease complete" : $"{GameData.ZoneName(run.Day.zone)}\n<size=28><color=#FFFFFFAA>Day {run.DayNumber} of {run.TotalDays}</color></size>";
            var zi = run.Finished ? null : Art.UI("zone_" + run.Day.zone);
            zoneIcon.sprite = zi;
            zoneIcon.enabled = zi != null;
            aqiText.text = $"AQI {s.aqi}\n<size=22>{Aqi.StageName(stage)}</size>";
            int spice = run.Finished ? GameData.MaxSpice : run.Spice;
            theekaText.text = $"THEEKA {spice}/8";
            for (int i = 0; i < chilies.Length; i++) chilies[i].color = i < spice ? Color.white : new Color(0.25f, 0.25f, 0.3f, 0.7f);
            var (r, g, b) = Aqi.Color(stage);
            aqiBox.color = new Color(r, g, b);
            phoneView.Refresh();
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
        IEnumerator WaitNext(string label)
        {
            while (true)
            {
                HideButtons();
                mainButton.SetText(label);
                mainButton.targetGraphic.color = UiKit.Green;
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
