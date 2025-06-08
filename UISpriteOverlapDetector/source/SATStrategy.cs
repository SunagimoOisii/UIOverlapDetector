using UnityEngine;

public sealed class SATStrategy : IOverlapStrategy
{
    public bool Overlap(Vector2[] a, Vector2[] b)
    {
        //分離軸判定
        for (int i = 0; i < 4; ++i)
        {
            //各辺ベクトルを計算し、その方向に正規化
            Vector2 axis;
            if (i < 2)
            {
                axis = (a[(i + 1) % 4] - a[i]).normalized;
            }
            else
            {
                axis = (b[(i - 2 + 1) % 4] - b[i - 2]).normalized;
            }

            //1つでも分離軸への投影に重なりがなければ衝突していない
            if (IsOverlapOnAxis(a, b, axis) == false)
            {
                return false;
            }
        }
        return true;
    }

    private static bool IsOverlapOnAxis(Vector2[] A, Vector2[] B, Vector2 axis)
    {
        Project(A, axis, out float minA, out float maxA);
        Project(B, axis, out float minB, out float maxB);
        return maxA >= minB && maxB >= minA;
    }

    private static void Project(Vector2[] pts, Vector2 axis, out float min, out float max)
    {
        min = Vector2.Dot(pts[0], axis);
        max = Vector2.Dot(pts[0], axis);
        for (int i = 1; i < pts.Length; ++i)
        {
            float d = Vector2.Dot(pts[i], axis);
            if (d < min)      min = d;
            else if (d > max) max = d;
        }
    }
}