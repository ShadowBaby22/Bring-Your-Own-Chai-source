using System.Collections.Generic;
using UnityEngine;

namespace KadakChai
{
    /// <summary>
    /// Art lookup with generated placeholders. To swap in real art, drop PNGs into Resources/Art:
    ///   Characters/&lt;combatant id&gt;.png   (new_kid, pinky_aunty, broker, ceo, babu, ...)
    ///   Backgrounds/&lt;zone&gt;.png          (dilli, noida, daftar, gurugram)
    /// No code or inspector changes needed. Missing files fall back to coloured blobs.
    /// </summary>
    public static class Art
    {
        static readonly Dictionary<string, Sprite> cache = new();

        // This project runs Play mode without a domain reload, so statics survive between sessions.
        // Start every session with empty caches so no stale runtime-made sprite is reused.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetStatics() { cache.Clear(); rounded = blob = dot = null; }

        public static (Sprite sprite, bool placeholder) Character(string id)
        {
            var real = Load("Art/Characters/" + id);
            return real != null ? (real, false) : (Blob, true);
        }

        /// <summary>UI/effect art from Resources/Art/UI (burst, dmg1, dmg2). Null if missing.</summary>
        public static Sprite UI(string name) => Load("Art/UI/" + name);

        public static bool HasCharacter(string id) => !string.IsNullOrEmpty(id) && Load("Art/Characters/" + id) != null;

        /// <summary>Optional pose variant, e.g. broker_hit.png or broker_attack.png. Null if not drawn.</summary>
        public static Sprite Pose(string id, string pose) => Load($"Art/Characters/{id}_{pose}");

        public static (Sprite sprite, bool placeholder) Background(string zone)
        {
            var real = Load("Art/Backgrounds/" + zone);
            return real != null ? (real, false) : (Generated("bg_" + zone, () => MakeBackground(zone)), true);
        }

        static Sprite Load(string path)
        {
            if (cache.TryGetValue(path, out var s)) return s;
            // Unity 6's 2D importer can auto-slice a PNG into several sprites ("Multiple" mode) when it has
            // separate pieces (a shadow, floating "?" marks, bruise overlays). Loading by name would then return
            // just one slice, so in that case build a sprite from the whole texture instead.
            var all = Resources.LoadAll<Sprite>(path);
            if (all.Length == 1) s = all[0];
            else { var tex = Resources.Load<Texture2D>(path); if (tex != null) s = FromTexture(tex); }
            cache[path] = s;
            return s;
        }

        static Sprite Generated(string key, System.Func<Texture2D> make)
        {
            if (cache.TryGetValue(key, out var s) && s != null) return s;
            s = FromTexture(make());
            cache[key] = s;
            return s;
        }

        static Sprite FromTexture(Texture2D t, Vector4 border = default) =>
            Sprite.Create(t, new Rect(0, 0, t.width, t.height), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, border);

        // --- Generated sprites -----------------------------------------------------

        static Sprite rounded;
        /// <summary>White rounded rectangle, 9-sliced, for panels and buttons.</summary>
        public static Sprite Rounded
        {
            get
            {
                if (rounded != null) return rounded;
                const int n = 64, r = 22;
                var t = NewTex(n, n);
                for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float dx = Mathf.Max(0, Mathf.Max(r - x, x - (n - 1 - r)));
                    float dy = Mathf.Max(0, Mathf.Max(r - y, y - (n - 1 - r)));
                    float a = Mathf.Clamp01(r + 0.5f - Mathf.Sqrt(dx * dx + dy * dy));
                    t.SetPixel(x, y, new Color(1, 1, 1, a));
                }
                t.Apply();
                rounded = FromTexture(t, new Vector4(r, r, r, r));
                return rounded;
            }
        }

        static Sprite dot;
        /// <summary>Soft white circle for particles.</summary>
        public static Sprite Dot
        {
            get
            {
                if (dot != null) return dot;
                const int n = 32;
                var t = NewTex(n, n);
                for (int y = 0; y < n; y++)
                for (int x = 0; x < n; x++)
                {
                    float d = Vector2.Distance(new Vector2(x, y), new Vector2(n / 2f - 0.5f, n / 2f - 0.5f));
                    t.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(n / 2f - d)));
                }
                t.Apply();
                dot = FromTexture(t);
                return dot;
            }
        }

        static Sprite blob;
        /// <summary>Placeholder character: white sticker blob with a dark outline and eyes. Tint with Image.color.</summary>
        public static Sprite Blob
        {
            get
            {
                if (blob != null) return blob;
                const int w = 256, h = 320;
                var t = NewTex(w, h);
                var cx = w / 2f; var cy = h * 0.46f; var rx = w * 0.44f; var ry = h * 0.43f;
                for (int y = 0; y < h; y++)
                for (int x = 0; x < w; x++)
                {
                    // distance-ish to ellipse edge in pixels
                    float nx = (x - cx) / rx, ny = (y - cy) / ry;
                    float d = (Mathf.Sqrt(nx * nx + ny * ny) - 1f) * Mathf.Min(rx, ry);
                    Color c = Color.clear;
                    if (d < -9) c = Color.white;
                    else if (d < 0) c = new Color(0.12f, 0.12f, 0.14f, 1);
                    // eyes
                    foreach (var ex in new[] { cx - 40, cx + 40 })
                    {
                        float ed = Vector2.Distance(new Vector2(x, y), new Vector2(ex, cy + 45));
                        if (ed < 22) c = new Color(0.12f, 0.12f, 0.14f, 1);
                        if (Vector2.Distance(new Vector2(x, y), new Vector2(ex + 6, cy + 53)) < 7) c = Color.white;
                    }
                    t.SetPixel(x, y, c);
                }
                t.Apply();
                blob = Sprite.Create(t, new Rect(0, 0, w, h), new Vector2(0.5f, 0f), 100);
                return blob;
            }
        }

        static Texture2D MakeBackground(string zone)
        {
            // Sky, smoggy horizon band, ground. Enough to tell zones apart until real art lands.
            (string sky, string haze, string ground) p = zone switch
            {
                "dilli" => ("#C9A27E", "#B08968", "#8D7B68"),
                "noida" => ("#9FB4C7", "#8A9BA8", "#6D7B6E"),
                "gurugram" => ("#8EC5D6", "#B0BEC5", "#607D8B"),
                "daftar" => ("#D7CCC8", "#BCAAA4", "#8D6E63"),
                _ => ("#B0BEC5", "#90A4AE", "#607D8B"),
            };
            Color sky = UiKit.Hex(p.sky), haze = UiKit.Hex(p.haze), ground = UiKit.Hex(p.ground);
            const int w = 130, h = 82; // tiny, scaled up with bilinear filtering
            var t = NewTex(w, h);
            for (int y = 0; y < h; y++)
            for (int x = 0; x < w; x++)
            {
                float v = y / (float)h; // 0 bottom, 1 top
                Color c = v < 0.32f ? ground : Color.Lerp(haze, sky, Mathf.InverseLerp(0.32f, 1f, v));
                // a few "buildings" on the horizon
                float bh = 0.32f + 0.12f * Mathf.Abs(Mathf.Sin(x * 0.37f) + Mathf.Sin(x * 0.11f));
                if (v >= 0.32f && v < bh && (x / 9) % 2 == 0) c = Color.Lerp(haze, ground, 0.35f);
                t.SetPixel(x, y, c);
            }
            t.Apply();
            t.filterMode = FilterMode.Bilinear;
            return t;
        }

        static Texture2D NewTex(int w, int h) =>
            new(w, h, TextureFormat.RGBA32, false) { wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
    }
}
