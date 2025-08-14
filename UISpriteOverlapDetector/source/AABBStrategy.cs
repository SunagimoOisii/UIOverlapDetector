using UnityEngine;

public sealed class AABBStrategy : IOverlapStrategy
{
    public bool Overlap(Vector2[] a, Vector2[] b)
    {
        Rect ra = CalcBoundingRect(a);
        Rect rb = CalcBoundingRect(b);
        return ra.Overlaps(rb);
    }

    private static Rect CalcBoundingRect(Vector2[] points)
    {
        float minX = points[0].x, minY = points[0].y;
        float maxX = points[0].x, maxY = points[0].y;
        foreach (var pt in points)
        {
            if (pt.x < minX) minX = pt.x;
            if (pt.x > maxX) maxX = pt.x;
            if (pt.y < minY) minY = pt.y;
            if (pt.y > maxY) maxY = pt.y;
        }
        return Rect.MinMaxRect(minX, minY, maxX, maxY);
    }
}
