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

        localCorners[0] = new(-ext.x, -ext.y, 0);
        localCorners[1] = new( ext.x, -ext.y, 0);
        localCorners[2] = new( ext.x,  ext.y, 0);
        localCorners[3] = new(-ext.x,  ext.y, 0);

        var tf = r.transform;
        for (int i = 0; i < 4; i++)
        {
            worldCorners[i] = tf.TransformPoint(localCorners[i]);
        }
        return true;
    }
}

