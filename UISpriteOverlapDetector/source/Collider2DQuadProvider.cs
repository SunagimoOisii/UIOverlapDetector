using UnityEngine;

/// <summary>
/// Collider2D基底のQuadProvider
/// </summary>
public sealed class Collider2DQuadProvider : IQuadProvider
{
    public int Priority => 90;

    public bool TryGetWorldQuad(Component c, Vector3[] worldCorners)
    {
        if (c is not Collider2D col) return false;

        var b     = col.bounds;
        var ext   = b.extents;
        var center = b.center;

        worldCorners[0] = new(center.x - ext.x, center.y - ext.y, center.z);
        worldCorners[1] = new(center.x + ext.x, center.y - ext.y, center.z);
        worldCorners[2] = new(center.x + ext.x, center.y + ext.y, center.z);
        worldCorners[3] = new(center.x - ext.x, center.y + ext.y, center.z);

        return true;
    }
}

