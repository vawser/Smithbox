using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// The Cutscene Editor's viewport scene: the actors and cameras of the selected cut, lit by the shared viewport
/// environment. The map behind them comes in a later phase (see Cutscene Editor Analysis).
/// </summary>
public class CutsceneUniverse : IUniverse
{
    public CutsceneEditorView View;
    public ProjectEntry Project;

    public RenderScene RenderScene => View.RenderScene;
    public ViewportSelection ViewportSelection => View.ViewportSelection;
    public ViewportActionManager ViewportActionManager => View.ViewportActionManager;

    public CutsceneUniverse(CutsceneEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }
}
