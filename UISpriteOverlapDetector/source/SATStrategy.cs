using System.Collections.Generic;
using UnityEngine;

public sealed class SATStrategy : IOverlapStrategy
{
    public bool Overlap(IReadOnlyList<Vector2> a, IReadOnlyList<Vector2> b)
    {
        // 分離軸定理による重なり判定
        for (int i = 0; i < a.Count; ++i)
        {
            Vector2 edge = a[(i + 1) % a.Count] - a[i];
            Vector2 axis = new Vector2(-edge.y, edge.x).normalized;
            if (IsOverlapOnAxis(a, b, axis) == false)
            {
                return false;
            }
        }
        for (int i = 0; i < b.Count; ++i)
        {
            Vector2 edge = b[(i + 1) % b.Count] - b[i];
            Vector2 axis = new Vector2(-edge.y, edge.x).normalized;
            if (IsOverlapOnAxis(a, b, axis) == false)
            {
                return false;
            }
        }
        return true;
    }

    private static bool IsOverlapOnAxis(IReadOnlyList<Vector2> A, IReadOnlyList<Vector2> B, Vector2 axis)
    {
        Project(A, axis, out float minA, out float maxA);
        Project(B, axis, out float minB, out float maxB);
        return maxA >= minB && maxB >= minA;
    }

    private static void Project(IReadOnlyList<Vector2> pts, Vector2 axis, out float min, out float max)
    {
        min = Vector2.Dot(pts[0], axis);
        max = Vector2.Dot(pts[0], axis);
        for (int i = 1; i < pts.Count; ++i)
        {
            float d = Vector2.Dot(pts[i], axis);
            if (d < min)      min = d;
            else if (d > max) max = d;
        }
    }
}
