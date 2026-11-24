using StudioCore.Application;
using StudioCore.Renderer;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// What the unattended captures (<see cref="ModelRenderSnapshotTool" />, <see cref="MaterialOracleTool" />) hide for their
/// run, so the images show the models alone: the dummy and bone markers and their labels (CFG, which
/// ModelViewportFilters applies every frame), asset collision (the scene's draw filter) and, optionally, the grid.
/// <see cref="Restore" /> puts them back as they were.
/// </summary>
public sealed class CaptureDisplay
{
    private readonly RenderScene Scene;
    private readonly bool Dummies, Nodes, DummyLabels, NodeLabels, Grid, Collision;
    private readonly bool HidesGrid;

    private CaptureDisplay(RenderScene scene, bool hideGrid)
    {
        Scene = scene;
        HidesGrid = hideGrid;
        Dummies = CFG.Current.ModelEditor.ModelEditor_DisplayDummies;
        Nodes = CFG.Current.ModelEditor.ModelEditor_DisplayNodes;
        DummyLabels = CFG.Current.ModelEditor.ModelEditor_DisplayDummyLabels;
        NodeLabels = CFG.Current.ModelEditor.ModelEditor_DisplayNodeLabels;
        Grid = CFG.Current.Interface.Interface_ModelEditor_Viewport_Grid;
        Collision = scene?.DrawFilter.HasFlag(RenderFilter.Collision) ?? false;
    }

    /// <summary>Hides them for a run, returning what to restore afterwards</summary>
    public static CaptureDisplay Hide(RenderScene scene, bool hideGrid)
    {
        var saved = new CaptureDisplay(scene, hideGrid);

        CFG.Current.ModelEditor.ModelEditor_DisplayDummies = false;
        CFG.Current.ModelEditor.ModelEditor_DisplayNodes = false;
        CFG.Current.ModelEditor.ModelEditor_DisplayDummyLabels = false;
        CFG.Current.ModelEditor.ModelEditor_DisplayNodeLabels = false;
        scene?.SetDrawFilter(RenderFilter.Collision, false);

        if (hideGrid)
            CFG.Current.Interface.Interface_ModelEditor_Viewport_Grid = false;

        return saved;
    }

    public void Restore()
    {
        CFG.Current.ModelEditor.ModelEditor_DisplayDummies = Dummies;
        CFG.Current.ModelEditor.ModelEditor_DisplayNodes = Nodes;
        CFG.Current.ModelEditor.ModelEditor_DisplayDummyLabels = DummyLabels;
        CFG.Current.ModelEditor.ModelEditor_DisplayNodeLabels = NodeLabels;
        Scene?.SetDrawFilter(RenderFilter.Collision, Collision);

        if (HidesGrid)
            CFG.Current.Interface.Interface_ModelEditor_Viewport_Grid = Grid;
    }
}
