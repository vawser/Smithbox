using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;

namespace StudioCore.Editors.LuaEditor;

/// <summary>
/// The Lua Editor's viewport scene: the battle goal's character and its target, lit by the shared viewport
/// environment (as the Model and FaceGen Editors' scenes are)
/// </summary>
public class LuaUniverse : IUniverse, IViewportWalkerHost
{
    /// <summary>The First Person Mode, which walks the viewport's camera as the target (set by the viewer)</summary>
    public IViewportWalker Walker { get; set; }

    public ProjectEntry Project;

    public RenderScene RenderScene { get; } = new();
    public ViewportSelection ViewportSelection { get; } = new();
    public ViewportActionManager ViewportActionManager { get; } = new();

    /// <summary>The viewport showing this scene, set by the viewer once it has made it</summary>
    public IViewport Viewport;

    /// <summary>What the viewport's gizmo moves: the target character</summary>
    public IGizmoTarget GizmoTarget;

    public LuaUniverse(ProjectEntry project)
    {
        Project = project;
    }
}
