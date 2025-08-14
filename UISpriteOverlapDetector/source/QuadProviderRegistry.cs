using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// QuadProviderの登録と検索を行うレジストリ
/// </summary>
public static class QuadProviderRegistry
{
    private static readonly List<IQuadProvider> providers = new();

    static QuadProviderRegistry()
    {
        //優先度の高い順に登録されるようソートする
        Register(new SpriteRendererQuadProvider());
        Register(new RendererQuadProvider());
        Register(new Collider2DQuadProvider());
    }

    public static void Register(IQuadProvider provider)
    {
        if (provider == null) return;
        providers.Add(provider);
        providers.Sort((a, b) => b.Priority.CompareTo(a.Priority));
    }

    public static bool TryGetWorldQuad(Component c, Vector3[] worldCorners)
    {
        foreach (var p in providers)
        {
            if (p.TryGetWorldQuad(c, worldCorners)) return true;
        }
        return false;
    }
}

