using UnityEngine;

/// <summary>
/// 非UIコンポーネントからワールド座標上の四隅を生成するインターフェース
/// </summary>
public interface IQuadProvider
{
    /// <summary>優先度。大きいほど優先される</summary>
    int Priority { get; }

    /// <summary>
    /// ワールド四隅の取得を試みる
    /// </summary>
    /// <param name="c">対象コンポーネント</param>
    /// <param name="worldCorners">結果格納先</param>
    /// <returns>成功した場合 true</returns>
    bool TryGetWorldQuad(Component c, Vector3[] worldCorners);
}

