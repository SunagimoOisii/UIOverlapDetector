using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// BoundsProvider の登録と検索を行うレジストリ
/// </summary>
public static class QuadProviderRegistry
{
    private static readonly List<IBoundsProvider> providers = new();

    static QuadProviderRegistry()
    {
        //優先度の高い順に登録されるようソートする
        Register(new SpriteRendererBoundsProvider());
        Register(new LineRendererBoundsProvider());
        Register(new MeshRendererBoundsProvider());
        Register(new ColliderBoundsProvider());
    }

    public static void Register(IBoundsProvider provider)
    {
        if (provider == null) return;
        providers.Add(provider);
        providers.Sort((a, b) => b.Priority.CompareTo(a.Priority));
    }

    public static bool TryGetWorldBounds(Component c, Vector3[] worldCorners)
    {
        foreach (var p in providers)
        {
            if (p.TryGetWorldBounds(c, worldCorners)) return true;
        }
        return false;
    }
}

