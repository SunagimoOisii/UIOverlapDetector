using UnityEngine;

/// <summary>
/// 非UIコンポーネントからワールド座標上の境界頂点を生成するインターフェース
/// </summary>
public interface IBoundsProvider
{
    /// <summary>優先度。大きいほど優先される</summary>
    int Priority { get; }

    /// <summary>
    /// ワールド座標上の境界8頂点の取得を試みる
    /// </summary>
    /// <param name="c">対象コンポーネント</param>
    /// <param name="worldCorners">結果格納先</param>
    /// <returns>成功した場合 true</returns>
    bool TryGetWorldBounds(Component c, Vector3[] worldCorners);
}

