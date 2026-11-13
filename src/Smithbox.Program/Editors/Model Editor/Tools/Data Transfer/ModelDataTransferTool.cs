using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ModelEditor;

/// <summary>
/// Data Transfer: copies dummies and materials (with their texture paths and GX lists) into the loaded model from
/// another model, or from a JSON file, and exports the loaded model's to JSON. Bones are found by name in the target,
/// and every import is one undo step. Plan: Documentation/Model Editor Second Review.md, R9.
/// </summary>
public class ModelDataTransferTool
{
    public ModelEditorView View;
    public ProjectEntry Project;

    private static readonly string[] Filters = ["json"];

    private readonly TransferOptions Options = new();

    // From Model: the source picked, and which of its objects are ticked
    private string SourceSearch = "";
    private string SourceSearchFor;
    private List<ModelContainerWrapper> SourceMatches = new();

    private ModelContainerWrapper SourceContainer;
    private ModelWrapper SourceModel;
    private FLVER2 SourceFlver;

    private bool[] DummyTicked = Array.Empty<bool>();
    private bool[] MaterialTicked = Array.Empty<bool>();
    private string DummyFilter = "";

    // Export
    private bool ExportSelectedOnly = true;
    private bool ExportDummies = true;
    private bool ExportMaterials = true;

    // Import: the file read, kept until imported or cleared
    private ModelTransferFile ImportFile;
    private string ImportPath;

    public ModelDataTransferTool(ModelEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private ModelContainer Target
    {
        get
        {
            var wrapper = View.Selection.SelectedModelWrapper;
            return wrapper?.Container?.View == View ? wrapper.Container : null;
        }
    }

    private string GameName => Project.Descriptor.ProjectType.ToString();

    private static string OutputFolder(ProjectEntry project) => Path.Join(project.Descriptor.ProjectPath, "output");

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("MODEL_Transfer_Header")}##modelTransferToolHeader"))
            return;

        ImGui.BeginChild("ModelTransferToolSection", ImGuiChildFlags.Borders);

        GUI.WrappedText(LOC.Get("MODEL_Transfer_Hint"));

        if (ImGui.BeginTabBar("##modelTransferTabs"))
        {
            if (ImGui.BeginTabItem($"{LOC.Get("MODEL_Transfer_Tab_From_Model")}##modelTransferTabFromModel"))
            {
                DisplayFromModel();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem($"{LOC.Get("MODEL_Transfer_Tab_Export")}##modelTransferTabExport"))
            {
                DisplayExport();
                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem($"{LOC.Get("MODEL_Transfer_Tab_Import")}##modelTransferTabImport"))
            {
                DisplayImport();
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        ImGui.EndChild();
    }

    #region From Model
    private void DisplayFromModel()
    {
        // Source Model
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MODEL_Transfer_Source_Header"), LOC.Get("MODEL_Transfer_Source_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputText("##modelTransferSourceSearch", ref SourceSearch, 255);
        GUI.Tooltip(LOC.Get("MODEL_Transfer_Source_Search_TT"));

        DisplaySourceList();

        if (SourceContainer != null && SourceContainer.Models.Count > 1)
        {
            DisplaySourceModels();
        }

        if (SourceFlver == null)
        {
            GUI.WrappedText(LOC.Get("MODEL_Transfer_Source_None"));
            return;
        }

        DisplaySourceContents();
        DisplayOptions();

        // Copy
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MODEL_Transfer_Copy_Header"), LOC.Get("MODEL_Transfer_Copy_Header_TT"));

        var dummies = TickedDummies().ToList();
        var materials = TickedMaterials().ToList();

        if (Target == null)
        {
            GUI.WrappedText(LOC.Get("MODEL_Transfer_No_Target"));
            return;
        }

        DisplayMissingBones(dummies.Select(d => BoneName(SourceFlver, d.ParentBoneIndex))
            .Concat(dummies.Select(d => BoneName(SourceFlver, d.AttachBoneIndex))));

        GUI.ConditionalMultiButtonInput("modelTransferCopyActions",
            "modelTransferCopy", LOC.Get("MODEL_Transfer_Copy", dummies.Count, materials.Count, View.Selection.SelectedModelWrapper?.Name ?? ""),
            LOC.Get("MODEL_Transfer_Copy_TT"),
            () => Import(ModelDataTransfer.Extract(SourceFlver, dummies, materials, SourceModel?.Name, GameName)),
            dummies.Count + materials.Count > 0);
    }

    private void DisplaySourceList()
    {
        var bank = Project.Handler.ModelData?.PrimaryBank;

        if (bank == null)
            return;

        // The matches are kept until the search changes
        if (SourceSearchFor != SourceSearch)
        {
            SourceSearchFor = SourceSearch;

            SourceMatches = string.IsNullOrWhiteSpace(SourceSearch)
                ? new()
                : bank.Models
                    .Where(e => e.Key.Filename.Contains(SourceSearch.Trim(), StringComparison.OrdinalIgnoreCase))
                    .Select(e => e.Value)
                    .OrderBy(e => e.Name, StringComparer.OrdinalIgnoreCase)
                    .ToList();
        }

        if (SourceMatches.Count == 0)
        {
            if (!string.IsNullOrWhiteSpace(SourceSearch))
                GUI.WrappedText(LOC.Get("MODEL_Transfer_Source_No_Match"));

            return;
        }

        ImGui.BeginChild("##modelTransferSourceList", new Vector2(0, 140 * DPI.UIScale()), ImGuiChildFlags.Borders);

        ModelContainerWrapper picked = null;

        var clipper = new ImGuiListClipper();
        clipper.Begin(SourceMatches.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var container = SourceMatches[i];

                if (ImGui.Selectable($"{container.Name}##modelTransferSource{i}", container == SourceContainer))
                {
                    picked = container;
                }
            }
        }

        clipper.End();

        ImGui.EndChild();

        if (picked != null)
        {
            SourceContainer = picked;

            // Lists the binder's models; models with edits in memory are kept
            picked.PopulateModelList();

            // A binder with one model reads it at once
            if (picked.Models.Count > 0)
                PickSource(picked.Models.FirstOrDefault(m => m.Name.Equals(picked.Name, StringComparison.OrdinalIgnoreCase)) ?? picked.Models[0]);
            else
                PickSource(null);
        }
    }

    private void DisplaySourceModels()
    {
        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##modelTransferSourceModel", SourceModel?.Name ?? ""))
        {
            foreach (var model in SourceContainer.Models)
            {
                if (ImGui.Selectable($"{model.Name}##modelTransferSourceModel_{model.Name}", model == SourceModel))
                {
                    PickSource(model);
                }
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("MODEL_Transfer_Source_Model_TT"));
    }

    private void PickSource(ModelWrapper model)
    {
        SourceModel = model;
        SourceFlver = model?.GetFlverForReading();

        DummyTicked = new bool[SourceFlver?.Dummies.Count ?? 0];
        MaterialTicked = new bool[SourceFlver?.Materials.Count ?? 0];

        if (model != null && SourceFlver == null)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Transfer_Source_Read_Failed", model.Name));
        }
    }

    private void DisplaySourceContents()
    {
        // Dummies
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MODEL_Transfer_Dummies_Header", DummyTicked.Count(t => t), DummyTicked.Length),
            LOC.Get("MODEL_Transfer_Dummies_Header_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputText("##modelTransferDummyFilter", ref DummyFilter, 255);
        GUI.Tooltip(LOC.Get("MODEL_Transfer_Dummy_Filter_TT"));

        var query = RefIdQuery.Parse(DummyFilter);
        var shown = Enumerable.Range(0, SourceFlver.Dummies.Count)
            .Where(i => query == null || query.Contains(SourceFlver.Dummies[i].ReferenceID))
            .ToList();

        DisplayTickButtons("Dummies", DummyTicked, shown);

        DisplayTickList("##modelTransferDummies", shown, DummyTicked, i =>
        {
            var d = SourceFlver.Dummies[i];
            return $"[{d.ReferenceID}] {BoneName(SourceFlver, d.ParentBoneIndex) ?? "-"} / {BoneName(SourceFlver, d.AttachBoneIndex) ?? "-"}";
        });

        // Materials
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MODEL_Transfer_Materials_Header", MaterialTicked.Count(t => t), MaterialTicked.Length),
            LOC.Get("MODEL_Transfer_Materials_Header_TT"));

        var allMaterials = Enumerable.Range(0, SourceFlver.Materials.Count).ToList();

        DisplayTickButtons("Materials", MaterialTicked, allMaterials);

        DisplayTickList("##modelTransferMaterials", allMaterials, MaterialTicked, i =>
        {
            var m = SourceFlver.Materials[i];
            return $"{m.Name}  ({Path.GetFileName(m.MTD ?? "")}, {m.Textures.Count} textures)";
        });
    }

    private static void DisplayTickButtons(string id, bool[] ticked, List<int> shown)
    {
        if (ImGui.Button($"{LOC.Get("MODEL_Transfer_Tick_All")}##modelTransferTickAll{id}"))
        {
            foreach (var i in shown)
                ticked[i] = true;
        }
        GUI.Tooltip(LOC.Get("MODEL_Transfer_Tick_All_TT"));

        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("MODEL_Transfer_Tick_None")}##modelTransferTickNone{id}"))
        {
            Array.Clear(ticked);
        }
        GUI.Tooltip(LOC.Get("MODEL_Transfer_Tick_None_TT"));
    }

    private static void DisplayTickList(string id, List<int> shown, bool[] ticked, Func<int, string> label)
    {
        ImGui.BeginChild(id, new Vector2(0, 160 * DPI.UIScale()), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(shown.Count);

        while (clipper.Step())
        {
            for (var row = clipper.DisplayStart; row < clipper.DisplayEnd; row++)
            {
                var i = shown[row];
                ImGui.Checkbox($"{label(i)}##{id}_{i}", ref ticked[i]);
            }
        }

        clipper.End();

        ImGui.EndChild();
    }

    private IEnumerable<FLVER.Dummy> TickedDummies()
    {
        return SourceFlver == null ? Enumerable.Empty<FLVER.Dummy>()
            : SourceFlver.Dummies.Where((_, i) => i < DummyTicked.Length && DummyTicked[i]);
    }

    private IEnumerable<FLVER2.Material> TickedMaterials()
    {
        return SourceFlver == null ? Enumerable.Empty<FLVER2.Material>()
            : SourceFlver.Materials.Where((_, i) => i < MaterialTicked.Length && MaterialTicked[i]);
    }

    private static string BoneName(FLVER2 flver, int index)
    {
        return index >= 0 && index < flver.Nodes.Count ? flver.Nodes[index].Name : null;
    }
    #endregion

    #region Options
    private void DisplayOptions()
    {
        // Options
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MODEL_Transfer_Options_Header"), LOC.Get("MODEL_Transfer_Options_Header_TT"));

        // Missing bones
        if (ImGui.RadioButton($"{LOC.Get("MODEL_Transfer_Missing_Skip")}##modelTransferMissingSkip", Options.MissingBone == TransferMissingBone.Skip))
            Options.MissingBone = TransferMissingBone.Skip;
        GUI.Tooltip(LOC.Get("MODEL_Transfer_Missing_Skip_TT"));

        ImGui.SameLine();

        if (ImGui.RadioButton($"{LOC.Get("MODEL_Transfer_Missing_Top")}##modelTransferMissingTop", Options.MissingBone == TransferMissingBone.TopLevel))
            Options.MissingBone = TransferMissingBone.TopLevel;
        GUI.Tooltip(LOC.Get("MODEL_Transfer_Missing_Top_TT"));

        // Reference ID offset
        ImGui.SetNextItemWidth(-1);
        ImGui.InputInt("##modelTransferRefOffset", ref Options.RefIdOffset);
        GUI.Tooltip(LOC.Get("MODEL_Transfer_Ref_Offset_TT"));

        // Materials
        if (ImGui.RadioButton($"{LOC.Get("MODEL_Transfer_Material_Add")}##modelTransferMaterialAdd", Options.MaterialMode == TransferMaterialMode.AddRenamed))
            Options.MaterialMode = TransferMaterialMode.AddRenamed;
        GUI.Tooltip(LOC.Get("MODEL_Transfer_Material_Add_TT"));

        ImGui.SameLine();

        if (ImGui.RadioButton($"{LOC.Get("MODEL_Transfer_Material_Replace")}##modelTransferMaterialReplace", Options.MaterialMode == TransferMaterialMode.ReplaceSameName))
            Options.MaterialMode = TransferMaterialMode.ReplaceSameName;
        GUI.Tooltip(LOC.Get("MODEL_Transfer_Material_Replace_TT"));

        ImGui.Checkbox($"{LOC.Get("MODEL_Transfer_Copy_Gx")}##modelTransferCopyGx", ref Options.CopyGx);
        GUI.Tooltip(LOC.Get("MODEL_Transfer_Copy_Gx_TT"));
    }

    /// <summary>Names the bones the dummies need that the target lacks, before anything is copied</summary>
    private void DisplayMissingBones(IEnumerable<string> boneNames)
    {
        var target = Target;

        if (target == null)
            return;

        var have = new HashSet<string>(ModelIndexRemapper.GetNodes(target).Select(n => n.Name ?? ""), StringComparer.Ordinal);
        var missing = boneNames.Where(n => n != null && !have.Contains(n)).Distinct().ToList();

        if (missing.Count == 0)
            return;

        var shown = string.Join(", ", missing.Take(6)) + (missing.Count > 6 ? $" (+{missing.Count - 6})" : "");

        GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get(
            Options.MissingBone == TransferMissingBone.Skip ? "MODEL_Transfer_Missing_Bones_Skip" : "MODEL_Transfer_Missing_Bones_Top",
            missing.Count, shown));
    }
    #endregion

    #region Export
    private void DisplayExport()
    {
        var target = Target;

        if (target == null)
        {
            GUI.WrappedText(LOC.Get("MODEL_Transfer_No_Target"));
            return;
        }

        var (dummies, materials) = GetExportObjects(target);

        // What
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MODEL_Transfer_Export_Header"), LOC.Get("MODEL_Transfer_Export_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("MODEL_Transfer_Export_Dummies")}##modelTransferExportDummies", ref ExportDummies);
        GUI.Tooltip(LOC.Get("MODEL_Transfer_Export_Dummies_TT"));

        ImGui.SameLine();

        ImGui.Checkbox($"{LOC.Get("MODEL_Transfer_Export_Materials")}##modelTransferExportMaterials", ref ExportMaterials);
        GUI.Tooltip(LOC.Get("MODEL_Transfer_Export_Materials_TT"));

        ImGui.Checkbox($"{LOC.Get("MODEL_Transfer_Export_Selected_Only")}##modelTransferExportSelected", ref ExportSelectedOnly);
        GUI.Tooltip(LOC.Get("MODEL_Transfer_Export_Selected_Only_TT"));

        GUI.WrappedText(LOC.Get("MODEL_Transfer_Export_Count", dummies.Count, materials.Count));

        GUI.ConditionalMultiButtonInput("modelTransferExportActions",
            "modelTransferExport", LOC.Get("MODEL_Transfer_Export"), LOC.Get("MODEL_Transfer_Export_TT"),
            () => Export(target, dummies, materials), dummies.Count + materials.Count > 0,

            "modelTransferExportClipboard", LOC.Get("MODEL_Transfer_Export_Clipboard"), LOC.Get("MODEL_Transfer_Export_Clipboard_TT"),
            () =>
            {
                PlatformUtils.Instance.SetClipboardText(ModelDataTransfer.ToJson(Extract(target, dummies, materials)));
                Smithbox.Log(this, LOC.Get("MODEL_Transfer_Copied", dummies.Count, materials.Count));
            },
            dummies.Count + materials.Count > 0);
    }

    private (List<FLVER.Dummy> Dummies, List<FLVER2.Material> Materials) GetExportObjects(ModelContainer target)
    {
        IEnumerable<object> source = ExportSelectedOnly
            ? View.ViewportSelection.GetFilteredSelection<Entity>().Where(target.Objects.Contains).Select(e => e.WrappedObject)
            : target.Objects.Select(e => e.WrappedObject);

        var picked = new HashSet<object>(source, ReferenceEqualityComparer.Instance);

        // In model order, so an export keeps the order of the lists
        var dummies = ExportDummies
            ? target.Objects.Select(e => e.WrappedObject).OfType<FLVER.Dummy>().Where(picked.Contains).ToList()
            : new List<FLVER.Dummy>();

        var materials = ExportMaterials
            ? target.Objects.Select(e => e.WrappedObject).OfType<FLVER2.Material>().Where(picked.Contains).ToList()
            : new List<FLVER2.Material>();

        return (dummies, materials);
    }

    private ModelTransferFile Extract(ModelContainer target, List<FLVER.Dummy> dummies, List<FLVER2.Material> materials)
    {
        var wrapper = View.Selection.SelectedModelWrapper;

        // The FLVER's lists follow the objects, for the bone names and GX lists
        wrapper.UpdateFLVER();

        return ModelDataTransfer.Extract(wrapper.FLVER, dummies, materials, wrapper.Name, GameName);
    }

    private void Export(ModelContainer target, List<FLVER.Dummy> dummies, List<FLVER2.Material> materials)
    {
        if (!PlatformUtils.Instance.SaveFileDialog(LOC.Get("MODEL_Transfer_Export_Dialog"), Filters, OutputFolder(Project), out var path) ||
            string.IsNullOrEmpty(path))
            return;

        if (!path.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            path += ".json";

        try
        {
            File.WriteAllText(path, ModelDataTransfer.ToJson(Extract(target, dummies, materials)));
            Smithbox.Log(this, LOC.Get("MODEL_Transfer_Exported", dummies.Count, materials.Count, path));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Transfer_Export_Failed", path), e);
        }
    }
    #endregion

    #region Import
    private void DisplayImport()
    {
        // File
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MODEL_Transfer_Import_Header"), LOC.Get("MODEL_Transfer_Import_Header_TT"));

        GUI.MultiButtonInput("modelTransferImportOpen",
            "modelTransferImportFile", LOC.Get("MODEL_Transfer_Import_Open"), LOC.Get("MODEL_Transfer_Import_Open_TT"),
            OpenImportFile,

            "modelTransferImportClipboard", LOC.Get("MODEL_Transfer_Import_Clipboard"), LOC.Get("MODEL_Transfer_Import_Clipboard_TT"),
            () => ReadImport(PlatformUtils.Instance.GetClipboardText(), LOC.Get("MODEL_Transfer_Clipboard_Name")));

        if (ImportFile == null)
        {
            GUI.WrappedText(LOC.Get("MODEL_Transfer_Import_None"));
            return;
        }

        GUI.WrappedText(LOC.Get("MODEL_Transfer_Import_Summary", ImportPath, ImportFile.SourceModel ?? "?", ImportFile.Game ?? "?",
            ImportFile.Dummies.Count, ImportFile.Materials.Count));

        if (ImportFile.Game != null && ImportFile.Game != GameName)
        {
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MODEL_Transfer_Import_Other_Game", ImportFile.Game, GameName));
        }

        DisplayOptions();

        // Import
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MODEL_Transfer_Copy_Header"), LOC.Get("MODEL_Transfer_Copy_Header_TT"));

        if (Target == null)
        {
            GUI.WrappedText(LOC.Get("MODEL_Transfer_No_Target"));
            return;
        }

        DisplayMissingBones(ImportFile.Dummies.Select(d => d.ParentBone).Concat(ImportFile.Dummies.Select(d => d.AttachBone)));

        GUI.MultiButtonInput("modelTransferImportActions",
            "modelTransferImport", LOC.Get("MODEL_Transfer_Import", ImportFile.Dummies.Count, ImportFile.Materials.Count,
                View.Selection.SelectedModelWrapper?.Name ?? ""),
            LOC.Get("MODEL_Transfer_Copy_TT"),
            () => Import(ImportFile));
    }

    private void OpenImportFile()
    {
        if (!PlatformUtils.Instance.OpenFileDialog(LOC.Get("MODEL_Transfer_Import_Dialog"), Filters, OutputFolder(Project), out var path) ||
            string.IsNullOrEmpty(path))
            return;

        try
        {
            ReadImport(File.ReadAllText(path), Path.GetFileName(path));
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Transfer_Import_Failed", path, e.Message), e);
        }
    }

    private void ReadImport(string text, string name)
    {
        var file = ModelDataTransfer.FromJson(text ?? "", out var error);

        if (file == null)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Transfer_Import_Failed", name, error));
            return;
        }

        ImportFile = file;
        ImportPath = name;
    }
    #endregion

    /// <summary>Adds the data to the loaded model as one undo step, and logs what was done and skipped</summary>
    private void Import(ModelTransferFile data)
    {
        var target = Target;

        if (target == null)
            return;

        var action = ModelDataTransfer.BuildImport(View, Project, target, data, Options, out var report);

        if (action != null)
        {
            View.ViewportActionManager.ExecuteAction(action);
        }

        Smithbox.Log(this, LOC.Get("MODEL_Transfer_Done", report.DummiesAdded, report.MaterialsAdded, report.MaterialsReplaced,
            report.GxListsAdded, View.Selection.SelectedModelWrapper?.Name ?? ""));

        if (report.SkippedDummies.Count > 0)
        {
            Smithbox.LogError(this, LOC.Get("MODEL_Transfer_Skipped", report.SkippedDummies.Count,
                string.Join("; ", report.SkippedDummies.Take(10))));
        }
    }
}
