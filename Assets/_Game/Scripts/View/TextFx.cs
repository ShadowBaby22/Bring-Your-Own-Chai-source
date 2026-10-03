using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace KadakChai
{
    /// <summary>
    /// Per-character text effects for legacy UI Text: typewriter reveal, wave and shake.
    /// Works on the glyph quads (6 verts each), so rich-text colours survive.
    /// Add it BEFORE any Outline/Shadow component so those copy the animated glyphs.
    /// </summary>
    [RequireComponent(typeof(Text))]
    public class TextFx : BaseMeshEffect
    {
        public float wave;        // px amplitude of a sine wave across the line
        public float shake;       // px of random jitter per glyph
        public float revealSpeed; // glyphs per second while revealing

        public event Action Completed;
        public Action<int> OnGlyph; // fired as each new glyph appears (voice blips)

        float visible = float.MaxValue;
        int glyphs = -1;
        float time;
        static readonly List<UIVertex> verts = new();

        public bool Revealing => visible < float.MaxValue;

        public void StartReveal(float glyphsPerSecond)
        {
            revealSpeed = glyphsPerSecond;
            visible = 0;
            glyphs = -1;
            graphic.SetVerticesDirty();
        }

        public void Complete()
        {
            if (!Revealing) return;
            visible = float.MaxValue;
            graphic.SetVerticesDirty();
            Completed?.Invoke();
        }

        void Update()
        {
            time += Time.unscaledDeltaTime;
            bool dirty = wave > 0 || shake > 0;
            if (Revealing && glyphs >= 0)
            {
                int before = (int)visible;
                visible += revealSpeed * Time.unscaledDeltaTime;
                if ((int)visible != before) OnGlyph?.Invoke((int)visible);
                if (visible >= glyphs) Complete();
                dirty = true;
            }
            if (dirty) graphic.SetVerticesDirty();
        }

        public override void ModifyMesh(VertexHelper vh)
        {
            if (!IsActive()) return;
            verts.Clear();
            vh.GetUIVertexStream(verts);
            glyphs = verts.Count / 6;

            for (int g = 0; g < glyphs; g++)
            {
                bool hidden = g >= visible;
                Vector3 off = Vector3.zero;
                if (wave > 0) off.y += Mathf.Sin(time * 8f + g * 0.55f) * wave;
                if (shake > 0) off += (Vector3)(UnityEngine.Random.insideUnitCircle * shake);
                for (int k = 0; k < 6; k++)
                {
                    var v = verts[g * 6 + k];
                    v.position += off;
                    if (hidden) { var c = v.color; c.a = 0; v.color = c; }
                    verts[g * 6 + k] = v;
                }
            }
            vh.Clear();
            vh.AddUIVertexTriangleStream(verts);
        }
    }
}
