using UnityEngine;

/// <summary>
/// MeshRenderer 専用の BoundsProvider
/// </summary>
public sealed class MeshRendererBoundsProvider : IBoundsProvider
{
    public int Priority => 100;

    private static readonly Vector3[] localCorners = new Vector3[8];

    public bool TryGetWorldBounds(Component c, Vector3[] worldCorners)
    {
        if (c is not MeshRenderer r) return false;

        var bounds = r.localBounds;
        var ext    = bounds.extents;
        var center = bounds.center;

        localCorners[0] = new(center.x - ext.x, center.y - ext.y, center.z - ext.z);
        localCorners[1] = new(center.x + ext.x, center.y - ext.y, center.z - ext.z);
        localCorners[2] = new(center.x + ext.x, center.y + ext.y, center.z - ext.z);
        localCorners[3] = new(center.x - ext.x, center.y + ext.y, center.z - ext.z);
        localCorners[4] = new(center.x - ext.x, center.y - ext.y, center.z + ext.z);
        localCorners[5] = new(center.x + ext.x, center.y - ext.y, center.z + ext.z);
        localCorners[6] = new(center.x + ext.x, center.y + ext.y, center.z + ext.z);
        localCorners[7] = new(center.x - ext.x, center.y + ext.y, center.z + ext.z);

        var tf = r.transform;
        for (int i = 0; i < 8; i++)
        {
            worldCorners[i] = tf.TransformPoint(localCorners[i]);
        }
        return true;
    }
}

