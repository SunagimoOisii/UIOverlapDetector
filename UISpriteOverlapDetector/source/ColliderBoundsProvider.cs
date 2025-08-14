using UnityEngine;

/// <summary>
/// Collider および Collider2D 共通の BoundsProvider
/// </summary>
public sealed class ColliderBoundsProvider : IBoundsProvider
{
    public int Priority => 80;

    public bool TryGetWorldBounds(Component c, Vector3[] worldCorners)
    {
        Bounds b;
        if (c is Collider col3D)
        {
            b = col3D.bounds;
        }
        else if (c is Collider2D col2D)
        {
            b = col2D.bounds;
        }
        else
        {
            return false;
        }

        var ext    = b.extents;
        var center = b.center;

        worldCorners[0] = new(center.x - ext.x, center.y - ext.y, center.z - ext.z);
        worldCorners[1] = new(center.x + ext.x, center.y - ext.y, center.z - ext.z);
        worldCorners[2] = new(center.x + ext.x, center.y + ext.y, center.z - ext.z);
        worldCorners[3] = new(center.x - ext.x, center.y + ext.y, center.z - ext.z);
        worldCorners[4] = new(center.x - ext.x, center.y - ext.y, center.z + ext.z);
        worldCorners[5] = new(center.x + ext.x, center.y - ext.y, center.z + ext.z);
        worldCorners[6] = new(center.x + ext.x, center.y + ext.y, center.z + ext.z);
        worldCorners[7] = new(center.x - ext.x, center.y + ext.y, center.z + ext.z);

        return true;
    }
}

