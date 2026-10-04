using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KadakChai
{
    /// <summary>
    /// The top half of the screen: background, puppets, popups, round counter, smog overlay, news ticker.
    /// Replays a pre-simulated BattleResult event by event, with juice: shake, hit-stop, flashes, sparks, slow-mo KO.
    /// </summary>
    public class BattleView : MonoBehaviour
    {
        public const float AW = 1080, AH = 820; // battle area size in reference px

        RectTransform area, stage, bgRoot, popups, tickerText, overlay;
        Image bg, bgNext, smog, flash;
        Text roundText, speedText;
        Puppet hero, aunty;   // aunty = the current zone companion's puppet (any companion)
        /// <summary>Bablu's HP after each hit or heal during playback (the phone header follows it live).</summary>
        public System.Action<int> HeroHp;
        readonly List<Puppet> enemies = new();
        string zone;
        // "1x" plays at 0.65 of the original pace (playtest: original felt too fast). The speed button cycles
        // through these multipliers (change log 18:39) and is remembered between visits.
        const float BasePace = 0.65f;
        static readonly float[] Speeds = { 0.5f, 1f, 1.25f, 1.5f, 2f };
        int speedIndex = 1;
        float speed = BasePace;
        Coroutine tickerLoop, shakeLoop;

        public float Speed => speed;

        public static BattleView Build(RectTransform phone)
        {
            var area = UiKit.Make("Battle", phone).Place(0, 100, AW, AH);
            area.gameObject.AddComponent<RectMask2D>();
            var v = area.gameObject.AddComponent<BattleView>();
            v.area = area;

            // Everything that shakes lives on the stage; HUD bits (round, speed, ticker) stay still.
            v.stage = UiKit.Make("Stage", area);
            v.stage.Fill();

            // Background is wider than the area so it can slide a little each day ("walking").
            v.bgRoot = UiKit.Make("Background", v.stage);
            v.bgRoot.anchorMin = new Vector2(0, 0); v.bgRoot.anchorMax = new Vector2(0, 1);
            v.bgRoot.pivot = new Vector2(0, 0.5f);
            v.bgRoot.sizeDelta = new Vector2(1320, 0);
            v.bg = v.bgRoot.gameObject.AddComponent<Image>();
            v.bgNext = UiKit.Panel(v.bgRoot, "Next", Color.clear);
            v.bgNext.rectTransform.Fill();

            v.smog = UiKit.Panel(v.stage, "Smog", new Color(0.62f, 0.63f, 0.66f, 0));
            v.smog.rectTransform.Fill();
            v.smog.raycastTarget = false;

            v.popups = UiKit.Make("Popups", v.stage);
            v.popups.Fill();

            v.flash = UiKit.Panel(v.stage, "Flash", new Color(1, 1, 1, 0));
            v.flash.rectTransform.Fill(-40);
            v.flash.raycastTarget = false;

            v.overlay = UiKit.Make("Overlay", area);
            v.overlay.Fill();

            v.roundText = UiKit.Label(area, "Round", "", 40, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            v.roundText.rectTransform.Place(340, 16, 400, 60, AW, AH);
            UiKit.Outline(v.roundText, new Color(0, 0, 0, 0.8f), 2);

            var speedBtn = UiKit.Button(area, "Speed", "1x", new Color(1f, 0.7f, 0.1f), 34, null);
            speedBtn.GetComponent<RectTransform>().Place(920, 16, 144, 76, AW, AH);
            v.speedText = speedBtn.GetComponentInChildren<Text>();
            speedBtn.onClick.AddListener(v.ToggleSpeed);
            try { v.speedIndex = Mathf.Clamp(PlayerPrefs.GetInt("kc_speed", 1), 0, Speeds.Length - 1); } catch { }
            v.ApplySpeed();

            // News ticker along the bottom of the battle area.
            var strip = UiKit.Panel(area, "Ticker", new Color(0, 0, 0, 0.82f));
            strip.rectTransform.Place(0, AH - 56, AW, 56, AW, AH);
            strip.gameObject.AddComponent<RectMask2D>();
            var tag = UiKit.Panel(strip.transform, "Tag", UiKit.Red);
            tag.rectTransform.Place(0, 0, 170, 56, AW, 56);
            var tagText = UiKit.Label(tag.transform, "T", "BREAKING", 26, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            tagText.rectTransform.Fill();
            var tt = UiKit.Label(strip.transform, "Text", "", 28, new Color(1f, 0.92f, 0.3f), TextAnchor.MiddleLeft);
            tt.horizontalOverflow = HorizontalWrapMode.Overflow;
            v.tickerText = tt.rectTransform;
            v.tickerText.anchorMin = new Vector2(0, 0); v.tickerText.anchorMax = new Vector2(0, 1);
            v.tickerText.pivot = new Vector2(0, 0.5f);
            v.tickerText.sizeDelta = new Vector2(2000, 0);
            tag.transform.SetAsLastSibling();
            return v;
        }

        void ToggleSpeed()
        {
            speedIndex = (speedIndex + 1) % Speeds.Length;
            try { PlayerPrefs.SetInt("kc_speed", speedIndex); PlayerPrefs.Save(); } catch { }
            ApplySpeed();
        }

        void ApplySpeed()
        {
            speed = BasePace * Speeds[speedIndex];
            speedText.text = "<size=20>SPEED</size>\n" + Speeds[speedIndex].ToString("0.##") + "x";
            speedText.lineSpacing = 0.8f;
        }

        // ============================================================ juice primitives

        public void Shake(float amp, float dur)
        {
            if (shakeLoop != null) StopCoroutine(shakeLoop);
            shakeLoop = StartCoroutine(ShakeRoutine(amp, dur));
        }

        IEnumerator ShakeRoutine(float amp, float dur)
        {
            for (float t = 0; t < dur; t += Time.unscaledDeltaTime)
            {
                float k = 1 - t / dur;
                stage.anchoredPosition = Random.insideUnitCircle * amp * k * k;
                yield return null;
            }
            stage.anchoredPosition = Vector2.zero;
        }

        IEnumerator HitStop(float realSeconds)
        {
            Time.timeScale = 0.02f;
            yield return new WaitForSecondsRealtime(realSeconds);
            Time.timeScale = 1f;
        }

        public void Flash(Color c, float dur = 0.15f) => StartCoroutine(FlashRoutine(c, dur));

        IEnumerator FlashRoutine(Color c, float dur)
        {
            for (float t = 0; t < dur; t += Time.unscaledDeltaTime)
            {
                flash.color = new Color(c.r, c.g, c.b, c.a * (1 - t / dur));
                yield return null;
            }
            flash.color = Color.clear;
        }

        IEnumerator ZoomPunch(float amount, float dur)
        {
            for (float t = 0; t < dur; t += Time.unscaledDeltaTime)
            {
                float k = t / dur;
                stage.localScale = Vector3.one * (1 + amount * Mathf.Sin(Mathf.PI * k) * (1 - k));
                yield return null;
            }
            stage.localScale = Vector3.one;
        }

        IEnumerator SlowMo(float scale, float realSeconds)
        {
            Time.timeScale = scale;
            yield return new WaitForSecondsRealtime(realSeconds);
            Time.timeScale = 1f;
        }

        public void Celebrate()
        {
            for (int i = 0; i < 5; i++)
                Fx.Burst(this, popups, new Vector2(Random.Range(-420f, 420f), 260), Fx.Confetti[i], 12, 700, 20, 1.4f, 900, square: true);
        }

        // ============================================================ scene dressing

        public void SetZone(string newZone, int dayInZone)
        {
            var (sprite, _) = Art.Background(newZone);
            float x = -55f * dayInZone;
            if (zone == null) { bg.sprite = sprite; bgRoot.anchoredPosition = new Vector2(x, 0); }
            else if (zone != newZone) StartCoroutine(Crossfade(sprite, x));
            zone = newZone;
        }

        IEnumerator Crossfade(Sprite next, float x)
        {
            bgNext.sprite = next;
            for (float t = 0; t < 1; t += Time.deltaTime * 2.5f) { bgNext.color = new Color(1, 1, 1, t); yield return null; }
            bg.sprite = next;
            bgNext.color = Color.clear;
            bgRoot.anchoredPosition = new Vector2(x, 0);
        }

        /// <summary>Hero bobs fast while the background slides: the walk to the next day.</summary>
        public IEnumerator Walk(int dayInZoneAfter)
        {
            if (hero != null) hero.BobSpeed = 7f;
            if (aunty != null) aunty.BobSpeed = 7f;
            var from = bgRoot.anchoredPosition;
            var to = new Vector2(-55f * dayInZoneAfter, 0);
            for (float t = 0; t < 1; t += Time.deltaTime * 1.4f)
            {
                bgRoot.anchoredPosition = Vector2.Lerp(from, to, t);
                yield return null;
            }
            if (hero != null) hero.BobSpeed = 2.2f;
            if (aunty != null) aunty.BobSpeed = 2.2f;
        }

        public void SetAqi(int aqi)
        {
            float a = Mathf.Clamp01((aqi - 120f) / 700f) * 0.55f;
            smog.color = new Color(0.62f, 0.63f, 0.66f, a); // hazy grey (change log 19:21)
        }

        public void SetTicker(string text)
        {
            if (text.StartsWith("BREAKING:")) text = text.Substring(9).TrimStart(); // the red tag already says it
            tickerText.GetComponent<Text>().text = text;
            if (tickerLoop != null) StopCoroutine(tickerLoop);
            tickerLoop = StartCoroutine(TickerLoop());
        }

        IEnumerator TickerLoop()
        {
            var t = tickerText.GetComponent<Text>();
            while (true)
            {
                float w = t.preferredWidth;
                for (float x = AW; x > -w; x -= Time.unscaledDeltaTime * 140f)
                {
                    tickerText.anchoredPosition = new Vector2(x, 0);
                    yield return null;
                }
            }
        }

        // ============================================================ puppets

        public void ShowParty(RunState s)
        {
            if (hero == null)
                hero = Puppet.Create(stage, "new_kid", GameData.HeroName, "#F2A65A", s.hp, s.maxHp, "HP", true, false);
            hero.Root.Place(150, 330, 250, 330, AW, AH);
            hero.SetHp(s.hp, s.maxHp);

            if (aunty != null && aunty.Id != s.companion) { Destroy(aunty.gameObject); aunty = null; } // companion left or changed
            if (s.HasCompanion && aunty == null && GameData.Combatants.TryGetValue(s.companion, out var ad))
            {
                aunty = Puppet.Create(stage, ad.id, ad.name, ad.color, 1, 1, "", false, false);
                aunty.Root.Place(10, 470, 160, 200, AW, AH);
                aunty.transform.SetSiblingIndex(hero.transform.GetSiblingIndex()); // behind the hero
                StartCoroutine(aunty.DropIn(0f));
                StartCoroutine(DustLater(aunty, 0.35f));
            }
            popups.SetAsLastSibling();
            flash.transform.SetAsLastSibling();
        }

        /// <summary>Enemies drop in from above with a thud and a puff of dust.</summary>
        public IEnumerator ShowEnemies(List<CombatantData> list)
        {
            ClearEnemies();
            var slots = list.Count switch
            {
                1 => new[] { new Rect(640, 260, 320, 400) },
                2 => new[] { new Rect(590, 360, 220, 300), new Rect(830, 380, 210, 280) },
                _ => new[] { new Rect(540, 390, 170, 260), new Rect(720, 370, 170, 270), new Rect(895, 390, 165, 260) },
            };
            for (int i = 0; i < list.Count && i < slots.Length; i++)
            {
                var e = list[i];
                var p = Puppet.Create(stage, e.id, e.name, e.color, e.hp, e.hp,
                                      string.IsNullOrEmpty(e.hpLabel) ? "HP" : e.hpLabel, true, true);
                p.Root.Place(slots[i].x, slots[i].y, slots[i].width, slots[i].height, AW, AH);
                enemies.Add(p);
                StartCoroutine(p.DropIn(i * 0.15f));
                StartCoroutine(DustLater(p, i * 0.15f + 0.35f));
            }
            popups.SetAsLastSibling();
            flash.transform.SetAsLastSibling();
            yield return new WaitForSecondsRealtime(0.5f + list.Count * 0.15f);
            if (list.Count == 1) Shake(10, 0.25f);
        }

        IEnumerator DustLater(Puppet p, float delay)
        {
            yield return new WaitForSecondsRealtime(delay);
            if (p != null) Fx.Burst(this, popups, p.PointIn(popups, 0.5f, 0.02f), new Color(0.85f, 0.8f, 0.72f, 0.9f), 10, 260, 34, 0.5f, 200);
        }

        public void ClearEnemies()
        {
            foreach (var e in enemies) if (e != null) Destroy(e.gameObject);
            enemies.Clear();
            roundText.text = "";
        }

        public void RefreshHero(RunState s) => hero?.SetHp(s.hp, s.maxHp);

        /// <summary>Jadi-Booti Zombie: Bablu turns green for the next fight.</summary>
        public void SetZombie(bool on) => hero?.SetTint(on ? new Color(0.62f, 1f, 0.55f) : Color.white);

        /// <summary>New run after a death: the hero gets back up.</summary>
        public void ReviveHero(RunState s)
        {
            if (hero == null) return;
            StartCoroutine(hero.StandUp(1f));
            hero.SetHp(s.hp, s.maxHp);
        }

        Image held;
        string heldId;
        /// <summary>The equipped weapon, drawn in the hero's hand.</summary>
        public void SetHeld(RunState s)
        {
            if (hero == null) return;
            string id = s.weapon?.data.id;
            if (id == heldId && held != null) return;
            heldId = id;
            if (held == null)
            {
                held = UiKit.Panel(hero.Root, "Held", Color.white);
                held.raycastTarget = false;
                held.preserveAspect = true;
                held.rectTransform.Place(150, 120, 150, 150, 250, 330);
                held.rectTransform.localRotation = Quaternion.Euler(0, 0, -20);
            }
            var sprite = id == null ? null : Art.UI("item_" + id);
            held.sprite = sprite;
            held.enabled = sprite != null;
        }

        /// <summary>Restore puppets after a lost fight so the retry starts clean.</summary>
        public IEnumerator ResetForRetry(RunState s, List<CombatantData> list)
        {
            if (hero != null) { StartCoroutine(hero.StandUp(1f)); hero.SetHp(s.hp, s.maxHp); }
            yield return ShowEnemies(list);
        }

        // ============================================================ VS intro

        /// <summary>Capybara-style versus card before a fight. Bigger and slower for bosses.</summary>
        public IEnumerator VsIntro(List<CombatantData> list, bool boss)
        {
            AudioKit.Sfx("whoosh");
            var root = UiKit.Make("VS", overlay);
            root.Fill();
            var group = root.gameObject.AddComponent<CanvasGroup>();
            var dim = UiKit.Panel(root, "Dim", new Color(0, 0, 0, 0.45f));
            dim.rectTransform.Fill();

            RectTransform Side(bool left, Color col, string spriteId, string label, string sub)
            {
                var panel = UiKit.Panel(root, left ? "Left" : "Right", col);
                var rt = panel.rectTransform;
                rt.Place(left ? -60 : 560, 210, 580, 360, AW, AH);
                rt.localRotation = Quaternion.Euler(0, 0, left ? 4 : -4);
                UiKit.Outline(panel, UiKit.Ink, 6);
                var (sp, ph) = Art.Character(spriteId);
                var img = UiKit.Panel(rt, "Sprite", Color.white);
                img.sprite = sp; img.preserveAspect = true;
                if (ph && GameData.Combatants.TryGetValue(spriteId, out var cd)) img.color = UiKit.Hex(cd.color);
                img.rectTransform.Place(left ? 110 : 190, -40, 280, 350, 580, 360);
                if (!left && !ph) img.rectTransform.localScale = new Vector3(-1, 1, 1);
                var name = UiKit.Label(rt, "Name", label, 40, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
                name.rectTransform.Place(20, 290, 540, 60, 580, 360);
                UiKit.Outline(name, UiKit.Ink, 3);
                if (!string.IsNullOrEmpty(sub))
                {
                    var s = UiKit.Label(rt, "Sub", sub, 30, new Color(1f, 0.9f, 0.3f), TextAnchor.MiddleCenter, FontStyle.Bold);
                    s.rectTransform.Place(20, 10, 540, 50, 580, 360);
                    UiKit.Outline(s, UiKit.Ink, 3);
                }
                return rt;
            }

            var leftRt = Side(true, UiKit.Hex("#2BB3A3"), "new_kid", GameData.HeroName, null);
            string enemyName = list.Count == 1 ? list[0].name : list.Count == 2 ? $"{list[0].name} + 1" : $"{list[0].name} + {list.Count - 1}";
            var rightRt = Side(false, boss ? UiKit.Purple : UiKit.Red, list.Count > 0 ? list[0].id : "", enemyName, boss ? "BOSS" : null);

            var vs = UiKit.Label(root, "VS", "VS", 150, new Color(1f, 0.85f, 0.15f), TextAnchor.MiddleCenter, FontStyle.Bold);
            vs.rectTransform.Place(390, 250, 300, 260, AW, AH);
            vs.gameObject.AddComponent<TextFx>().shake = 3f;
            UiKit.Outline(vs, UiKit.Ink, 7);

            Vector2 lHome = leftRt.anchoredPosition, rHome = rightRt.anchoredPosition;
            for (float t = 0; t < 1; t += Time.unscaledDeltaTime / 0.25f)
            {
                float k = 1 - (1 - t) * (1 - t);
                leftRt.anchoredPosition = lHome + new Vector2(-700 * (1 - k), 0);
                rightRt.anchoredPosition = rHome + new Vector2(700 * (1 - k), 0);
                vs.rectTransform.localScale = Vector3.one * Mathf.Lerp(3f, 1f, k);
                group.alpha = Mathf.Clamp01(t * 3);
                yield return null;
            }
            leftRt.anchoredPosition = lHome; rightRt.anchoredPosition = rHome;
            vs.rectTransform.localScale = Vector3.one;
            AudioKit.Sfx("stamp", 0.8f);
            Shake(boss ? 22 : 12, 0.35f);
            yield return new WaitForSecondsRealtime(boss ? 1.1f : 0.6f);
            for (float t = 0; t < 1; t += Time.unscaledDeltaTime / 0.2f) { group.alpha = 1 - t; yield return null; }
            Destroy(root.gameObject);
        }

        // ============================================================ playback

        Puppet PuppetFor(int unitIndex, BattleResult r)
        {
            if (unitIndex < 0 || unitIndex >= r.units.Count) return null;
            var u = r.units[unitIndex];
            if (u.isPlayerSide) return u.isPet ? aunty : hero;
            int enemyIndex = 0;
            for (int i = 0; i < unitIndex; i++) if (!r.units[i].isPlayerSide) enemyIndex++;
            return enemyIndex < enemies.Count ? enemies[enemyIndex] : null;
        }

        readonly HashSet<string> liveConditions = new();

        /// <summary>Bablu's lasting statuses on his puppet (overlays + pills).</summary>
        public void SetConditions(RunState s)
        {
            liveConditions.Clear();
            foreach (var c in s.conditions) liveConditions.Add(c);
            hero?.SetConditions(liveConditions);
        }

        static string StatusIdFromLabel(string label)
        {
            foreach (var kv in Conditions.All) if (label.StartsWith(kv.Value.name.ToUpper())) return kv.Key;
            return label;
        }

        /// <summary>First punchy phrase of a gimmick line: "PIVOT! Now B2B" -> "PIVOT!".</summary>
        static string ShortWord(string label)
        {
            if (string.IsNullOrEmpty(label)) return "";
            int i = label.IndexOfAny(new[] { '!', '.', '?' });
            string w = i > 0 && i < 28 ? label.Substring(0, i + 1) : label.Length > 24 ? label.Substring(0, 22) + "..." : label;
            return w.ToUpper();
        }

        /// <summary>Paper Mario-style status card: a white paper sticker with a thick outline and a drop shadow.</summary>
        void PaperCard(Puppet over, string word, Color color)
        {
            var root = UiKit.Make("PaperCard", popups);
            root.sizeDelta = new Vector2(440, 130);
            root.anchoredPosition = over.HeadPosition(popups) + new Vector2(0, 110);
            root.localRotation = Quaternion.Euler(0, 0, Random.Range(-7f, 7f));
            var shadow = UiKit.Panel(root, "Shadow", new Color(0, 0, 0, 0.35f), rounded: true);
            shadow.rectTransform.Fill(); shadow.rectTransform.anchoredPosition = new Vector2(10, -10);
            var paper = UiKit.Panel(root, "Paper", Color.white, rounded: true);
            paper.rectTransform.Fill();
            UiKit.Outline(paper, UiKit.Ink, 6);
            var t = UiKit.Label(root, "Word", word, 56, color, TextAnchor.MiddleCenter, FontStyle.Bold);
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.rectTransform.Fill(8);
            UiKit.Outline(t, UiKit.Ink, 3);
            t.gameObject.AddComponent<TextFx>().wave = 4f;
            StartCoroutine(ImpactRoutine(root));
        }

        /// <summary>Comic-book burst with a big word over a puppet: DISHOOM oomph for gimmicks and statuses.</summary>
        void Gimmick(Puppet over, string word, Color color)
        {
            var root = UiKit.Make("Gimmick", popups);
            root.sizeDelta = new Vector2(460, 200);
            root.anchoredPosition = over.HeadPosition(popups) + new Vector2(Random.Range(-30f, 30f), 70);
            root.localRotation = Quaternion.Euler(0, 0, Random.Range(-10f, 10f));
            var burst = Art.UI("burst");
            if (burst != null)
            {
                var b = UiKit.Panel(root, "Burst", color);
                b.sprite = burst; b.raycastTarget = false;
                b.rectTransform.Fill(-20);
            }
            var t = UiKit.Label(root, "Word", word, 52, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.rectTransform.Fill();
            UiKit.Outline(t, UiKit.Ink, 5);
            t.gameObject.AddComponent<TextFx>().wave = 5f;
            StartCoroutine(ImpactRoutine(root));
        }

        public IEnumerator Play(BattleResult r)
        {
            bool techno = false; // switch to Delhi techno once the hero drops below 50% HP (change log 19:21)
            bool effectiveShownHero = false, effectiveShownFoe = false;   // teach close-beats-ranged once per side per fight
            int enemiesLeft = 0;
            foreach (var u in r.units) if (!u.isPlayerSide) enemiesLeft++;

            foreach (var e in r.events)
            {
                var actor = PuppetFor(e.actor, r);
                var target = PuppetFor(e.target, r);
                float dir = actor != null && actor.Root.anchorMin.x < 0.5f ? 1 : -1;
                bool targetIsHero = target != null && target == hero;

                switch (e.type)
                {
                    case BattleEventType.Round:
                        roundText.text = e.amount > AutoBattle.MaxRounds ? "<color=#FF5252>SUDDEN DEATH</color>" : $"Round {e.amount}/{AutoBattle.MaxRounds}";
                        yield return Wait(0.15f);
                        break;

                    case BattleEventType.Hit:
                        if (!techno && targetIsHero && e.hpAfter < r.units[0].maxHp * 0.5f) { techno = true; AudioKit.Music("techno"); }
                        if (actor != null && actor != target) yield return Approach(e, actor, target, dir);
                        if (!string.IsNullOrEmpty(e.label) && actor != null) Popup(e.label, actor, new Color(0.7f, 0.95f, 1f), 42, 60);
                        if (target != null)
                        {
                            target.Hit(speed, e.big);
                            target.SetHp(e.hpAfter);
                            if (targetIsHero) HeroHp?.Invoke(e.hpAfter);
                            var at = target.PointIn(popups, 0.5f, 0.5f);
                            Fx.Burst(this, popups, at, e.big ? new Color(1f, 0.85f, 0.2f) : Color.white, e.big ? 16 : 7, e.big ? 900 : 550, e.big ? 26 : 18, 0.45f);
                            ImpactNumber(target, e.amount, e.big, targetIsHero);
                            if (e.effective && !(targetIsHero ? effectiveShownFoe : effectiveShownHero))
                            {
                                if (targetIsHero) effectiveShownFoe = true; else effectiveShownHero = true;
                                Popup("SUPER EFFECTIVE!", target, new Color(1f, 0.85f, 0.2f), 40, 120, true);
                            }
                        }
                        PlayHitSound(e);
                        if (e.big)
                        {
                            Shake(18, 0.3f);
                            Flash(new Color(1, 1, 1, 0.45f), 0.12f);
                            StartCoroutine(ZoomPunch(0.04f, 0.25f));
                            yield return HitStop(0.07f);
                        }
                        else Shake(targetIsHero ? 8 : 5, 0.15f);
                        if (targetIsHero && e.amount >= r.units[0].maxHp * 0.12f) Flash(new Color(1, 0, 0, 0.25f), 0.2f);
                        yield return Wait(0.3f);
                        break;

                    case BattleEventType.Miss:
                    case BattleEventType.Dodge:
                    case BattleEventType.Immune:
                        if (actor != null && actor != target) yield return Approach(e, actor, target, dir);
                        if (target != null) Popup(e.type == BattleEventType.Immune ? e.label : e.type.ToString(), target, new Color(0.8f, 0.8f, 0.8f), 42);
                        yield return Wait(0.3f);
                        break;

                    case BattleEventType.Heal:
                        target?.SetHp(e.hpAfter);
                        if (targetIsHero) HeroHp?.Invoke(e.hpAfter);
                        if (target != null)
                        {
                            Popup("+" + e.amount, target, new Color(0.45f, 1f, 0.5f), 52);
                            Fx.Burst(this, popups, target.PointIn(popups, 0.5f, 0.3f), new Color(0.5f, 1f, 0.55f), 10, 300, 20, 0.8f, -300);
                        }
                        if (!string.IsNullOrEmpty(e.label) && actor != null) Popup(e.label, actor, new Color(0.7f, 1f, 0.75f), 42, 60);
                        AudioKit.Sfx("heal", 0.7f);
                        yield return Wait(0.35f);
                        break;

                    case BattleEventType.Stun:
                        if (actor != null && actor != hero && actor != aunty && !string.IsNullOrEmpty(e.label)) Gimmick(actor, ShortWord(e.label), UiKit.Hex("#FF7043"));
                        if (target != null) Popup(e.label ?? "Stunned", target, new Color(1f, 0.75f, 1f), 40, 0, true);
                        AudioKit.Sfx(GameData.SkillByLabel(e.label)?.sfx ?? "shout", 0.7f);
                        yield return Wait(0.45f);
                        break;

                    case BattleEventType.Shout:
                        if (e.big && target != null)
                        {
                            // A lasting status just landed on Bablu: DISHOOM-style word over him (change log).
                            AudioKit.Sfx("boom", 0.7f);
                            PaperCard(target, e.label, UiKit.Hex("#8E24AA"));
                            liveConditions.Add(StatusIdFromLabel(e.label));
                            if (target == hero) StartCoroutine(hero.PaperFlip(() => hero.SetConditions(liveConditions)));
                            Shake(14, 0.3f);
                            yield return Wait(0.9f);
                            break;
                        }
                        if (actor != null && actor != hero && actor != aunty) Gimmick(actor, ShortWord(e.label), UiKit.Hex("#FF7043"));
                        Banner(e.label);
                        yield return Wait(0.9f);
                        break;

                    case BattleEventType.Death:
                        AudioKit.Sfx("ko");
                        if (target != null)
                        {
                            Fx.Burst(this, popups, target.PointIn(popups, 0.5f, 0.3f), new Color(0.75f, 0.72f, 0.68f, 0.9f), 14, 420, 36, 0.7f, 300);
                            bool finalBlow = !targetIsHero && --enemiesLeft == 0;
                            if (finalBlow)
                            {
                                Shake(26, 0.5f);
                                Flash(new Color(1, 1, 1, 0.6f), 0.2f);
                                StartCoroutine(ZoomPunch(0.08f, 0.6f));
                                StartCoroutine(SlowMo(0.3f, 0.7f));
                            }
                            yield return target.Die(speed);
                            if (finalBlow) { Celebrate(); yield return new WaitForSecondsRealtime(0.4f); }
                        }
                        break;

                    case BattleEventType.Summon:
                        // Lalaji's board meeting: a relative drops into the fight.
                        if (e.target >= 0 && e.target < r.units.Count)
                        {
                            var u = r.units[e.target];
                            var slot = enemies.Count == 1 ? new Rect(470, 400, 200, 270) : new Rect(900, 420, 170, 240);
                            var p = Puppet.Create(stage, u.id, u.name, u.color, u.hp, u.maxHp, u.hpLabel, true, true);
                            p.Root.Place(slot.x, slot.y, slot.width, slot.height, AW, AH);
                            enemies.Add(p);
                            popups.SetAsLastSibling(); flash.transform.SetAsLastSibling();
                            enemiesLeft++;
                            StartCoroutine(p.DropIn(0f));
                            StartCoroutine(DustLater(p, 0.35f));
                            AudioKit.Sfx("whoosh");
                            Gimmick(p, u.id == "cfo_bunty" ? "CFO!" : "CTO!", UiKit.Hex("#FF7043"));
                            Shake(12, 0.25f);
                        }
                        yield return Wait(0.7f);
                        break;

                    case BattleEventType.Revive:
                        target?.SetHp(e.hpAfter);
                        if (targetIsHero) HeroHp?.Invoke(e.hpAfter);
                        Flash(new Color(1f, 0.9f, 0.3f, 0.5f), 0.3f);
                        Shake(16, 0.4f);
                        if (target != null) yield return target.StandUp(speed);
                        Banner(e.label);
                        yield return Wait(1.2f);
                        break;
                }
            }
            Time.timeScale = 1f;
            yield return Wait(0.4f);
        }

        WaitForSeconds Wait(float s) => new(s / speed);

        /// <summary>Close attacks lunge in; ranged attacks flick a projectile across the stage.</summary>
        IEnumerator Approach(BattleEvent e, Puppet actor, Puppet target, float dir)
        {
            if (!e.ranged || target == null) { yield return actor.Lunge(dir, speed); yield break; }
            yield return actor.Throw(dir, speed);
            yield return Projectile(e, actor, target, dir);
        }

        IEnumerator Projectile(BattleEvent e, Puppet actor, Puppet target, float dir)
        {
            var sprite = string.IsNullOrEmpty(e.projectile) ? null : Art.UI(e.projectile);
            var img = UiKit.Panel(popups, "Projectile", Color.white);
            img.raycastTarget = false;
            img.preserveAspect = true;
            if (sprite != null) img.sprite = sprite;
            else
            {
                img.sprite = Art.Dot;
                var u = actor == hero ? "#F2A65A" : null;
                img.color = UiKit.Hex(u ?? "#FFE14D");
            }
            var rt = img.rectTransform;
            rt.sizeDelta = sprite != null ? new Vector2(120, 120) : new Vector2(46, 46);
            Vector2 a = actor.PointIn(popups, dir > 0 ? 0.8f : 0.2f, 0.62f);
            Vector2 b = target.PointIn(popups, 0.5f, 0.55f);
            bool spins = e.projectile == null || !(e.projectile.Contains("firecracker") || e.projectile.Contains("sector_cabbie"));
            if (!spins) rt.localScale = new Vector3(dir > 0 ? 1 : -1, 1, 1); // rockets and honks fly nose-first
            AudioKit.SfxVaried("whoosh", 0.45f, 0.1f);
            float dur = 0.3f / speed;
            for (float t = 0; t < dur; t += Time.deltaTime)
            {
                float k = t / dur;
                rt.anchoredPosition = Vector2.Lerp(a, b, k) + new Vector2(0, Mathf.Sin(Mathf.PI * k) * 110f);
                if (spins) rt.localRotation = Quaternion.Euler(0, 0, -dir * 900f * t);
                yield return null;
            }
            Destroy(img.gameObject);
        }

        static readonly string[] ComicWords = { "DHISHOOM!", "PATAAK!", "DHADAM!", "THWACK!", "DHAAP!", "KAPOW!" };

        void PlayHitSound(BattleEvent e)
        {
            var skill = GameData.SkillByLabel(e.label);
            if (skill != null && !string.IsNullOrEmpty(skill.sfx)) AudioKit.SfxVaried(skill.sfx, 0.8f, 0.05f);
            if (e.big) { AudioKit.SfxVaried("crit", 0.9f, 0.06f); AudioKit.SfxVaried("dhishoom", 0.6f, 0.08f); }
            else AudioKit.Punch(0.9f);
        }

        /// <summary>
        /// Stick-of-Truth style: the number lands ON the body inside a comic starburst, holds for a beat,
        /// then floats off. Big hits are bigger, red, and get a Bollywood sound word.
        /// </summary>
        void ImpactNumber(Puppet target, int amount, bool big, bool onHero)
        {
            var root = UiKit.Make("Impact", popups);
            root.sizeDelta = big ? new Vector2(270, 270) : new Vector2(190, 190);
            root.anchoredPosition = target.PointIn(popups, Random.Range(0.35f, 0.65f), Random.Range(0.45f, 0.7f));
            root.localRotation = Quaternion.Euler(0, 0, Random.Range(-15f, 15f));

            var burst = Art.UI("burst");
            if (burst != null)
            {
                var b = UiKit.Panel(root, "Burst", big ? UiKit.Hex("#FF5A36") : onHero ? UiKit.Hex("#FFB4B4") : UiKit.Hex("#FFE14D"));
                b.sprite = burst;
                b.raycastTarget = false;
                b.rectTransform.Fill();
            }
            var t = UiKit.Label(root, "Num", amount.ToString(), big ? 96 : 66, big ? Color.white : UiKit.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            t.rectTransform.Fill();
            if (big) { t.gameObject.AddComponent<TextFx>().shake = 3f; UiKit.Outline(t, UiKit.Ink, 4); }

            if (big)
            {
                var w = UiKit.Label(root, "Word", ComicWords[Random.Range(0, ComicWords.Length)], 58, new Color(1f, 0.9f, 0.2f), TextAnchor.MiddleCenter, FontStyle.Bold);
                w.horizontalOverflow = HorizontalWrapMode.Overflow;
                w.rectTransform.anchorMin = w.rectTransform.anchorMax = new Vector2(0.5f, 1f);
                w.rectTransform.sizeDelta = new Vector2(500, 90);
                w.rectTransform.anchoredPosition = new Vector2(0, 30);
                w.rectTransform.localRotation = Quaternion.Euler(0, 0, Random.Range(-12f, 12f));
                w.gameObject.AddComponent<TextFx>().wave = 5f;
                UiKit.Outline(w, UiKit.Ink, 5);
            }
            StartCoroutine(ImpactRoutine(root));
        }

        IEnumerator ImpactRoutine(RectTransform rt)
        {
            var group = rt.gameObject.AddComponent<CanvasGroup>();
            Vector2 start = rt.anchoredPosition;
            float life = 1.1f;
            for (float t = 0; t < life; t += Time.unscaledDeltaTime * Mathf.Max(1f, speed))
            {
                float k = t / life;
                // slam in big, settle, hold, then drift up and fade
                float s = k < 0.08f ? Mathf.Lerp(1.5f, 0.9f, k / 0.08f) : k < 0.16f ? Mathf.Lerp(0.9f, 1.05f, (k - 0.08f) / 0.08f) : 1.05f;
                rt.localScale = Vector3.one * s;
                if (k > 0.55f)
                {
                    float f = (k - 0.55f) / 0.45f;
                    rt.anchoredPosition = start + new Vector2(0, 80 * f);
                    group.alpha = 1 - f;
                }
                yield return null;
            }
            Destroy(rt.gameObject);
        }

        void Popup(string text, Puppet over, Color color, int size, float extraY = 0, bool shaky = false)
        {
            var t = UiKit.Label(popups, "Popup", text, size, color, TextAnchor.MiddleCenter, FontStyle.Bold);
            t.horizontalOverflow = HorizontalWrapMode.Overflow;
            if (shaky) t.gameObject.AddComponent<TextFx>().shake = 2.5f;
            UiKit.Outline(t, new Color(0, 0, 0, 0.85f), 3);
            var rt = t.rectTransform;
            rt.sizeDelta = new Vector2(600, 100);
            rt.anchoredPosition = over.HeadPosition(popups) + new Vector2(Random.Range(-25f, 25f), 30 + extraY);
            rt.localRotation = Quaternion.Euler(0, 0, Random.Range(-8f, 8f));
            StartCoroutine(FloatAway(rt, t));
        }

        IEnumerator FloatAway(RectTransform rt, Text t)
        {
            Vector2 start = rt.anchoredPosition;
            Color c = t.color;
            for (float k = 0; k < 1; k += Time.unscaledDeltaTime * 1.2f * speed)
            {
                rt.anchoredPosition = start + new Vector2(0, 100 * (1 - (1 - k) * (1 - k)));
                float pop = k < 0.12f ? Mathf.Lerp(0.3f, 1.35f, k / 0.12f) : k < 0.25f ? Mathf.Lerp(1.35f, 1f, (k - 0.12f) / 0.13f) : 1f;
                rt.localScale = Vector3.one * pop;
                t.color = new Color(c.r, c.g, c.b, k < 0.65f ? 1 : 1 - (k - 0.65f) / 0.35f);
                yield return null;
            }
            Destroy(rt.gameObject);
        }

        /// <summary>Big centred wavy text for gimmicks: "PIVOT!", "Lunch Break!", odd-even, etc.</summary>
        public void Banner(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            AudioKit.Sfx("shout", 0.7f);
            var bgImg = UiKit.Panel(popups, "Banner", new Color(0, 0, 0, 0.75f), rounded: true);
            bgImg.rectTransform.Place(50, 110, 980, 140, AW, AH);
            var t = UiKit.Label(bgImg.transform, "Text", UiKit.Rich(text), 40, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            t.rectTransform.Fill(14);
            t.gameObject.AddComponent<TextFx>().wave = 4f;
            StartCoroutine(FadeBanner(bgImg));
        }

        IEnumerator FadeBanner(Image img)
        {
            var group = img.gameObject.AddComponent<CanvasGroup>();
            var rt = img.rectTransform;
            for (float k = 0; k < 1; k += Time.unscaledDeltaTime * 0.9f * speed)
            {
                group.alpha = k < 0.1f ? k / 0.1f : k > 0.75f ? 1 - (k - 0.75f) / 0.25f : 1;
                rt.localScale = new Vector3(1, k < 0.1f ? Mathf.Lerp(0.2f, 1f, k / 0.1f) : 1, 1);
                yield return null;
            }
            Destroy(img.gameObject);
        }
    }
}
