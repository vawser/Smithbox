using StudioCore.Editors.Common;
using StudioCore.Editors.Viewport;
using StudioCore.Renderer;

namespace StudioCore.Editors.FxEditor;

/// <summary>
/// The FX Editor's viewport scene (FX Editor Plan, phase V1): the grid, the shared viewport environment's
/// lighting and the selected effect's debug shapes. Particles join it once the runtime exists (V2, V3).
/// </summary>
public class FxUniverse : IUniverse
{
    public FxEditorScreen Editor;
    public ProjectEntry Project;

    public RenderScene RenderScene => Editor.RenderScene;
    public ViewportSelection ViewportSelection => Editor.ViewportSelection;
    public ViewportActionManager ViewportActionManager => Editor.ViewportActionManager;

    public FxUniverse(FxEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }
}
