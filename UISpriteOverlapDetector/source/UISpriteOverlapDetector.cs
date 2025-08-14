using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI(RectTransform)とSpriteRendererのスクリーン上での重なりを検出するクラス
/// </summary>
[DisallowMultipleComponent]
public sealed class UISpriteOverlapDetector : MonoBehaviour
{
    [Header("監視対象環境")]
    [SerializeField] private Canvas targetCanvas;
    [SerializeField] private Camera targetCamera;

    [Header("監視対象リスト")]
    [SerializeField] private List<Component> notUIs  = new();
    [SerializeField] private List<RectTransform> UIs = new();

    [Header("オプション")]
    [SerializeField] private bool visualizeGizmos = true;
    [SerializeField] private bool includeRotated  = false;

    public event Action<Component, RectTransform> OnOverlapEnter;
    public event Action<Component, RectTransform> OnOverlapStay;
    public event Action<Component, RectTransform> OnOverlapExit;

    private readonly struct PairKey : IEquatable<PairKey>
    {
        public readonly Component c;
        public readonly RectTransform r;

        public PairKey(Component c, RectTransform r) { this.c = c; this.r = r; }

        public bool Equals(PairKey other) => c == other.c && r == other.r;
        public override int GetHashCode() => HashCode.Combine(c, r);
    }
    private readonly HashSet<PairKey> previousState = new();
    private HashSet<PairKey> currentState;
    private HashSet<PairKey> entered;
    private HashSet<PairKey> stayed;
    private HashSet<PairKey> exited;
    private IOverlapStrategy strategy;

    // CalcScreenQuadで使用する一時配列
    private static readonly Vector3[] worldCorners = new Vector3[4];
    private static readonly Vector3[] localCorners = new Vector3[4];
    private readonly Vector2[] quadNonUI = new Vector2[4];
    private readonly Vector2[] quadUI    = new Vector2[4];
#if UNITY_EDITOR
    private readonly Vector2[] gizmoQuad = new Vector2[4];
#endif

    #region 外部公開関数
    public void AddNotUI(Component comp)
    {
        if (comp == null) return;
        if (comp is not SpriteRenderer) return;
        if (notUIs.Contains(comp)) return;

        notUIs.Add(comp);
    }
    public void RemoveNotUI(Component comp)
    {
        notUIs.Remove(comp);
    }

    public void AddUI(RectTransform rt)
    {
        if (rt == null) return;
        if (UIs.Contains(rt)) return;

        UIs.Add(rt);
    }
    public void RemoveUI(RectTransform rt)
    {
        UIs.Remove(rt);
    }
    #endregion

    private void Awake()
    {
        if (targetCanvas == null)
        {
            targetCanvas = GetComponentInParent<Canvas>();
        }

        strategy     = includeRotated ? new SATStrategy() : new AABBStrategy();
        currentState = new HashSet<PairKey>();
        entered      = new HashSet<PairKey>();
        stayed       = new HashSet<PairKey>();
        exited       = new HashSet<PairKey>();
    }

    private void LateUpdate()
    {
        var cam = (targetCamera != null) ? targetCamera : Camera.main;
        currentState.Clear();

        //監視対象グループからnullを破棄
        notUIs.RemoveAll(x => x == null);
        UIs.RemoveAll(x => x == null);

        //監視グループの全組み合わせを走査
        //始めに各要素の矩形化(Vector2)を行う
        foreach (var nonUI in notUIs)
        {
            if (CalcScreenQuad(nonUI, targetCanvas, cam, quadNonUI) == false) continue;

            foreach (var ui in UIs)
            {
                if (CalcScreenQuad(ui, targetCanvas, cam, quadUI) == false) continue;

                //重なりを検知した場合
                if (strategy.Overlap(quadNonUI, quadUI))
                {
                    currentState.Add(new PairKey(nonUI, ui));
                }
            }
        }

        //Enter, Stay, Exit判定
        entered.Clear();
        entered.UnionWith(currentState);
        stayed.Clear();
        stayed.UnionWith(currentState);
        exited.Clear();
        exited.UnionWith(previousState);
        entered.ExceptWith(previousState);
        stayed.IntersectWith(previousState);
        exited.ExceptWith(currentState);
        foreach (var key in entered) OnOverlapEnter?.Invoke(key.c, key.r);
        foreach (var key in stayed) OnOverlapStay?.Invoke(key.c, key.r);
        foreach (var key in exited) OnOverlapExit?.Invoke(key.c, key.r);

        //重なり検知状態の記録
        previousState.Clear();
        previousState.UnionWith(currentState);
    }

    private static bool CalcScreenQuad(Component obj, Canvas canvas, Camera cam, Vector2[] screenPts)
    {
        //ワールド空間上の四つ角の取得
        if (obj is RectTransform rt)
        {
            rt.GetWorldCorners(worldCorners);
        }
        else if (obj is SpriteRenderer sr)
        {
            var tf = sr.transform;
            var sprite = sr.sprite;
            if (sprite == null) return false;

            var bounds = sprite.bounds;
            var ext    = bounds.extents;

            //ローカル空間のOBB四隅
            localCorners[0] = new(-ext.x, -ext.y, 0);
            localCorners[1] = new( ext.x, -ext.y, 0);
            localCorners[2] = new( ext.x,  ext.y, 0);
            localCorners[3] = new(-ext.x,  ext.y, 0);

            for (int i = 0; i < 4; i++)
            {
                worldCorners[i] = tf.TransformPoint(localCorners[i]);
            }
        }
        else
        {
            return false;
        }

        //四つ角をスクリーン上の座標に変換
        for (int i = 0; i < 4; i++)
        {
            if (obj is RectTransform)
            {
                //CanvasModeで分岐
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    screenPts[i] = RectTransformUtility.WorldToScreenPoint(null, worldCorners[i]);
                }
                else
                {
                    screenPts[i] = cam.WorldToScreenPoint(worldCorners[i]);
                }
            }
            else
            {
                //非UIは常にCameraベースで変換
                screenPts[i] = cam.WorldToScreenPoint(worldCorners[i]);
            }
        }
        return true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (visualizeGizmos == false) return;

        var cam = (targetCamera != null) ? targetCamera : Camera.main;
        if (cam == null) return;

        Gizmos.color = Color.yellow;
        foreach (var nonUI in notUIs)
        {
            if (nonUI == null) continue;

            if (CalcScreenQuad(nonUI, targetCanvas, cam, gizmoQuad))
            {
                DrawQuadGizmo(gizmoQuad, cam, strategy);
            }
        }

        Gizmos.color = Color.cyan;
        foreach (var ui in UIs)
        {
            if (ui == null) continue;

            if (CalcScreenQuad(ui, targetCanvas, cam, gizmoQuad))
            {
                DrawQuadGizmo(gizmoQuad, cam, strategy);
            }
        }
    }

    private static void DrawQuadGizmo(
    Vector2[] quad, Camera cam, IOverlapStrategy strategy)
    {
        if (strategy is AABBStrategy)
        {
            //AABBを求めて軸整列の矩形を描く
            float minX = quad[0].x, minY = quad[0].y,
                  maxX = quad[0].x, maxY = quad[0].y;
            foreach (var p in quad)
            {
                if (p.x < minX) minX = p.x; if (p.x > maxX) maxX = p.x;
                if (p.y < minY) minY = p.y; if (p.y > maxY) maxY = p.y;
            }

            Vector3 tl = cam.ScreenToWorldPoint(new(minX, maxY, cam.nearClipPlane));
            Vector3 tr = cam.ScreenToWorldPoint(new(maxX, maxY, cam.nearClipPlane));
            Vector3 br = cam.ScreenToWorldPoint(new(maxX, minY, cam.nearClipPlane));
            Vector3 bl = cam.ScreenToWorldPoint(new(minX, minY, cam.nearClipPlane));

            Gizmos.DrawLine(tl, tr); Gizmos.DrawLine(tr, br);
            Gizmos.DrawLine(br, bl); Gizmos.DrawLine(bl, tl);
        }
        else
        {
            for (int i = 0; i < 4; i++)
            {
                Vector3 a = cam.ScreenToWorldPoint(
                    new(quad[i].x, quad[i].y, cam.nearClipPlane));
                Vector3 b = cam.ScreenToWorldPoint(
                    new(quad[(i + 1) % 4].x, quad[(i + 1) % 4].y, cam.nearClipPlane));
                Gizmos.DrawLine(a, b);
            }
        }
    }
#endif
}