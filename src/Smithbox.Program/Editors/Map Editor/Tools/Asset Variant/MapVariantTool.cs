using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Map Editor's Variant tool (Model Variants Plan.md §6, V5): a Make Variant section for the selected asset parts
/// (the same prompt as their context menu's Make Variant...), the variants already made of their model with a switch
/// to each, then the shared variant sections (<see cref="AssetVariantPanel" />) when the selected model is a variant.
/// Collision is edited in the Model Editor, so its section opens the model there.
/// </summary>
public class MapVariantTool
{
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;
    private readonly AssetVariantPanel Panel;

    public MapVariantTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        // Tints and shifts go into this view's history, so its undo / redo (and History tool) cover them
        Panel = new AssetVariantPanel(project, "mapVariant", () => View.ViewportActionManager,
            new(() => CFG.Current.MapEditor.MapEditor_VariantTool_Show_Tint, v => CFG.Current.MapEditor.MapEditor_VariantTool_Show_Tint = v),
            new(() => CFG.Current.MapEditor.MapEditor_VariantTool_Show_Colour_Shift, v => CFG.Current.MapEditor.MapEditor_VariantTool_Show_Colour_Shift = v),
            new(() => CFG.Current.MapEditor.MapEditor_VariantTool_Show_Collision, v => CFG.Current.MapEditor.MapEditor_VariantTool_Show_Collision = v));
    }

    public bool CanUse() => AssetVariant.IsSupported(Project.Descriptor.ProjectType);

    /// <summary>Finishes a colour shift on the main thread; called every frame by the view.</summary>
    public void Update() => Panel.Update();

    public void OnToolWindow()
    {
        if (!CanUse())
            return;

        var model = View.AssetVariantTool.SelectedAssetModel(out var parts);

        DisplayMake(model, parts);

        if (model == null)
            return;

        var record = AssetVariantLineage.Find(Project, model);

        GUI.Spacer();

        if (record == null)
        {
            GUI.WrappedText(LOC.Get("MAP_VariantTool_Not_Variant", AssetVariant.ModelName(model)));
            return;
        }

        Panel.Display(model, record, () => DisplayCollision(model));
    }

    /// <summary>
    /// The context menu's Make Variant..., for the selection: which parts and model it would use, the button that opens
    /// the prompt, and the variants already made of that model, each with a switch for the selected parts.
    /// </summary>
    private void DisplayMake(string model, System.Collections.Generic.List<Entity> parts)
    {
        GUI.ConditionalHeader(LOC.Get("MAP_VariantTool_Make_Header"), LOC.Get("MAP_VariantTool_Make_Header_TT"),
            ref CFG.Current.MapEditor.MapEditor_VariantTool_Show_Make);

        if (!CFG.Current.MapEditor.MapEditor_VariantTool_Show_Make)
            return;

        if (model == null)
        {
            GUI.WrappedText(parts.Count > 0
                ? LOC.Get("MAP_VariantTool_Make_Several_Models", parts.Count)
                : LOC.Get("MAP_VariantTool_Make_Select"));
            return;
        }

        GUI.WrappedText(LOC.Get("MAP_VariantTool_Make_Selection", parts.Count, AssetVariant.ModelName(model)));

        GUI.MultiButtonInput("mapVariantMakeActions", "mapVariantMake",
            LOC.Get("MAP_AssetVariant_Context"), LOC.Get("MAP_AssetVariant_Context_TT", AssetVariant.ModelName(model), parts.Count),
            () => View.AssetVariantTool.Begin(model, parts));

        // Variants already made of this model (or of its source, when it is one)
        var source = AssetVariantLineage.Find(Project, model)?.Source ?? model;
        var variants = AssetVariantLineage.VariantsOf(Project, source)
            .Select(v => v.Variant)
            .Prepend(source)
            .Where(v => v != model)
            .ToList();

        // The source and its variants, less the selection's own model: nothing to switch to when it has none
        if (variants.Count == 0)
            return;

        GUI.Spacer();
        GUI.WrappedText(LOC.Get("MAP_VariantTool_Make_Switch", AssetVariant.ModelName(source)));

        foreach (var variant in variants)
        {
            var label = variant == source
                ? LOC.Get("MAP_VariantTool_Make_Switch_Source", AssetVariant.ModelName(variant))
                : AssetVariant.ModelName(variant);

            if (ImGui.Button($"{label}##mapVariantSwitch{variant}"))
                View.ModelBrowserTool.SetObjectModel(AssetVariant.ModelName(variant), FileSelectionType.Asset, "", parts);
            GUI.Tooltip(LOC.Get("MAP_VariantTool_Make_Switch_TT", parts.Count, AssetVariant.ModelName(variant)));

            ImGui.SameLine();
        }

        ImGui.NewLine();
    }

    /// <summary>Collision is edited in the Model Editor's Collision tool: open the variant there.</summary>
    private void DisplayCollision(string model)
    {
        GUI.WrappedText(LOC.Get("MAP_VariantTool_Collision"));

        GUI.MultiButtonInput("mapVariantCollisionActions", "mapVariantOpenModel",
            LOC.Get("MAP_AssetVariant_Open_Model_Editor"), LOC.Get("MAP_AssetVariant_Open_Model_Editor_TT", AssetVariant.ModelName(model)),
            () => EditorCommandQueue.AddCommand($"model/load/{model}/Asset"));
    }
}
