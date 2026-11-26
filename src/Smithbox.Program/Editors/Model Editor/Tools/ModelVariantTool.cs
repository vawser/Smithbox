using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MapEditor;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// The Model Editor's Variant tool (Model Variants Plan.md §6, V5): the shared variant sections
/// (<see cref="AssetVariantPanel" />) for the loaded model, when it was made with Make Variant. Its collision is edited
/// here, in the Collision tool's Replace Materials.
/// </summary>
public class ModelVariantTool
{
    private readonly ModelEditorView View;
    private readonly ProjectEntry Project;
    private readonly AssetVariantPanel Panel;

    public ModelVariantTool(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        // Tints and shifts go into this view's history, so its undo / redo (and History tool) cover them
        Panel = new AssetVariantPanel(project, "modelVariant", () => View.ViewportActionManager,
            new(() => CFG.Current.ModelEditor.ModelEditor_VariantTool_Show_Tint, v => CFG.Current.ModelEditor.ModelEditor_VariantTool_Show_Tint = v),
            new(() => CFG.Current.ModelEditor.ModelEditor_VariantTool_Show_Colour_Shift, v => CFG.Current.ModelEditor.ModelEditor_VariantTool_Show_Colour_Shift = v),
            new(() => CFG.Current.ModelEditor.ModelEditor_VariantTool_Show_Collision, v => CFG.Current.ModelEditor.ModelEditor_VariantTool_Show_Collision = v));
    }

    /// <summary>Finishes a colour shift on the main thread; called every frame by the view.</summary>
    public void Update() => Panel.Update();

    public void OnToolWindow()
    {
        if (!AssetVariant.IsSupported(Project.Descriptor.ProjectType))
            return;

        if (!ToolSections.Header($"{LOC.Get("MODEL_VariantTool_Header")}##modelVariantToolHeader"))
            return;

        var wrapper = View.Selection.SelectedModelWrapper;
        var asset = wrapper?.Parent == null ? null : AssetVariant.NormalizeName(wrapper.Parent.Name);

        if (asset == null)
        {
            GUI.WrappedText(LOC.Get("MODEL_VariantTool_No_Asset"));
            return;
        }

        var record = AssetVariantLineage.Find(Project, asset);

        if (record == null)
        {
            GUI.WrappedText(LOC.Get("MODEL_VariantTool_Not_Variant", AssetVariant.ModelName(asset)));
            return;
        }

        Panel.Display(asset, record, () => GUI.WrappedText(LOC.Get("MODEL_VariantTool_Collision")));
    }
}
