using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace KadakChai
{
    public enum CardStyle { Story, Header, Event, Good, Bad, Note }

    /// <summary>The scrolling story log in the bottom half (Capybara Go style). Cards fade in and auto-scroll.</summary>
    public class StoryLog : MonoBehaviour
    {
        const int MaxCards = 40;

        ScrollRect scroll;
        RectTransform content;

        public static StoryLog Build(RectTransform phone)
        {
            var frame = UiKit.Panel(phone, "Log", new Color(0, 0, 0, 0.25f), rounded: true);
            frame.rectTransform.Place(24, 1078, 1032, 634);
            var log = frame.gameObject.AddComponent<StoryLog>();

            var viewport = UiKit.Make("Viewport", frame.transform);
            viewport.Fill(10);
            viewport.gameObject.AddComponent<RectMask2D>();

            log.content = UiKit.Make("Content", viewport);
            log.content.anchorMin = new Vector2(0, 1); log.content.anchorMax = new Vector2(1, 1);
            log.content.pivot = new Vector2(0.5f, 1);
            log.content.sizeDelta = Vector2.zero;
            var vlg = log.content.gameObject.AddComponent<VerticalLayoutGroup>();
            vlg.spacing = 14;
            vlg.padding = new RectOffset(4, 4, 6, 6);
            vlg.childControlHeight = true; vlg.childControlWidth = true;
            vlg.childForceExpandHeight = false; vlg.childForceExpandWidth = true;
            log.content.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            log.scroll = frame.gameObject.AddComponent<ScrollRect>();
            log.scroll.viewport = viewport;
            log.scroll.content = log.content;
            log.scroll.horizontal = false;
            log.scroll.movementType = ScrollRect.MovementType.Clamped;
            log.scroll.scrollSensitivity = 30;
            return log;
        }

        public void Clear()
        {
            foreach (Transform c in content) Destroy(c.gameObject);
        }

        public void Add(string text, CardStyle style = CardStyle.Story)
        {
            if (string.IsNullOrEmpty(text)) return;

            Color bg = style switch
            {
                CardStyle.Event => UiKit.Lavender,
                CardStyle.Good => UiKit.Mint,
                CardStyle.Bad => UiKit.Blush,
                CardStyle.Header or CardStyle.Note => Color.clear,
                _ => UiKit.Cream,
            };
            var card = UiKit.Panel(content, "Card", bg, rounded: true);
            var pad = style == CardStyle.Header || style == CardStyle.Note ? 4 : 22;
            var layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(pad + 6, pad + 6, pad, pad);
            layout.childControlHeight = true; layout.childControlWidth = true;
            layout.childForceExpandHeight = false;

            int size = style switch { CardStyle.Header => 36, CardStyle.Note => 30, _ => 38 }; // sized for phones
            Color color = style switch { CardStyle.Header => Color.white, CardStyle.Note => new Color(1, 1, 1, 0.8f), _ => UiKit.Ink };
            var label = UiKit.Label(card.transform, "Text", UiKit.Rich(text), size, color, TextAnchor.UpperLeft,
                                    style == CardStyle.Header ? FontStyle.Bold : FontStyle.Normal);
            label.lineSpacing = 1.1f;
            if (style != CardStyle.Header && style != CardStyle.Note)
            {
                label.gameObject.AddComponent<TextFx>().StartReveal(110f); // quick typewriter
                AudioKit.Sfx("card", 0.6f);
            }

            while (content.childCount > MaxCards) DestroyImmediate(content.GetChild(0).gameObject);
            StartCoroutine(FadeInAndScroll(card.gameObject.AddComponent<CanvasGroup>()));
        }

        IEnumerator FadeInAndScroll(CanvasGroup g)
        {
            g.alpha = 0;
            yield return null; // let layout run
            Canvas.ForceUpdateCanvases();
            float start = scroll.verticalNormalizedPosition;
            for (float t = 0; t < 1; t += Time.deltaTime * 4f)
            {
                if (g == null) yield break;
                g.alpha = t;
                scroll.verticalNormalizedPosition = Mathf.Lerp(start, 0, t);
                yield return null;
            }
            if (g != null) g.alpha = 1;
            scroll.verticalNormalizedPosition = 0;
        }
    }
}
