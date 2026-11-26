using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;

namespace StudioCore.Editors.AnimationEditor;

/// <summary>
/// The Animation Editor's viewport scene: the open character playing the selected clip, lit by the shared viewport
/// environment.
/// </summary>
public class AnimationUniverse : IUniverse
{
    public AnimationEditorView View;
    public ProjectEntry Project;

    public RenderScene RenderScene => View.RenderScene;
    public ViewportSelection ViewportSelection => View.ViewportSelection;
    public ViewportActionManager ViewportActionManager => View.ViewportActionManager;

    public AnimationUniverse(AnimationEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }
}
