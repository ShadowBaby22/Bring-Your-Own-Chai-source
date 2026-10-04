using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace KadakChai
{
    public enum CardStyle { Story, Header, Event, Good, Bad, Note }

    /// <summary>
    /// The story log, shown as chat bubbles in the phone's Messages app. Story and events arrive on the left,
    /// your wins on the right, notes and day headers in the middle. Bubbles fade in and auto-scroll.
    /// </summary>
    public class StoryLog : MonoBehaviour, UnityEngine.EventSystems.IBeginDragHandler, UnityEngine.EventSystems.IEndDragHandler, UnityEngine.EventSystems.IScrollHandler
    {
        bool stickToBottom = true; // follow new messages until the player drags to read older ones
        const int MaxCards = 40;

        ScrollRect scroll;
        RectTransform content;
        public event System.Action Added;

        /// <summary>Fills the given container (the Messages app page).</summary>
        public static StoryLog Build(RectTransform parent)
        {
            var frame = UiKit.Panel(parent, "Log", new Color(0, 0, 0, 0.18f), rounded: true);
            frame.rectTransform.Fill();
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
                CardStyle.Event => UiKit.Hex("#FFF3C4"),
                CardStyle.Good => UiKit.Hex("#DCF8C6"),   // "sent" bubble
                CardStyle.Bad => UiKit.Blush,
                CardStyle.Header => new Color(0, 0, 0, 0.45f),
                CardStyle.Note => Color.clear,
                _ => UiKit.Cream,
            };
            // A row positions the bubble: left (incoming), right (your wins) or centred (headers, notes).
            var row = UiKit.Make("Row", content);
            var rowLayout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            rowLayout.childControlHeight = true; rowLayout.childControlWidth = true;
            rowLayout.childForceExpandHeight = false; rowLayout.childForceExpandWidth = true;
            rowLayout.padding = style switch
            {
                CardStyle.Good => new RectOffset(110, 6, 0, 0),
                CardStyle.Header => new RectOffset(150, 150, 0, 0),
                CardStyle.Note => new RectOffset(30, 30, 0, 0),
                _ => new RectOffset(6, 110, 0, 0),
            };

            var card = UiKit.Panel(row, "Card", bg, rounded: true);
            var pad = style == CardStyle.Note ? 2 : style == CardStyle.Header ? 8 : 18;
            var layout = card.gameObject.AddComponent<VerticalLayoutGroup>();
            layout.padding = new RectOffset(pad + 8, pad + 8, pad, pad);
            layout.childControlHeight = true; layout.childControlWidth = true;
            layout.childForceExpandHeight = false;

            int size = style switch { CardStyle.Header => 28, CardStyle.Note => 28, _ => 34 }; // sized for phones
            Color color = style switch { CardStyle.Header => Color.white, CardStyle.Note => new Color(1, 1, 1, 0.85f), _ => UiKit.Ink };
            var anchor = style == CardStyle.Header || style == CardStyle.Note ? TextAnchor.UpperCenter : TextAnchor.UpperLeft;
            var label = UiKit.Label(card.transform, "Text", UiKit.Rich(text), size, color, anchor,
                                    style == CardStyle.Header ? FontStyle.Bold : FontStyle.Normal);
            if (style == CardStyle.Note) UiKit.Outline(label, new Color(0, 0, 0, 0.5f), 2);
            label.lineSpacing = 1.1f;
            if (style != CardStyle.Header && style != CardStyle.Note)
            {
                label.gameObject.AddComponent<TextFx>().StartReveal(110f); // quick typewriter
                AudioKit.Sfx("card", 0.6f);
            }

            while (content.childCount > MaxCards) DestroyImmediate(content.GetChild(0).gameObject);
            if (isActiveAndEnabled) StartCoroutine(FadeIn(row.gameObject.AddComponent<CanvasGroup>()));
            stickToBottom = true;
            Added?.Invoke();
        }

        /// <summary>Jump to the newest message (when the Messages app is reopened).</summary>
        public void ScrollToEnd()
        {
            if (!isActiveAndEnabled) return;
            stickToBottom = true;
            Canvas.ForceUpdateCanvases();
            scroll.verticalNormalizedPosition = 0;
        }

        IEnumerator FadeIn(CanvasGroup g)
        {
            g.alpha = 0;
            for (float t = 0; t < 1; t += Time.deltaTime * 4f)
            {
                if (g == null) yield break;
                g.alpha = t;
                yield return null;
            }
            if (g != null) g.alpha = 1;
        }

        public void OnBeginDrag(UnityEngine.EventSystems.PointerEventData e) => stickToBottom = false;
        // Back at the bottom after a drag or the mouse wheel: follow new messages again.
        public void OnEndDrag(UnityEngine.EventSystems.PointerEventData e) { if (AtBottom) stickToBottom = true; }
        /// <summary>The mouse wheel on desktop: it used to snap straight back to the newest line every frame.</summary>
        public void OnScroll(UnityEngine.EventSystems.PointerEventData e)
        {
            if (e.scrollDelta.y > 0.01f) stickToBottom = false;
            else if (AtBottom) stickToBottom = true;
        }
        bool AtBottom => scroll == null || scroll.verticalNormalizedPosition <= 0.02f;

        void LateUpdate()
        {
            // Typewriter text grows bubbles every frame; keep the newest line in view (no fighting coroutines).
            if (!stickToBottom || content == null) return;
            Canvas.ForceUpdateCanvases();
            scroll.verticalNormalizedPosition = 0;
            scroll.velocity = Vector2.zero;
        }
    }
}
