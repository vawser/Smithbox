using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// The FX Editor's viewport scene (FX.md, phase V1): the grid, the shared viewport environment's
/// lighting and the selected effect's debug shapes. Particles join it once the runtime exists (V2, V3).
/// </summary>
public class FxUniverse : IUniverse
{
    public FxEditorView View;
    public ProjectEntry Project;

    public RenderScene RenderScene => View.RenderScene;
    public ViewportSelection ViewportSelection => View.ViewportSelection;
    public ViewportActionManager ViewportActionManager => View.ViewportActionManager;

    public FxUniverse(FxEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }
}
