using UnityEngine;
using UnityEngine.UI;

namespace KadakChai
{
    /// <summary>
    /// Keeps a label horizontally inside the nearest clipping parent (the screen), so names over fighters near an
    /// edge ("Kush (Rival Uni)", "South Delhi Girl") are nudged in instead of being cut off.
    /// </summary>
    public class ClampInside : MonoBehaviour
    {
        static readonly Vector3[] mine = new Vector3[4], outer = new Vector3[4];
        RectTransform rt, bounds;
        float baseX;
        bool ready;

        void LateUpdate()
        {
            if (!ready)
            {
                rt = (RectTransform)transform;
                var mask = transform.parent != null ? transform.parent.GetComponentInParent<RectMask2D>() : null;
                bounds = mask != null ? (RectTransform)mask.transform : null;
                baseX = rt.anchoredPosition.x;
                ready = true;
            }
            if (bounds == null) return;
            var p = rt.anchoredPosition;
            p.x = baseX;
            rt.anchoredPosition = p;
            rt.GetWorldCorners(mine);
            bounds.GetWorldCorners(outer);
            float pad = (outer[2].x - outer[0].x) * 0.01f, shift = 0;
            float left = Mathf.Min(mine[0].x, mine[2].x), right = Mathf.Max(mine[0].x, mine[2].x);
            if (left < outer[0].x + pad) shift = outer[0].x + pad - left;
            else if (right > outer[2].x - pad) shift = outer[2].x - pad - right;
            if (shift == 0) return;
            float scale = rt.lossyScale.x;
            if (Mathf.Abs(scale) < 1e-5f) return;
            p.x = baseX + shift / scale;
            rt.anchoredPosition = p;
        }
    }
}
