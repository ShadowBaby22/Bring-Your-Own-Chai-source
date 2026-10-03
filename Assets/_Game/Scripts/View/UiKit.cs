using System;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.UI;

namespace KadakChai
{
    /// <summary>
    /// Builds uGUI from code so the whole game lives in one scene with zero prefab wiring.
    /// Layout uses reference pixels on a 1080x1920 portrait "phone", origin top-left.
    /// </summary>
    public static class UiKit
    {
        public const float W = 1080, H = 1920;

        public static readonly Color Ink = Hex("#2B2B2B");
        public static readonly Color Cream = Hex("#FFF4DC");
        public static readonly Color Night = Hex("#1E2233");
        public static readonly Color Orange = Hex("#E8590C");
        public static readonly Color Green = Hex("#43A047");
        public static readonly Color Red = Hex("#E53935");
        public static readonly Color Purple = Hex("#7E57C2");
        public static readonly Color Lavender = Hex("#EDE7F6");
        public static readonly Color Mint = Hex("#E8F5E9");
        public static readonly Color Blush = Hex("#FFEBEE");

        static Font font;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() => font = null; // no domain reload between Play sessions
        public static Font Font
        {
            get
            {
                if (font != null) return font;
                // Drop any .ttf into Resources/Fonts (e.g. Baloo 2) and it's used everywhere.
                var custom = Resources.LoadAll<Font>("Fonts");
                font = custom.Length > 0 ? custom[0] : Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
                return font;
            }
        }

        public static Color Hex(string hex) => ColorUtility.TryParseHtmlString(hex, out var c) ? c : Color.magenta;

        public static RectTransform Make(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }

        /// <summary>Place in reference pixels relative to a parent of size pw x ph (default: the phone).</summary>
        public static RectTransform Place(this RectTransform rt, float x, float y, float w, float h, float pw = W, float ph = H)
        {
            rt.anchorMin = new Vector2(x / pw, 1f - (y + h) / ph);
            rt.anchorMax = new Vector2((x + w) / pw, 1f - y / ph);
            rt.offsetMin = rt.offsetMax = Vector2.zero;
            return rt;
        }

        public static RectTransform Fill(this RectTransform rt, float inset = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = new Vector2(inset, inset);
            rt.offsetMax = new Vector2(-inset, -inset);
            return rt;
        }

        public static Image Panel(Transform parent, string name, Color color, bool rounded = false)
        {
            var img = Make(name, parent).gameObject.AddComponent<Image>();
            img.color = color;
            if (rounded) { img.sprite = Art.Rounded; img.type = Image.Type.Sliced; }
            return img;
        }

        public static Text Label(Transform parent, string name, string text, int size, Color color,
                                 TextAnchor anchor = TextAnchor.MiddleCenter, FontStyle style = FontStyle.Normal)
        {
            var t = Make(name, parent).gameObject.AddComponent<Text>();
            t.font = Font;
            t.text = text;
            t.fontSize = size;
            t.color = color;
            t.alignment = anchor;
            t.fontStyle = style;
            t.supportRichText = true;
            t.horizontalOverflow = HorizontalWrapMode.Wrap;
            t.verticalOverflow = VerticalWrapMode.Overflow;
            t.raycastTarget = false;
            return t;
        }

        public static Button Button(Transform parent, string name, string text, Color bg, int size, Action onClick)
        {
            var img = Panel(parent, name, bg, rounded: true);
            var b = img.gameObject.AddComponent<Button>();
            b.targetGraphic = img;
            var label = Label(img.transform, "Label", text, size, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            label.rectTransform.Fill(12);
            Outline(label, new Color(0, 0, 0, 0.35f), 2);
            b.onClick.AddListener(() => { AudioKit.Sfx("click"); onClick?.Invoke(); });
            return b;
        }

        public static void SetText(this Button b, string text) => b.GetComponentInChildren<Text>().text = text;

        public static void Outline(Graphic g, Color c, float px)
        {
            var o = g.gameObject.AddComponent<Outline>();
            o.effectColor = c;
            o.effectDistance = new Vector2(px, -px);
        }

        static readonly Regex Highlight = new(@"\*(.+?)\*");

        /// <summary>*word* becomes bold orange, matching Capybara Go's keyword highlights.</summary>
        public static string Rich(string text) =>
            Highlight.Replace(text ?? "", "<color=#E8590C><b>$1</b></color>");
    }
}
