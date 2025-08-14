using UnityEngine;

/// <summary>
/// LineRenderer 専用の BoundsProvider
/// </summary>
public sealed class LineRendererBoundsProvider : IBoundsProvider
{
    public int Priority => 150;

    public bool TryGetWorldBounds(Component c, Vector3[] worldCorners)
    {
        if (c is not LineRenderer lr) return false;
        if (lr.positionCount < 2) return false;

        Vector3 start = lr.GetPosition(0);
        Vector3 end   = lr.GetPosition(lr.positionCount - 1);

        if (lr.useWorldSpace == false)
        {
            var tf = lr.transform;
            start = tf.TransformPoint(start);
            end   = tf.TransformPoint(end);
        }

        float width = Mathf.Max(lr.startWidth, lr.endWidth) * lr.widthMultiplier;
        Vector3 dir = (end - start).normalized;
        if (dir == Vector3.zero) return false;
        Vector3 normal = Vector3.Cross(dir, Vector3.forward).normalized * (width * 0.5f);

        worldCorners[0] = start + normal;
        worldCorners[1] = start - normal;
        worldCorners[2] = end   - normal;
        worldCorners[3] = end   + normal;
        worldCorners[4] = worldCorners[0];
        worldCorners[5] = worldCorners[1];
        worldCorners[6] = worldCorners[2];
        worldCorners[7] = worldCorners[3];
        return true;
    }
}
