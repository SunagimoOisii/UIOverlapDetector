using UnityEngine;

public interface IOverlapStrategy
{
    //OBB�Ή��̂���Rect�ł͂Ȃ�Vector2���g�p(Rect�͉�]���Ȃ�����)
    bool Overlap(Vector2[] a, Vector2[] b);
}