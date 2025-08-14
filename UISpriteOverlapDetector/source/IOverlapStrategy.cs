using System.Collections.Generic;
using UnityEngine;

public interface IOverlapStrategy
{
    // OBB や Rect 型ではなく、任意数の頂点リストを受け取る
    bool Overlap(IReadOnlyList<Vector3> a, IReadOnlyList<Vector3> b);
}
