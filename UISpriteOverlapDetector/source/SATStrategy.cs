using UnityEngine;

public sealed class SATStrategy : IOverlapStrategy
{
    public bool Overlap(Vector2[] a, Vector2[] b)
    {
        //����������
        for (int i = 0; i < 4; ++i)
        {
            //�e�Ӄx�N�g�����v�Z���A���̕����ɐ��K��
            Vector2 axis;
            if (i < 2)
            {
                axis = (a[(i + 1) % 4] - a[i]).normalized;
            }
            else
            {
                axis = (b[(i - 2 + 1) % 4] - b[i - 2]).normalized;
            }

            //1�ł��������ւ̓��e�ɏd�Ȃ肪�Ȃ���ΏՓ˂��Ă��Ȃ�
            if (IsOverlapOnAxis(a, b, axis) == false)
            {
                return false;
            }
        }
        return true;
    }

    private static bool IsOverlapOnAxis(Vector2[] A, Vector2[] B, Vector2 axis)
    {
        Project(A, axis, out float minA, out float maxA);
        Project(B, axis, out float minB, out float maxB);
        return maxA >= minB && maxB >= minA;
    }

    private static void Project(Vector2[] pts, Vector2 axis, out float min, out float max)
    {
        min = Vector2.Dot(pts[0], axis);
        max = Vector2.Dot(pts[0], axis);
        for (int i = 1; i < pts.Length; ++i)
        {
            float d = Vector2.Dot(pts[i], axis);
            if (d < min)      min = d;
            else if (d > max) max = d;
        }
    }
}