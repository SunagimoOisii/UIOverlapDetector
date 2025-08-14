using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// UI(RectTransform)と非UI(SpriteRenderer/Collider2D)のスクリーン上での重なりを検出するクラス
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
    public bool IncludeRotated
    {
        get => includeRotated;
        set
        {
            if (includeRotated == value) return;
            includeRotated = value;
            strategy = includeRotated ? new SATStrategy() : new AABBStrategy();
        }
    }

    public event Action<Component, RectTransform> OnOverlapEnter;
    public event Action<Component, RectTransform> OnOverlapStay;
    public event Action<Component, RectTransform> OnOverlapExit;

    private readonly struct PairKey : IEquatable<PairKey>
    {
        public PairKey(Component c, RectTransform r) { this.c = c; this.r = r; }
        public bool Equals(PairKey other) => c == other.c && r == other.r;
        public override int GetHashCode() => HashCode.Combine(c, r);

        public readonly Component c;
        public readonly RectTransform r;
    }
    private readonly HashSet<PairKey> previousState = new();
    private HashSet<PairKey> currentState;
    private HashSet<PairKey> entered;
    private HashSet<PairKey> stayed;
    private HashSet<PairKey> exited;
    private IOverlapStrategy strategy;

    //CalcScreenQuadで使用する一時配列
    private static readonly Vector3[] worldCorners = new Vector3[4];
    private readonly List<Vector2> quadNonUI = new(4);
    private readonly List<Vector2> quadUI    = new(4);
#if UNITY_EDITOR
    private readonly List<Vector2> gizmoQuad = new(4);
#endif

    #region 外部公開関数
    public void AddNotUI(Component comp)
    {
        if (comp == null) return;
        if (comp is RectTransform)
        {
            Debug.LogWarning("RectTransformは非UIリストに追加できません", this);
            return;
        }
        if (comp is not SpriteRenderer &&
            comp is not LineRenderer &&
            comp is not Collider2D)
        {
            Debug.LogWarning($"{comp.GetType().Name}は非UIリストに追加できません", this);
            return;
        }
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
        for (int i = notUIs.Count - 1; i >= 0; i--)
        {
            if (notUIs[i] is not SpriteRenderer && 
                notUIs[i] is not LineRenderer &&
                notUIs[i] is not Collider2D)
            {
                Debug.LogWarning($"非UIリストに対応外コンポーネントが含まれています: {notUIs[i].name}", this);
                notUIs.RemoveAt(i);
            }
        }

        strategy = includeRotated ? new SATStrategy() : new AABBStrategy();
        currentState = new HashSet<PairKey>();
        entered      = new HashSet<PairKey>();
        stayed       = new HashSet<PairKey>();
        exited       = new HashSet<PairKey>();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        strategy = includeRotated ? new SATStrategy() : new AABBStrategy();
    }
#endif

    private void LateUpdate()
    {
        var cam = (targetCamera != null) ? targetCamera : Camera.main;
        var projector = new CameraScreenProjector(cam);

        CalculateCurrentState(projector);
        DispatchEvents();
    }

    /// <summary>
    /// 監視対象の矩形化と重なり判定を行い、現在の重なり状態を計算する
    /// </summary>
    private void CalculateCurrentState(IScreenProjector projector)
    {
        currentState.Clear();

        //監視対象グループからnullを破棄
        notUIs.RemoveAll(x => x == null);
        UIs.RemoveAll(x => x == null);

        //監視グループの全組み合わせを走査
        //始めに各要素の矩形化(Vector2)を行う
        foreach (var nonUI in notUIs)
        {
            if (CalcScreenQuad(nonUI, targetCanvas, projector, quadNonUI) == false) continue;

            foreach (var ui in UIs)
            {
                if (CalcScreenQuad(ui, targetCanvas, projector, quadUI) == false) continue;

                //重なりを検知した場合
                if (strategy.Overlap(quadNonUI, quadUI))
                {
                    currentState.Add(new PairKey(nonUI, ui));
                }
            }
        }
    }

    /// <summary>
    /// Enter, Stay, Exit を計算し、対応イベントを発行する
    /// </summary>
    private void DispatchEvents()
    {
        //Enter, Stay, Exit判定
        entered.Clear();
        stayed.Clear();
        exited.Clear();
        entered.UnionWith(currentState);
        stayed.UnionWith(currentState);
        exited.UnionWith(previousState);
        entered.ExceptWith(previousState);
        stayed.IntersectWith(previousState);
        exited.ExceptWith(currentState);
        foreach (var key in entered) OnOverlapEnter?.Invoke(key.c, key.r);
        foreach (var key in stayed)  OnOverlapStay?.Invoke(key.c, key.r);
        foreach (var key in exited)  OnOverlapExit?.Invoke(key.c, key.r);

        //重なり検知状態の記録
        previousState.Clear();
        previousState.UnionWith(currentState);
    }

    private static bool TryGetWorldCorners(RectTransform rt, Vector3[] worldCorners)
    {
        if (rt == null) return false;

        rt.GetWorldCorners(worldCorners);
        return true;
    }

    private static bool CalcScreenQuad(Component obj, Canvas canvas, IScreenProjector projector, List<Vector2> screenPts)
    {
        screenPts.Clear();
        if (obj is RectTransform rt)
        {
            if (TryGetWorldCorners(rt, worldCorners) == false) return false;

            for (int i = 0; i < 4; i++)
            {
                if (canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                {
                    screenPts.Add(RectTransformUtility.WorldToScreenPoint(null, worldCorners[i]));
                }
                else
                {
                    screenPts.Add(projector.WorldToScreen(worldCorners[i]));
                }
            }
            return true;
        }

        if (QuadProviderRegistry.TryGetWorldQuad(obj, worldCorners) == false) return false;

        for (int i = 0; i < 4; i++)
        {
            screenPts.Add(projector.WorldToScreen(worldCorners[i]));
        }
        return true;
    }

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (visualizeGizmos == false) return;

        var cam = (targetCamera != null) ? targetCamera : Camera.main;
        if (cam == null) return;
        var projector = new CameraScreenProjector(cam);

        Gizmos.color = Color.yellow;
        foreach (var nonUI in notUIs)
        {
            if (nonUI == null) continue;

            if (CalcScreenQuad(nonUI, targetCanvas, projector, gizmoQuad))
            {
                DrawQuadGizmo(gizmoQuad, cam, strategy);
            }
        }

        Gizmos.color = Color.cyan;
        foreach (var ui in UIs)
        {
            if (ui == null) continue;

            if (CalcScreenQuad(ui, targetCanvas, projector, gizmoQuad))
            {
                DrawQuadGizmo(gizmoQuad, cam, strategy);
            }
        }
    }

    private static void DrawQuadGizmo(IReadOnlyList<Vector2> quad, Camera cam, IOverlapStrategy s)
    {
        if (s is AABBStrategy)
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
            for (int i = 0; i < quad.Count; i++)
            {
                Vector3 a = cam.ScreenToWorldPoint(
                    new(quad[i].x, quad[i].y, cam.nearClipPlane));
                Vector3 b = cam.ScreenToWorldPoint(
                    new(quad[(i + 1) % quad.Count].x, quad[(i + 1) % quad.Count].y, cam.nearClipPlane));
                Gizmos.DrawLine(a, b);
            }
        }
    }
#endif
}