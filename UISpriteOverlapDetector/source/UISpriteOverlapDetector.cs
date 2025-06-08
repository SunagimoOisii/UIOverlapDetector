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
    private readonly List<PairKey> previousState = new();
    private IOverlapStrategy strategy;

    #region 外部公開関数
    public void AddNotUI(Component comp)
    {
        if (comp is SpriteRenderer && 
            notUIs.Contains(comp) == false)
        {
            notUIs.Add(comp);
        }
    }
    public void RemoveNotUI(Component comp)
    {
        notUIs.Remove(comp);
    }

    public void AddUI(RectTransform rt)
    {
        if (rt &&
            UIs.Contains(rt) == false)
        {
            UIs.Add(rt);
        }
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

        strategy = includeRotated ? new SATStrategy() : new AABBStrategy();
    }

    private void LateUpdate()
    {
        var cam = (targetCamera != null) ? targetCamera : Camera.main;
        var currentState = new List<PairKey>();

        //監視対象グループからnullを破棄
        notUIs.RemoveAll(x => x == null);
        UIs.RemoveAll(x => x == null);

        //監視グループの全組み合わせを走査
        //始めに各要素の矩形化(Vector2)を行う
        foreach (var nonUI in notUIs)
        {
            Vector2[] quad_nonUI = CalcScreenQuad(nonUI, targetCanvas, cam);
            if (quad_nonUI == null) continue;

            foreach (var ui in UIs)
            {
                Vector2[] quad_UI = CalcScreenQuad(ui, targetCanvas, cam);
                if (quad_UI == null) continue;

                //重なりを検知した場合
                if (strategy.Overlap(quad_nonUI, quad_UI))
                {
                    var key = new PairKey(nonUI, ui);
                    currentState.Add(key);

                    if (previousState.Contains(key) == false)
                    {
                        OnOverlapEnter?.Invoke(nonUI, ui);
                    }
                    else
                    {
                        OnOverlapStay?.Invoke(nonUI, ui);
                    }
                }
            }
        }

        //Exitイベント発行判定
        foreach (var key in previousState)
        {
            if (currentState.Contains(key) == false)
            {
                OnOverlapExit?.Invoke(key.c, key.r);
            }
        }

        //重なり検知状態の記録
        previousState.Clear();
        foreach (var k in currentState)
        {
            previousState.Add(k);
        }
    }

    private static Vector2[] CalcScreenQuad(Component obj, Canvas canvas, Camera cam)
    {
        Vector3[] worldCorners = new Vector3[4];

        //ワールド空間上の四つ角の取得
        if (obj is RectTransform rt)
        {
            rt.GetWorldCorners(worldCorners);
        }
        else if (obj is SpriteRenderer sr)
        {
            var tf = sr.transform;
            var sprite = sr.sprite;
            if (sprite == null) return null;

            var bounds = sprite.bounds;
            var ext    = bounds.extents;

            //ローカル空間のOBB四隅
            Vector3[] localCorners = new Vector3[]
            {
                new(-ext.x, -ext.y, 0),
                new( ext.x, -ext.y, 0),
                new( ext.x,  ext.y, 0),
                new(-ext.x,  ext.y, 0),
            };

            for (int i = 0; i < 4; i++)
            {
                worldCorners[i] = tf.TransformPoint(localCorners[i]);
            }
        }
        else
        {
            return null;
        }

        //四つ角をスクリーン上の座標に変換
        Vector2[] screenPts = new Vector2[4];
        for (int i = 0; i < 4; i++)
        {
            if (obj is RectTransform)
            {
                //CanvaModeで分岐
                if(canvas.renderMode == RenderMode.ScreenSpaceOverlay)
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
        return screenPts;
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

            var quad = CalcScreenQuad(nonUI, targetCanvas, cam);
            if (quad != null) DrawQuadGizmo(quad, cam, strategy);
        }

        Gizmos.color = Color.cyan;
        foreach (var ui in UIs)
        {
            if (ui == null) continue;

            var quad = CalcScreenQuad(ui, targetCanvas, cam);
            if (quad != null) DrawQuadGizmo(quad, cam, strategy);
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