using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KadakChai
{
    /// <summary>
    /// The NCR map (GDD: Map and toll tax). Shown at every border: the hero walks from one city to the next,
    /// stops at the toll plaza, and takes the Toll Tax (or beeps through with a FASTag).
    /// Coordinates are in the 1000x1000 map image (Art/UI/map.png, drawn by Art/src/ui.py).
    /// </summary>
    public static class MapView
    {
        const float MX = 40, MY = 300, MS = 1000; // where the map sits on the phone

        public static readonly Dictionary<string, Vector2> Cities = new()
        {
            ["dilli"] = new Vector2(470, 330),
            ["gurugram"] = new Vector2(230, 700),
            ["noida"] = new Vector2(790, 610),
            ["daftar"] = new Vector2(560, 175),
        };

        public static readonly Dictionary<string, (Vector2 at, string name)> Tolls = new()
        {
            ["dilli>gurugram"] = (new Vector2(340, 525), "Sirhaul Toll Plaza"),
            ["gurugram>noida"] = (new Vector2(620, 600), "DND Flyway"),
            ["noida>daftar"] = (new Vector2(700, 380), "Delhi Border"),
        };

        public static IEnumerator Cross(RectTransform phone, string from, string to, bool fastag)
        {
            var dim = UiKit.Panel(phone, "Map", new Color(0.05f, 0.06f, 0.1f, 0.96f));
            dim.rectTransform.Fill();
            var root = dim.rectTransform;
            var group = root.gameObject.AddComponent<CanvasGroup>();

            var head = UiKit.Label(root, "Head", "Crossing the border", 64, ShopView.Brand, TextAnchor.MiddleCenter, FontStyle.Bold);
            head.rectTransform.Place(40, 110, 1000, 100);
            UiKit.Outline(head, Color.black, 4);
            var sub = UiKit.Label(root, "Sub", $"{GameData.ZoneName(from)}  →  {GameData.ZoneName(to)}", 36, Color.white);
            sub.rectTransform.Place(40, 210, 1000, 70);

            var map = UiKit.Panel(root, "MapImage", Color.white);
            map.sprite = Art.UI("map");
            map.preserveAspect = true;
            map.rectTransform.Place(MX, MY, MS, MS);
            if (map.sprite == null) // no art yet: plain blobs
            {
                map.color = new Color(0.85f, 0.8f, 0.68f);
                foreach (var kv in Cities)
                {
                    var c = UiKit.Panel(root, "City_" + kv.Key, UiKit.Orange);
                    c.sprite = Art.Dot;
                    Put(c.rectTransform, kv.Value, 90);
                    var l = UiKit.Label(root, "L", GameData.ZoneName(kv.Key), 30, UiKit.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
                    Put(l.rectTransform, kv.Value + new Vector2(0, 80), 360);
                }
            }

            string key = from + ">" + to;
            var toll = Tolls.TryGetValue(key, out var tl) ? tl : ((Cities[from] + Cities[to]) / 2, "Border");

            var marker = UiKit.Panel(root, "Marker", Color.white);
            marker.sprite = Art.Character("new_kid").sprite;
            marker.preserveAspect = true;
            marker.raycastTarget = false;

            for (float t = 0; t < 1; t += Time.unscaledDeltaTime * 4f) { group.alpha = t; Put(marker.rectTransform, Cities[from], 150); yield return null; }
            group.alpha = 1;

            // Walk to the toll
            AudioKit.Sfx("whoosh", 0.6f);
            yield return Walk(marker.rectTransform, Cities[from], toll.Item1, 1.0f);

            // The toll plaza
            var stamp = UiKit.Panel(root, "Stamp", fastag ? new Color(0.1f, 0.55f, 0.25f, 0.92f) : new Color(0.75f, 0.08f, 0.08f, 0.92f), rounded: true);
            stamp.rectTransform.Place(110, 1340, 860, 250);
            stamp.rectTransform.localRotation = Quaternion.Euler(0, 0, fastag ? 3 : -6);
            string text = fastag
                ? $"<size=40>{toll.Item2}</size>\nFASTag: BEEP! No Toll Tax."
                : $"<size=40>{toll.Item2}</size>\nTOLL TAX: ALL STATS -{RunState.TollPenalty * 100:0}%\n<size=36>for {RunState.TollDays} days</size>";
            var st = UiKit.Label(stamp.transform, "T", text, 54, Color.white, TextAnchor.MiddleCenter, FontStyle.Bold);
            st.rectTransform.Fill(14);
            UiKit.Outline(st, new Color(0, 0, 0, 0.5f), 3);
            AudioKit.Sfx(fastag ? "ping" : "stamp");
            for (float t = 0; t < 1; t += Time.unscaledDeltaTime * 6f) { stamp.rectTransform.localScale = Vector3.one * Mathf.Lerp(2.2f, 1f, t); yield return null; }
            stamp.rectTransform.localScale = Vector3.one;
            yield return new WaitForSecondsRealtime(0.6f);

            // Onwards
            yield return Walk(marker.rectTransform, toll.Item1, Cities[to], 1.0f);
            AudioKit.Sfx("card", 0.7f);

            bool tapped = false;
            var go = UiKit.Button(root, "MapContinue", $"Enter {GameData.ZoneName(to)}", UiKit.Green, 40, () => tapped = true);
            go.GetComponent<RectTransform>().Place(140, 1736, 800, 150);
            while (!tapped) yield return null;

            for (float t = 1; t > 0; t -= Time.unscaledDeltaTime * 5f) { group.alpha = t; yield return null; }
            Object.Destroy(dim.gameObject);
        }

        static IEnumerator Walk(RectTransform marker, Vector2 a, Vector2 b, float seconds)
        {
            for (float t = 0; t < seconds; t += Time.unscaledDeltaTime)
            {
                float k = Mathf.SmoothStep(0, 1, t / seconds);
                var p = Vector2.Lerp(a, b, k) + new Vector2(0, -Mathf.Abs(Mathf.Sin(t * 14f)) * 14f); // bouncy walk
                Put(marker, p, 150);
                yield return null;
            }
            Put(marker, b, 150);
        }

        /// <summary>Centres a square of the given size on a map coordinate (marker feet at the point).</summary>
        static void Put(RectTransform rt, Vector2 mapPos, float size) =>
            rt.Place(MX + mapPos.x - size / 2, MY + mapPos.y - size * 0.9f, size, size);
    }
}
