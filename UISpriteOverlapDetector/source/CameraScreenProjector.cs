using UnityEngine;

/// <summary>
/// Camera を用いてワールド座標をスクリーン座標へ変換する実装
/// 透視・直交カメラのいずれにも対応する
/// </summary>
public sealed class CameraScreenProjector : IScreenProjector
{
    private readonly Camera cam;

    public CameraScreenProjector(Camera cam)
    {
        this.cam = cam;
    }

    /// <inheritdoc/>
    public Vector3 WorldToScreen(Vector3 worldPos)
    {
        return cam.WorldToScreenPoint(worldPos);
    }
}
