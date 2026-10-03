using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KadakChai
{
    /// <summary>
    /// Visual-novel overlay for boss intros, key events and the ending (scenes.csv).
    /// Big portraits slide in, the speaker is highlighted, text types out with voice blips. Tap to advance.
    /// </summary>
    public class VNPlayer : MonoBehaviour
    {
        RectTransform root;
        CanvasGroup group;
        Image leftImg, rightImg, plate, box;
        Text nameText, bodyText, hint;
        TextFx fx;
        string leftId, rightId;
        bool tapped, skip;
        string voice = "mid";
        int blipCounter;

        /// <summary>UI that must stay reachable above scenes (the Options button).</summary>
        public Transform keepOnTop;

        public static VNPlayer Build(RectTransform phone)
        {
            var root = UiKit.Make("VN", phone);
            root.Fill();
            var vn = root.gameObject.AddComponent<VNPlayer>();
            vn.root = root;
            vn.group = root.gameObject.AddComponent<CanvasGroup>();

            // Full-screen tap catcher (also dims the game behind).
            var dim = UiKit.Panel(root, "Dim", new Color(0, 0, 0, 0.55f));
            dim.rectTransform.Fill();
            dim.gameObject.AddComponent<Button>().onClick.AddListener(() => vn.tapped = true);

            vn.leftImg = Portrait(root, "Left");
            vn.leftImg.rectTransform.Place(-40, 500, 600, 840);
            vn.rightImg = Portrait(root, "Right");
            vn.rightImg.rectTransform.Place(520, 500, 600, 840);
            vn.rightImg.rectTransform.localScale = new Vector3(-1, 1, 1); // everyone is drawn facing right

            vn.box = UiKit.Panel(root, "Box", UiKit.Cream, rounded: true);
            vn.box.rectTransform.Place(30, 1310, 1020, 430);
            vn.box.raycastTarget = false;
            UiKit.Outline(vn.box, UiKit.Ink, 6);

            vn.plate = UiKit.Panel(root, "Plate", UiKit.Orange, rounded: true);
            vn.plate.rectTransform.Place(60, 1250, 440, 90);
            vn.plate.raycastTarget = false;
            UiKit.Outline(vn.plate, UiKit.Ink, 5);
            vn.nameText = UiKit.Label(vn.plate.transform, "Name", "", 44, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            vn.nameText.rectTransform.Fill(8);
            UiKit.Outline(vn.nameText, new Color(0, 0, 0, 0.5f), 2);

            vn.bodyText = UiKit.Label(vn.box.transform, "Body", "", 46, UiKit.Ink, TextAnchor.UpperLeft);
            vn.bodyText.rectTransform.Fill(44);
            vn.bodyText.rectTransform.offsetMax = new Vector2(-44, -64);
            vn.bodyText.lineSpacing = 1.15f;
            vn.fx = vn.bodyText.gameObject.AddComponent<TextFx>();
            vn.fx.OnGlyph = _ => { if (++vn.blipCounter % 2 == 0) AudioKit.Sfx("blip_" + vn.voice, 0.5f); };

            vn.hint = UiKit.Label(vn.box.transform, "Hint", "tap >>", 30, new Color(0.5f, 0.4f, 0.3f), TextAnchor.LowerRight, FontStyle.Bold);
            vn.hint.rectTransform.Fill(24);

            var skipBtn = UiKit.Button(root, "Skip", "Skip >>", new Color(0, 0, 0, 0.5f), 30, () => vn.skip = true);
            skipBtn.GetComponent<RectTransform>().Place(860, 200, 190, 70);

            root.gameObject.SetActive(false);
            return vn;
        }

        static Image Portrait(RectTransform root, string name)
        {
            var img = UiKit.Panel(root, name, Color.white);
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.rectTransform.pivot = new Vector2(0.5f, 0f);
            img.enabled = false;
            return img;
        }

        public IEnumerator Play(List<SceneLine> lines)
        {
            if (lines == null || lines.Count == 0) yield break;
            root.gameObject.SetActive(true);
            root.SetAsLastSibling();
            if (keepOnTop != null) keepOnTop.SetAsLastSibling();
            leftId = rightId = null;
            leftImg.enabled = rightImg.enabled = false;
            bodyText.text = ""; nameText.text = ""; // no flash of the previous scene's last line while fading in
            plate.gameObject.SetActive(false);
            skip = false;
            for (float t = 0; t < 1; t += Time.unscaledDeltaTime * 5f) { group.alpha = t; yield return null; }
            group.alpha = 1;

            foreach (var line in lines)
            {
                if (skip) break;
                yield return ShowLine(line);
                tapped = false;
                while (fx.Revealing && !tapped && !skip) yield return null; // typing; a tap completes it
                if (tapped) { fx.Complete(); tapped = false; }
                hint.enabled = true;
                while (!tapped && !skip) yield return null;                  // wait for the next tap
                AudioKit.Sfx("click", 0.5f);
            }

            for (float t = 1; t > 0; t -= Time.unscaledDeltaTime * 5f) { group.alpha = t; yield return null; }
            root.gameObject.SetActive(false);
        }

        IEnumerator ShowLine(SceneLine line)
        {
            hint.enabled = false;
            bool narrator = line.IsNarrator;
            bool left = line.side == "left" || (string.IsNullOrEmpty(line.side) && line.speaker == "new_kid");

            if (!narrator && Art.HasCharacter(line.speaker))
            {
                if (left && leftId != line.speaker) { leftId = line.speaker; yield return SlideIn(leftImg, line.speaker, -1); }
                if (!left && rightId != line.speaker) { rightId = line.speaker; yield return SlideIn(rightImg, line.speaker, 1); }
            }

            // Highlight the speaker, dim the other.
            Highlight(leftImg, !narrator && left);
            Highlight(rightImg, !narrator && !left);

            plate.gameObject.SetActive(!narrator);
            if (!narrator)
            {
                nameText.text = GameData.SpeakerName(line.speaker);
                var col = GameData.Combatants.TryGetValue(line.speaker, out var cd) && !string.IsNullOrEmpty(cd.color) ? UiKit.Hex(cd.color) : UiKit.Orange;
                plate.color = Color.Lerp(col, UiKit.Ink, 0.25f);
                plate.rectTransform.Place(left ? 60 : 580, 1250, 440, 90);
                voice = cd?.voice ?? "mid";
            }
            else voice = "lo";

            bodyText.fontStyle = narrator ? FontStyle.Italic : FontStyle.Normal;
            bodyText.text = UiKit.Rich(line.text);
            blipCounter = 0;
            fx.StartReveal(45f);
        }

        IEnumerator SlideIn(Image img, string id, float dir)
        {
            img.sprite = Art.Character(id).sprite;
            img.enabled = true;
            var rt = img.rectTransform;
            Vector2 home = rt.anchoredPosition;
            for (float t = 0; t < 1; t += Time.unscaledDeltaTime * 5f)
            {
                float k = 1 - (1 - t) * (1 - t);
                rt.anchoredPosition = home + new Vector2(dir * 500 * (1 - k), 0);
                yield return null;
            }
            rt.anchoredPosition = home;
        }

        void Highlight(Image img, bool active)
        {
            if (!img.enabled) return;
            img.color = active ? Color.white : new Color(0.45f, 0.45f, 0.5f);
            float flip = Mathf.Sign(img.rectTransform.localScale.x);
            img.rectTransform.localScale = new Vector3(flip, 1, 1) * (active ? 1.04f : 0.94f);
            if (active) img.transform.SetSiblingIndex(box.transform.GetSiblingIndex()); // in front of the other portrait, behind the box
        }

        void Update()
        {
            // Talking bob for whoever is speaking.
            foreach (var img in new[] { leftImg, rightImg })
            {
                if (img == null || !img.enabled || img.color != Color.white) continue;
                float bob = fx != null && fx.Revealing ? Mathf.Abs(Mathf.Sin(Time.unscaledTime * 14f)) * 10f : 0f;
                var p = img.rectTransform.anchoredPosition;
                img.rectTransform.anchoredPosition = new Vector2(p.x, bob);
            }
        }
    }
}
