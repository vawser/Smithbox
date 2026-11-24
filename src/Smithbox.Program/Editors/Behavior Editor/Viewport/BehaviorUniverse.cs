using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;

namespace StudioCore.Editors.BehaviorEditor;

/// <summary>
/// The Behavior Editor's viewport scene (Havok behaviors): the character the behavior's clips are played on, lit by
/// the shared viewport environment (as the Model, FaceGen and Lua Editors' scenes are)
/// </summary>
public class BehaviorUniverse : IUniverse
{
    public ProjectEntry Project;

    public RenderScene RenderScene { get; } = new();
    public ViewportSelection ViewportSelection { get; } = new();
    public ViewportActionManager ViewportActionManager { get; } = new();

    /// <summary>The viewport showing this scene, set by the viewer once it has made it</summary>
    public IViewport Viewport;

    public BehaviorUniverse(ProjectEntry project)
    {
        Project = project;
    }
}
