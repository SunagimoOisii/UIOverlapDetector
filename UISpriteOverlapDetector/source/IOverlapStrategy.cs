using UnityEngine;

public interface IOverlapStrategy
{
    // OBB や Rect 型ではなく、四隅の Vector2 配列を受け取る
    bool Overlap(Vector2[] a, Vector2[] b);
}
