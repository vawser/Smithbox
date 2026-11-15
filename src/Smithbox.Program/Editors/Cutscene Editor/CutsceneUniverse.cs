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
    public CutsceneEditorScreen Editor;
    public ProjectEntry Project;

    public RenderScene RenderScene => Editor.RenderScene;
    public ViewportSelection ViewportSelection => Editor.ViewportSelection;
    public ViewportActionManager ViewportActionManager => Editor.ViewportActionManager;

    public CutsceneUniverse(CutsceneEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }
}
