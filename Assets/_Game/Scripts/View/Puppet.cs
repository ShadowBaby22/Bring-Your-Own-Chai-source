using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KadakChai
{
    /// <summary>
    /// One character on screen. A single static image; every animation is code (GDD §7.3):
    /// drop-in entrance, idle squash-bob, anticipation + lunge, hit squash + flash + shake, tip over, stand up.
    /// Optional pose art: &lt;id&gt;_attack.png and &lt;id&gt;_hit.png are swapped in briefly if they exist.
    /// </summary>
    public class Puppet : MonoBehaviour
    {
        public string Id { get; private set; }
        public RectTransform Root => (RectTransform)transform;
        public bool FacesLeft { get; private set; }

        RectTransform body;
        Image image, hpFill, hpGhost, damage;
        Text hpText;
        CanvasGroup group;
        Color baseColor;
        Sprite baseSprite;
        int maxHp, shownHp;
        string hpLabel;
        bool down, showHp, busy, mirror;
        float phase, bobSpeed = 2.2f;
        Vector3 squash = Vector3.one;
        public float BobSpeed { set => bobSpeed = value; }

        Sprite normalSprite, hurtSprite;      // the hurt face becomes the idle pose below 33% HP
        RectTransform pills;

        RectTransform[] stars;
        bool dizzy, flipping;

        /// <summary>Lasting statuses on the hero: bald sprite (Head Shaved), orbiting dizzy stars (Confused), pills.</summary>
        public void SetConditions(IEnumerable<string> ids)
        {
            var set = new HashSet<string>(ids);
            if (pills == null)
            {
                pills = UiKit.Make("Pills", Root);
                pills.anchorMin = pills.anchorMax = new Vector2(0.5f, 1);
                pills.pivot = new Vector2(0.5f, 0);
                pills.sizeDelta = new Vector2(420, 40);
                pills.anchoredPosition = new Vector2(0, 80);
                var hl = pills.gameObject.AddComponent<HorizontalLayoutGroup>();
                hl.spacing = 6; hl.childAlignment = TextAnchor.MiddleCenter;
                hl.childControlWidth = hl.childControlHeight = true; hl.childForceExpandWidth = hl.childForceExpandHeight = false;
            }
            // Head Shaved swaps the whole sprite (a real bald Bablu, not a cap).
            bool bald = set.Contains("shaved") && Art.HasCharacter(Id + "_shaved");
            var n = bald ? Art.Character(Id + "_shaved").sprite : Art.Character(Id).sprite;
            var h = bald ? Art.Pose(Id + "_shaved", "hit") : Art.Pose(Id, "hit");
            if (n != normalSprite)
            {
                bool wasHurt = baseSprite == hurtSprite && hurtSprite != null;
                normalSprite = n; hurtSprite = h;
                baseSprite = wasHurt && h != null ? h : n;
                if (!busy && !down) image.sprite = baseSprite;
            }
            // Confused: three comic stars orbit his head.
            dizzy = set.Contains("confused");
            if (dizzy && stars == null && Art.UI("star") != null)
            {
                stars = new RectTransform[3];
                for (int i = 0; i < 3; i++)
                {
                    var st = UiKit.Panel(body, "Star" + i, Color.white);
                    st.sprite = Art.UI("star"); st.preserveAspect = true; st.raycastTarget = false;
                    st.rectTransform.anchorMin = st.rectTransform.anchorMax = new Vector2(0.5f, 0.88f);
                    st.rectTransform.sizeDelta = new Vector2(46, 46);
                    stars[i] = st.rectTransform;
                }
            }
            if (stars != null) foreach (var st in stars) st.gameObject.SetActive(dizzy);

            foreach (Transform c in pills) Destroy(c.gameObject);
            foreach (var id in set)
            {
                var pill = UiKit.Panel(pills, "Pill", new Color(0.55f, 0.1f, 0.6f, 0.9f), rounded: true);
                var lg = pill.gameObject.AddComponent<HorizontalLayoutGroup>();
                lg.padding = new RectOffset(10, 10, 2, 2); lg.childControlWidth = lg.childControlHeight = true;
                lg.childForceExpandWidth = lg.childForceExpandHeight = false;
                var t = UiKit.Label(pill.transform, "T", Conditions.Name(id), 20, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
                t.horizontalOverflow = HorizontalWrapMode.Overflow;
            }
        }

        /// <summary>Tints the sprite (zombie green). Hit flashes return to this colour.</summary>
        public void SetTint(Color c)
        {
            if (baseColor == c) return;
            baseColor = c;
            if (image != null) image.color = c;
        }

        /// <summary>Paper Mario-style flip: the paper cutout turns edge-on, changes, and turns back.</summary>
        public IEnumerator PaperFlip(System.Action atEdge)
        {
            flipping = true;
            for (float t = 0; t < 1; t += Time.deltaTime * 6f) { squash = new Vector3(Mathf.Cos(t * Mathf.PI / 2), 1.04f, 1); yield return null; }
            atEdge?.Invoke();
            for (float t = 0; t < 1; t += Time.deltaTime * 6f) { squash = new Vector3(Mathf.Sin(t * Mathf.PI / 2) * 1.15f, 1.04f, 1); yield return null; }
            squash = new Vector3(1.15f, 0.9f, 1);
            flipping = false;
        }

        public static Puppet Create(Transform parent, string id, string displayName, string colorHex,
                                    int hp, int maxHp, string hpLabel, bool showHp, bool faceLeft)
        {
            var root = UiKit.Make("Puppet_" + id, parent);
            var p = root.gameObject.AddComponent<Puppet>();
            p.Id = id;
            p.maxHp = maxHp;
            p.hpLabel = hpLabel;
            p.showHp = showHp;
            p.FacesLeft = faceLeft;
            p.group = root.gameObject.AddComponent<CanvasGroup>();
            p.phase = Random.value * 10f;

            // Body: bottom-centre pivot so squash and bob happen on its feet.
            p.body = UiKit.Make("Body", root);
            p.body.Fill();
            p.body.pivot = new Vector2(0.5f, 0f);
            p.image = p.body.gameObject.AddComponent<Image>();
            var (sprite, placeholder) = Art.Character(id);
            p.image.sprite = p.baseSprite = p.normalSprite = sprite;
            p.hurtSprite = placeholder ? null : Art.Pose(id, "hit");
            p.image.preserveAspect = true;
            p.baseColor = placeholder ? UiKit.Hex(string.IsNullOrEmpty(colorHex) ? "#F2A65A" : colorHex) : Color.white;
            p.image.color = p.baseColor;
            p.mirror = faceLeft && !placeholder;

            // Battle damage overlay (bruises, band-aids, black eye) that builds up as HP drops.
            if (!placeholder)
            {
                p.damage = UiKit.Make("Damage", p.body).gameObject.AddComponent<Image>();
                p.damage.rectTransform.Fill();
                p.damage.preserveAspect = true;
                p.damage.raycastTarget = false;
                p.damage.enabled = false;
            }

            var name = UiKit.Label(root, "Name", displayName, 30, Color.white, TextAnchor.LowerCenter, FontStyle.Bold);
            name.rectTransform.anchorMin = new Vector2(0.5f, 1); name.rectTransform.anchorMax = new Vector2(0.5f, 1);
            name.rectTransform.pivot = new Vector2(0.5f, 0);
            name.rectTransform.sizeDelta = new Vector2(420, 80);
            name.rectTransform.anchoredPosition = new Vector2(0, 4);
            UiKit.Outline(name, new Color(0, 0, 0, 0.8f), 2);

            if (showHp)
            {
                var bar = UiKit.Panel(root, "HpBar", new Color(0, 0, 0, 0.6f), rounded: true);
                bar.rectTransform.anchorMin = new Vector2(0, 0); bar.rectTransform.anchorMax = new Vector2(1, 0);
                bar.rectTransform.pivot = new Vector2(0.5f, 1);
                bar.rectTransform.sizeDelta = new Vector2(20, 34);
                bar.rectTransform.anchoredPosition = new Vector2(0, -10);
                // "Ghost" bar lags behind the real one so chunks of damage are visible.
                p.hpGhost = UiKit.Panel(bar.transform, "Ghost", new Color(1f, 0.9f, 0.6f), rounded: true);
                p.hpGhost.rectTransform.Fill(4);
                p.hpFill = UiKit.Panel(bar.transform, "Fill", UiKit.Red, rounded: true);
                p.hpFill.rectTransform.Fill(4);
                p.hpText = UiKit.Label(bar.transform, "Text", "", 22, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
                p.hpText.rectTransform.Fill();
                UiKit.Outline(p.hpText, new Color(0, 0, 0, 0.8f), 1.5f);
            }
            p.shownHp = hp;
            p.SetHp(hp);
            return p;
        }

        float Flip => mirror ? -1f : 1f; // only real art gets mirrored (placeholder blobs are symmetric)

        void Update()
        {
            if (!down)
            {
                phase += Time.deltaTime * bobSpeed;
                float s = Mathf.Sin(phase * Mathf.PI);
                body.localScale = new Vector3(Flip * (1f + s * 0.025f) * squash.x, (1f - s * 0.035f) * squash.y, 1f);
            }
            if (!flipping) squash = Vector3.Lerp(squash, Vector3.one, Time.deltaTime * 12f);
            if (dizzy && stars != null)
                for (int i = 0; i < stars.Length; i++)
                {
                    float a = Time.time * 3.2f + i * Mathf.PI * 2 / stars.Length;
                    stars[i].anchoredPosition = new Vector2(Mathf.Cos(a) * 70f, Mathf.Sin(a) * 18f + 10f);
                    stars[i].localScale = Vector3.one * (0.8f + 0.25f * Mathf.Sin(a));   // nearer = bigger
                }

            if (showHp)
            {
                // ghost bar drains smoothly toward the real value
                float target = hpFill.rectTransform.anchorMax.x;
                var g = hpGhost.rectTransform.anchorMax;
                hpGhost.rectTransform.anchorMax = new Vector2(Mathf.MoveTowards(g.x, target, Time.deltaTime * 0.6f), g.y);
            }
        }

        public void SetHp(int hp, int newMax)
        {
            maxHp = Mathf.Max(1, newMax);
            SetHp(hp);
        }

        public void SetHp(int hp)
        {
            if (!showHp) return;
            hp = Mathf.Clamp(hp, 0, maxHp);
            if (damage != null)
            {
                float r = hp / (float)maxHp;
                var sp = r > 0.66f ? null : r > 0.33f ? Art.UI("dmg1") : Art.UI("dmg2");
                if (sp != null && damage.sprite != sp) squash = new Vector3(1.1f, 0.92f, 1); // little jolt when a new bruise appears
                damage.sprite = sp;
                damage.enabled = sp != null;
                // Match the face to the real state (change log): below a third of HP the idle pose is the hurt one.
                var want = r < 0.33f && hp > 0 && hurtSprite != null ? hurtSprite : normalSprite;
                if (want != null && baseSprite != want) { baseSprite = want; if (!busy && !down) image.sprite = want; }
            }
            var a = hpFill.rectTransform.anchorMax;
            hpFill.rectTransform.anchorMax = new Vector2(Mathf.Max(0.001f, hp / (float)maxHp), a.y);
            if (hp > shownHp) hpGhost.rectTransform.anchorMax = hpFill.rectTransform.anchorMax; // heals snap
            shownHp = hp;
            hpText.text = $"{hpLabel} {hp}/{maxHp}";
        }

        public IEnumerator DropIn(float delay)
        {
            group.alpha = 0;
            yield return new WaitForSecondsRealtime(delay);
            Vector2 home = body.anchoredPosition;
            float t = 0, dur = 0.35f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                float k = Mathf.Clamp01(t / dur);
                body.anchoredPosition = home + new Vector2(0, 700f * (1 - k) * (1 - k));
                group.alpha = Mathf.Clamp01(k * 3);
                yield return null;
            }
            body.anchoredPosition = home;
            squash = new Vector3(1.25f, 0.75f, 1);
            AudioKit.Sfx("hit", 0.4f);
        }

        public IEnumerator Lunge(float dir, float speed)
        {
            busy = true;
            Vector2 home = body.anchoredPosition;
            var attack = Art.Pose(Id, "attack");
            // anticipation: lean back
            float t = 0, wind = 0.07f / speed;
            while (t < wind) { t += Time.deltaTime; body.anchoredPosition = home - new Vector2(dir * 22f * (t / wind), 0); yield return null; }
            if (attack != null) image.sprite = attack;
            squash = new Vector3(1.15f, 0.9f, 1);
            t = 0;
            float dur = 0.16f / speed;
            while (t < dur) { t += Time.deltaTime; body.anchoredPosition = home + new Vector2(dir * 95f * Mathf.Sin(Mathf.PI * t / dur), 0); yield return null; }
            body.anchoredPosition = home;
            image.sprite = baseSprite;
            busy = false;
        }

        /// <summary>Ranged attack: a quick wind-up and flick instead of running in.</summary>
        public IEnumerator Throw(float dir, float speed)
        {
            busy = true;
            Vector2 home = body.anchoredPosition;
            var attack = Art.Pose(Id, "attack");
            float t = 0, wind = 0.08f / speed;
            while (t < wind) { t += Time.deltaTime; body.anchoredPosition = home - new Vector2(dir * 30f * (t / wind), 0); yield return null; }
            if (attack != null) image.sprite = attack;
            squash = new Vector3(1.12f, 0.92f, 1);
            body.anchoredPosition = home + new Vector2(dir * 18f, 0);
            StartCoroutine(Settle(home, 0.12f / speed, attack != null));
        }

        IEnumerator Settle(Vector2 home, float dur, bool posed)
        {
            yield return new WaitForSeconds(dur);
            body.anchoredPosition = home;
            if (posed) image.sprite = baseSprite;
            busy = false;
        }

        public void Hit(float speed, bool big)
        {
            if (!isActiveAndEnabled) return;
            squash = big ? new Vector3(0.78f, 1.22f, 1) : new Vector3(0.88f, 1.1f, 1);
            StartCoroutine(HitRoutine(speed, big));
        }

        IEnumerator HitRoutine(float speed, bool big)
        {
            var hitPose = Art.Pose(Id, "hit");
            if (hitPose != null) image.sprite = hitPose;
            Vector2 home = body.anchoredPosition;
            float t = 0, dur = (big ? 0.32f : 0.22f) / speed, amp = big ? 16f : 9f;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = t / dur;
                image.color = Color.Lerp(k < 0.25f ? Color.white * 2f : new Color(1f, 0.35f, 0.35f), baseColor, k);
                if (!busy) body.anchoredPosition = home + new Vector2(Random.Range(-amp, amp) * (1 - k), 0);
                yield return null;
            }
            image.color = baseColor;
            if (!busy) body.anchoredPosition = home;
            if (!down) image.sprite = baseSprite;
        }

        public IEnumerator Die(float speed)
        {
            down = true;
            float t = 0, dur = 0.45f / speed;
            float dir = Root.anchorMin.x > 0.5f ? -1 : 1;
            Vector2 home = body.anchoredPosition;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = t / dur;
                body.localRotation = Quaternion.Euler(0, 0, dir * 85f * k);
                body.anchoredPosition = home + new Vector2(-dir * 40f * k, 30f * Mathf.Sin(Mathf.PI * k));
                group.alpha = 1f - 0.6f * k;
                yield return null;
            }
        }

        public IEnumerator StandUp(float speed)
        {
            float t = 0, dur = 0.35f / speed;
            var from = body.localRotation;
            var fromPos = body.anchoredPosition;
            while (t < dur)
            {
                t += Time.deltaTime;
                float k = t / dur;
                body.localRotation = Quaternion.Slerp(from, Quaternion.identity, k);
                body.anchoredPosition = Vector2.Lerp(fromPos, Vector2.zero, k);
                group.alpha = Mathf.Lerp(group.alpha, 1, k);
                yield return null;
            }
            body.localRotation = Quaternion.identity;
            body.anchoredPosition = Vector2.zero;
            group.alpha = 1;
            squash = new Vector3(1.2f, 0.8f, 1);
            down = false;
        }

        /// <summary>Position (in the given space) just above the head, for popups.</summary>
        public Vector2 HeadPosition(RectTransform space) => PointIn(space, 0.5f, 1f);

        /// <summary>Position of a normalised point on this puppet's rect, in the given space.</summary>
        public Vector2 PointIn(RectTransform space, float nx, float ny)
        {
            var corners = new Vector3[4];
            Root.GetWorldCorners(corners); // BL, TL, TR, BR
            var p = Vector3.Lerp(Vector3.Lerp(corners[0], corners[3], nx), Vector3.Lerp(corners[1], corners[2], nx), ny);
            return space.InverseTransformPoint(p);
        }
    }
}
