using StudioCore.Application;
using Veldrid;

namespace StudioCore.Renderer;

/// <summary>
///     Pipeline state shared by collision and navmesh meshes, which are see-through to the degree set by
///     <see cref="CFG.Viewport_Collision_Opacity" />. The opacity itself reaches the shaders through
///     <see cref="SceneParam.CollisionOpacity" />, so only crossing between see-through and opaque needs a rebuild.
/// </summary>
public static class CollisionTransparency
{
    // What the pipelines were last built for, so they can be rebuilt once the opacity crosses over
    private static bool? _wasTransparent;

    /// <summary>
    ///     Below full opacity the meshes stop writing depth, so that their own hidden faces show through.
    ///     At full opacity they write it again, as otherwise faces would draw over each other in index order.
    /// </summary>
    public static bool IsTransparent => CFG.Current.Viewport_Collision_Opacity < 1.0f;

    public static BlendStateDescription BlendState => IsTransparent
        ? BlendStateDescription.SingleAlphaBlend
        : BlendStateDescription.SingleOverrideBlend;

    public static bool DepthWrite => !IsTransparent;

    /// <summary>
    ///     Rebuilds the collision and navmesh renderables if the opacity has crossed between see-through and opaque.
    ///     Cheap enough to call every frame.
    /// </summary>
    public static void Refresh()
    {
        var transparent = IsTransparent;

        if (_wasTransparent == null)
        {
            _wasTransparent = transparent;
            return;
        }

        if (_wasTransparent == transparent)
        {
            return;
        }

        _wasTransparent = transparent;
        MeshProviderCache.RebuildCollisionRenderables();
    }
}
