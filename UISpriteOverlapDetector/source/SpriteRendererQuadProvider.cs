using UnityEngine;

/// <summary>
/// SpriteRenderer専用のQuadProvider
/// </summary>
public sealed class SpriteRendererQuadProvider : IQuadProvider
{
    public int Priority => 200;

    private static readonly Vector3[] localCorners = new Vector3[4];

    public bool TryGetWorldQuad(Component c, Vector3[] worldCorners)
    {
        if (c is not SpriteRenderer sr) return false;

        var sprite = sr.sprite;
        if (sprite == null) return false;

        var bounds = sprite.bounds;
        var ext    = bounds.extents;

        localCorners[0] = new(-ext.x, -ext.y, 0);
        localCorners[1] = new( ext.x, -ext.y, 0);
        localCorners[2] = new( ext.x,  ext.y, 0);
        localCorners[3] = new(-ext.x,  ext.y, 0);

        var tf = sr.transform;
        for (int i = 0; i < 4; i++)
        {
            worldCorners[i] = tf.TransformPoint(localCorners[i]);
        }
        return true;
    }
}

