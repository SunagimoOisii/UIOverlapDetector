using UnityEngine;

/// <summary>
/// SpriteRenderer 専用の BoundsProvider
/// </summary>
public sealed class SpriteRendererBoundsProvider : IBoundsProvider
{
    public int Priority => 200;

    private static readonly Vector3[] localCorners = new Vector3[8];

    public bool TryGetWorldBounds(Component c, Vector3[] worldCorners)
    {
        if (c is not SpriteRenderer sr) return false;

        var sprite = sr.sprite;
        if (sprite == null) return false;

        var bounds = sprite.bounds;
        var ext    = bounds.extents;

        localCorners[0] = new(-ext.x, -ext.y, -ext.z);
        localCorners[1] = new( ext.x, -ext.y, -ext.z);
        localCorners[2] = new( ext.x,  ext.y, -ext.z);
        localCorners[3] = new(-ext.x,  ext.y, -ext.z);
        localCorners[4] = new(-ext.x, -ext.y,  ext.z);
        localCorners[5] = new( ext.x, -ext.y,  ext.z);
        localCorners[6] = new( ext.x,  ext.y,  ext.z);
        localCorners[7] = new(-ext.x,  ext.y,  ext.z);

        var tf = sr.transform;
        for (int i = 0; i < 8; i++)
        {
            worldCorners[i] = tf.TransformPoint(localCorners[i]);
        }
        return true;
    }
}

