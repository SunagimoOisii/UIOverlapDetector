using UnityEngine;

/// <summary>
/// ワールド座標をスクリーン座標へ投影するためのインターフェース
/// </summary>
public interface IScreenProjector
{
    /// <summary>
    /// ワールド座標をスクリーン座標へ変換する
    /// </summary>
    Vector3 WorldToScreen(Vector3 worldPos);
}
