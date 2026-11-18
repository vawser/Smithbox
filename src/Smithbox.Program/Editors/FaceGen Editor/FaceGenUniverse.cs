using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;

namespace StudioCore.Editors.FaceGenEditor;

/// <summary>
/// The FaceGen Editor's viewport scene: the character wearing the active face, lit by the shared viewport environment.
/// </summary>
public class FaceGenUniverse : IUniverse
{
    public FaceGenEditorView View;
    public ProjectEntry Project;

    public RenderScene RenderScene => View.RenderScene;
    public ViewportSelection ViewportSelection => View.ViewportSelection;
    public ViewportActionManager ViewportActionManager => View.ViewportActionManager;

    public FaceGenUniverse(FaceGenEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }
}
