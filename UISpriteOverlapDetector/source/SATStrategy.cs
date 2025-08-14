using System.Collections.Generic;
using UnityEngine;

public sealed class SATStrategy : IOverlapStrategy
{
    public bool Overlap(IReadOnlyList<Vector3> a, IReadOnlyList<Vector3> b)
    {
        // 分離軸定理による重なり判定
        for (int i = 0; i < a.Count; ++i)
        {
            Vector2 ai = new(a[i].x, a[i].y);
            Vector2 anext = new(a[(i + 1) % a.Count].x, a[(i + 1) % a.Count].y);
            Vector2 edge = anext - ai;
            Vector2 axis = new(-edge.y, edge.x).normalized;
            if (IsOverlapOnAxis(a, b, axis) == false)
            {
                return false;
            }
        }
        for (int i = 0; i < b.Count; ++i)
        {
            Vector2 bi = new(b[i].x, b[i].y);
            Vector2 bnext = new(b[(i + 1) % b.Count].x, b[(i + 1) % b.Count].y);
            Vector2 edge = bnext - bi;
            Vector2 axis = new(-edge.y, edge.x).normalized;
            if (IsOverlapOnAxis(a, b, axis) == false)
            {
                return false;
            }
        }
        return true;
    }

    private static bool IsOverlapOnAxis(IReadOnlyList<Vector3> A, IReadOnlyList<Vector3> B, Vector2 axis)
    {
        Project(A, axis, out float minA, out float maxA);
        Project(B, axis, out float minB, out float maxB);
        return maxA >= minB && maxB >= minA;
    }

    private static void Project(IReadOnlyList<Vector3> pts, Vector2 axis, out float min, out float max)
    {
        Vector2 p0 = new(pts[0].x, pts[0].y);
        min = Vector2.Dot(p0, axis);
        max = min;
        for (int i = 1; i < pts.Count; ++i)
        {
            Vector2 p = new(pts[i].x, pts[i].y);
            float d = Vector2.Dot(p, axis);
            if (d < min)      min = d;
            else if (d > max) max = d;
        }
    }
}
