using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KadakChai
{
    /// <summary>
    /// The sticker sheet (Album): a full-screen card like a printed sticker sheet. Collected stickers are die-cut art;
    /// missing ones are white cut-outs with a hint. The player screenshots it to keep or share it.
    /// </summary>
    public static class StickerSheet
    {
        const float CW = 1020, CH = 1560;   // the card, in reference pixels

        public static void Show(Transform parent, Action onClose = null)
        {
            var overlay = UiKit.Panel(parent, "StickerSheet", new Color(0, 0, 0, 0.92f));
            overlay.rectTransform.Fill();

            var card = UiKit.Panel(overlay.transform, "Card", Color.white, rounded: true);
            card.rectTransform.Place(30, 60, CW, CH);
            card.gameObject.AddComponent<RectMask2D>();
            Image Box(string name, float x, float y, float w, float h, Color c)
            {
                var i = UiKit.Panel(card.transform, name, c);
                i.raycastTarget = false;
                i.rectTransform.Place(x, y, w, h, CW, CH);
                return i;
            }
            Box("Sky", 14, 14, CW - 28, CH - 28, UiKit.Hex("#7EC8F2"));
            var city = Box("City", 14, CH - 14 - 620, CW - 28, 620, Color.white);
            city.sprite = Art.Background("dilli").sprite;
            city.preserveAspect = false;
            Box("Grass", 14, CH - 14 - 150, CW - 28, 150, UiKit.Hex("#7CB342"));

            var logo = UiKit.Label(card.transform, "Logo", "KADAK <color=#FFFFFF>Chai</color>", 92, UiKit.Hex("#FFE14D"), TextAnchor.MiddleLeft, FontStyle.Bold);
            logo.rectTransform.Place(40, 26, 640, 120, CW, CH);
            UiKit.Outline(logo, Color.black, 6);
            var sub = UiKit.Label(card.transform, "Sub", $"Bablu's sticker sheet\n<b>{Album.Count} / {Album.Total}</b> collected", 34, Color.white, TextAnchor.MiddleRight, FontStyle.Bold);
            sub.rectTransform.Place(560, 34, 430, 110, CW, CH);
            UiKit.Outline(sub, new Color(0, 0, 0, 0.75f), 3);

            const int Cols = 5;
            float cellW = (CW - 60) / Cols, cellH = 268, top = 168;
            for (int i = 0; i < Album.All.Length; i++)
            {
                var st = Album.All[i];
                int row = i / Cols, col = i % Cols;
                float jitter = ((i * 53) % 21) - 10;
                float cx = 30 + col * cellW + cellW / 2, cy = top + row * cellH + cellH / 2 - 16 + jitter;
                float size = st.big ? 228 : 196;
                bool have = Album.Has(st.id);
                var img = UiKit.Panel(card.transform, "Sticker_" + st.id, Color.white);
                img.sprite = Art.Sticker(have ? st.id : st.id + "_blank");
                img.preserveAspect = true;
                img.raycastTarget = false;
                img.rectTransform.Place(cx - size / 2, cy - size / 2, size, size, CW, CH);
                img.rectTransform.localRotation = Quaternion.Euler(0, 0, ((i * 37) % 13) - 6);
                if (img.sprite == null) img.color = new Color(1, 1, 1, have ? 0.9f : 0.5f);

                var label = UiKit.Label(card.transform, "Label", have ? st.name : st.how, have ? 27 : 25,
                                        have ? Color.white : new Color(1, 1, 1, 0.92f), TextAnchor.UpperCenter, have ? FontStyle.Bold : FontStyle.Italic);
                label.rectTransform.Place(cx - cellW / 2 + 4, cy + size / 2 - 6, cellW - 8, 56, CW, CH);
                UiKit.Outline(label, new Color(0, 0, 0, 0.8f), 2);

                if (have && Album.NewThisRun.Contains(st.id))
                {
                    var badge = UiKit.Panel(card.transform, "New", UiKit.Red, rounded: true);
                    badge.raycastTarget = false;
                    badge.rectTransform.Place(cx + size / 2 - 76, cy - size / 2 + 2, 76, 38, CW, CH);
                    var bt = UiKit.Label(badge.transform, "T", "NEW", 24, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
                    bt.rectTransform.Fill();
                }
            }

            var prompt = UiKit.Label(overlay.transform, "Prompt", Album.Count < Album.Total
                ? "<b>Screenshot this sheet to keep it!</b>\nStickers stay across runs. The white ones are still out there."
                : "<b>Full sheet! Screenshot it and show off.</b>", 32, Color.white);
            prompt.rectTransform.Place(40, 1632, 1000, 110);
            var close = UiKit.Button(overlay.transform, "Close", "Close", UiKit.Green, 46, () =>
            {
                UnityEngine.Object.Destroy(overlay.gameObject);
                onClose?.Invoke();
            });
            close.GetComponent<RectTransform>().Place(320, 1760, 440, 130);
        }

        /// <summary>A sticker that pops in at the right of the battle area: "NEW STICKER!".</summary>
        public static IEnumerator Toast(Transform parent, List<StickerData> fresh)
        {
            if (fresh == null || fresh.Count == 0) yield break;
            var st = fresh[0];
            var root = UiKit.Panel(parent, "StickerToast", new Color(0, 0, 0, 0.7f), rounded: true);
            root.raycastTarget = false;
            root.rectTransform.Place(720, 250, 330, 360);
            var img = UiKit.Panel(root.transform, "Art", Color.white);
            img.sprite = Art.Sticker(st.id);
            img.preserveAspect = true;
            img.raycastTarget = false;
            img.rectTransform.Place(45, 60, 240, 220, 330, 360);
            var head = UiKit.Label(root.transform, "Head", "NEW STICKER!", 34, UiKit.Hex("#FFE14D"), TextAnchor.MiddleCenter, FontStyle.Bold);
            head.rectTransform.Place(0, 8, 330, 52, 330, 360);
            string more = fresh.Count > 1 ? $"  +{fresh.Count - 1} more" : "";
            var name = UiKit.Label(root.transform, "Name", $"{st.name}{more}\n<size=22>{Album.Count}/{Album.Total}  ·  Profile > Stickers</size>", 28, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            name.rectTransform.Place(6, 278, 318, 78, 330, 360);
            AudioKit.Sfx("coin", 0.8f);

            var rt = root.rectTransform;
            var group = root.gameObject.AddComponent<CanvasGroup>();
            for (float t = 0; t < 2.6f; t += Time.unscaledDeltaTime)
            {
                if (root == null) yield break;
                float pop = t < 0.25f ? Mathf.Lerp(0.3f, 1.08f, t / 0.25f) : t < 0.4f ? Mathf.Lerp(1.08f, 1f, (t - 0.25f) / 0.15f) : 1f;
                rt.localScale = Vector3.one * pop;
                rt.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(t * 6f) * 3f * Mathf.Max(0, 1 - t));
                group.alpha = t > 2.2f ? 1 - (t - 2.2f) / 0.4f : 1;
                yield return null;
            }
            if (root != null) UnityEngine.Object.Destroy(root.gameObject);
        }
    }
}
