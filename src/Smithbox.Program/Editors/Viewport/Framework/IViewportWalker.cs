#nullable enable
using Veldrid.Sdl2;

namespace StudioCore.Editors.Viewport;

/// <summary>
/// Something that walks the viewport's camera in first person (the Lua Editor's First Person Mode): while it walks, it
/// takes the viewport's input each frame, and it draws its own overlay. The Map Editor's first person tool is called
/// by the viewport directly; any other editor's universe offers one through <see cref="IViewportWalkerHost"/>.
/// </summary>
public interface IViewportWalker
{
    /// <summary>True once, when walking starts in this viewport: the viewport window takes the keyboard focus</summary>
    bool TakeViewportFocus(VulkanViewport viewport);

    /// <summary>
    /// Called each frame before the viewport's own input: true while walking in this viewport, which then skips its
    /// camera keys, picking and gizmos
    /// </summary>
    bool UpdateWalking(VulkanViewport viewport, Sdl2Window window, float dt);

    /// <summary>Draws the crosshair and hints over this viewport while walking in it</summary>
    void DrawOverlay(VulkanViewport viewport);
}

/// <summary>A universe whose viewport can be walked in first person</summary>
public interface IViewportWalkerHost
{
    IViewportWalker? Walker { get; }
}
