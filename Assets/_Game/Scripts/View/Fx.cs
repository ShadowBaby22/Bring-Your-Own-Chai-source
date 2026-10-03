using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace KadakChai
{
    /// <summary>Cheap UI particles: sparks on hits, plus-signs on heals, dust on landings and KOs, confetti on wins.</summary>
    public static class Fx
    {
        public static void Burst(MonoBehaviour host, RectTransform parent, Vector2 at, Color color, int count,
                                 float speed = 600f, float size = 22f, float life = 0.5f, float gravity = 1400f, bool square = false)
        {
            for (int i = 0; i < count; i++)
            {
                var img = UiKit.Panel(parent, "Particle", color);
                img.raycastTarget = false;
                if (!square) img.sprite = Art.Dot;
                var rt = img.rectTransform;
                float s = size * Random.Range(0.6f, 1.25f);
                rt.sizeDelta = new Vector2(s, s);
                rt.anchoredPosition = at;
                rt.localRotation = Quaternion.Euler(0, 0, Random.Range(0f, 360f));
                Vector2 v = Random.insideUnitCircle.normalized * speed * Random.Range(0.4f, 1f);
                v.y += speed * 0.35f;
                host.StartCoroutine(Fly(rt, img, v, life * Random.Range(0.7f, 1.2f), gravity));
            }
        }

        static IEnumerator Fly(RectTransform rt, Image img, Vector2 v, float life, float gravity)
        {
            Color c = img.color;
            float spin = Random.Range(-400f, 400f);
            for (float t = 0; t < life; t += Time.unscaledDeltaTime)
            {
                if (rt == null) yield break;
                v.y -= gravity * Time.unscaledDeltaTime;
                rt.anchoredPosition += v * Time.unscaledDeltaTime;
                rt.Rotate(0, 0, spin * Time.unscaledDeltaTime);
                float k = t / life;
                img.color = new Color(c.r, c.g, c.b, c.a * (1 - k * k));
                rt.localScale = Vector3.one * (1 - 0.5f * k);
                yield return null;
            }
            if (rt != null) Object.Destroy(rt.gameObject);
        }

        public static readonly Color[] Confetti =
        {
            UiKit.Hex("#E8590C"), UiKit.Hex("#43A047"), UiKit.Hex("#1E88E5"), UiKit.Hex("#FDD835"), UiKit.Hex("#E91E63"),
        };
    }
}
