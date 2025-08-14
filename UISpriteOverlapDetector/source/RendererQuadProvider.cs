using UnityEngine;

/// <summary>
/// Renderer基底のQuadProvider
/// </summary>
public sealed class RendererQuadProvider : IQuadProvider
{
    public int Priority => 100;

    private static readonly Vector3[] localCorners = new Vector3[4];

    public bool TryGetWorldQuad(Component c, Vector3[] worldCorners)
    {
        if (c is not Renderer r) return false;

        var bounds = r.localBounds;
        var ext    = bounds.extents;
        var center = bounds.center;

        // localBoundsの中心が原点からずれている場合に対応する
        localCorners[0] = new(center.x - ext.x, center.y - ext.y, center.z);
        localCorners[1] = new(center.x + ext.x, center.y - ext.y, center.z);
        localCorners[2] = new(center.x + ext.x, center.y + ext.y, center.z);
        localCorners[3] = new(center.x - ext.x, center.y + ext.y, center.z);

        var tf = r.transform;
        for (int i = 0; i < 4; i++)
        {
            worldCorners[i] = tf.TransformPoint(localCorners[i]);
        }
        return true;
    }
}

