using UnityEngine;
using UnityEngine.UI;

namespace KadakChai
{
    /// <summary>
    /// Small looping motions for the title screen, all optional and combinable:
    /// spin, bob, sway, pulse, drift, a pop-in with overshoot, rising steam and twinkling.
    /// </summary>
    public class TitleFx : MonoBehaviour
    {
        public float spin;                      // degrees per second
        public float bob, bobSpeed = 2f;        // px up and down
        public float sway, swaySpeed = 1.5f;    // degrees of rocking
        public float pulse, pulseSpeed = 3f;    // scale amount (0.05 = 5%)
        public float drift, driftSpeed = 0.2f;  // px side to side
        public float popDelay = -1f;            // seconds; 0 or more = grow in from nothing with an overshoot
        public float rise, riseTime = 2f;       // steam: float up this many px while fading, then start again
        public float twinkle;                   // alpha flicker speed
        public float phase;                     // seconds, to desync copies

        RectTransform rt;
        Graphic graphic;
        Vector2 basePos;
        Quaternion baseRot;
        Vector3 baseScale;
        float baseAlpha, start;

        void Start()
        {
            rt = (RectTransform)transform;
            graphic = GetComponent<Graphic>();
            basePos = rt.anchoredPosition;
            baseRot = rt.localRotation;
            baseScale = rt.localScale;
            baseAlpha = graphic != null ? graphic.color.a : 1f;
            start = Time.unscaledTime;
            if (popDelay >= 0) rt.localScale = Vector3.zero;
        }

        void Update()
        {
            float age = Time.unscaledTime - start, t = age + phase;
            var pos = basePos;
            var rot = baseRot;
            float s = 1f, alpha = baseAlpha;

            if (spin != 0) rot *= Quaternion.Euler(0, 0, -spin * t);
            if (sway != 0) rot *= Quaternion.Euler(0, 0, Mathf.Sin(t * swaySpeed) * sway);
            if (bob != 0) pos.y += Mathf.Sin(t * bobSpeed) * bob;
            if (drift != 0) pos.x += Mathf.Sin(t * driftSpeed) * drift;
            if (pulse != 0) s *= 1f + Mathf.Sin(t * pulseSpeed) * pulse;
            if (popDelay >= 0) s *= BackOut(Mathf.Clamp01((age - popDelay) / 0.45f));
            if (rise != 0)
            {
                float k = Mathf.Repeat(t / riseTime, 1f);
                pos.y += k * rise;
                s *= 0.6f + 0.8f * k;
                alpha = baseAlpha * Mathf.Sin(k * Mathf.PI);
            }
            if (twinkle != 0) alpha = baseAlpha * (0.25f + 0.75f * Mathf.Abs(Mathf.Sin(t * twinkle)));

            rt.anchoredPosition = pos;
            rt.localRotation = rot;
            rt.localScale = baseScale * s;
            if (graphic != null && (rise != 0 || twinkle != 0))
            {
                var c = graphic.color;
                c.a = alpha;
                graphic.color = c;
            }
        }

        static float BackOut(float k)
        {
            const float c1 = 1.70158f, c3 = c1 + 1f;
            float x = k - 1f;
            return 1f + c3 * x * x * x + c1 * x * x;
        }
    }
}
