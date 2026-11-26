using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Make Variant (Claude/Documentation/Model Variants Plan.md §6, V2): the prompt opened from a selected asset part's
/// context menu or the Model Selector, the background write of the variant's files, then on the main thread the file
/// lists, the param rows (pending in the Param Editor, decision D1) and the parts switched to the variant (one undo
/// step). A result popup offers to save the params.
/// </summary>
public class AssetVariantTool
{
    /// <summary>The Map Editor view whose parts can be switched; null for the Model Editor's instance.</summary>
    private readonly MapEditorView View;
    private readonly ProjectEntry Project;

    private AssetVariantPlan Plan;
    private List<Entity> Parts = new();
    private string NameInput = "";
    private bool OpenPrompt;

    // The files the current name would write, rebuilt when the name changes
    private string FilesFor;
    private List<(string Source, string Target)> FilesPreview = new();

    private volatile bool Running;
    private readonly ConcurrentQueue<Action> MainThreadQueue = new();

    // The result popup
    private bool OpenResult;
    private List<string> ResultLines = new();
    private bool ResultHasParams;
    private string ResultTarget;

    /// <param name="view">The Map Editor view, or null when used from the Model Editor (no parts to switch).</param>
    public AssetVariantTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    private bool Supported => AssetVariant.IsSupported(Project.Descriptor.ProjectType);

    /// <summary>The asset model the selected asset parts share, or null (none selected, or several models).</summary>
    public string SelectedAssetModel(out List<Entity> parts)
    {
        if (View == null)
        {
            parts = new();
            return null;
        }

        parts =View.ViewportSelection.GetFilteredSelection<Entity>().Where(EntityHelper.IsPartAsset).ToList();

        var models = parts.Select(p => AssetVariant.NormalizeName(p.GetPropertyValue<string>("ModelName")))
            .Distinct().ToList();

        return models.Count == 1 ? models[0] : null;
    }

    #region Entry points
    /// <summary>Context menu of a map object (Map Contents and Map Data): for selected assets of one model.</summary>
    public void OnContext(Entity ent)
    {
        if (!Supported || !EntityHelper.IsPartAsset(ent))
            return;

        var model = SelectedAssetModel(out var parts);

        if (model == null)
            return;

        if (ImGui.Selectable($"{LOC.Get("MAP_AssetVariant_Context")}##assetVariantContext"))
            Begin(model, parts);

        GUI.Tooltip(LOC.Get("MAP_AssetVariant_Context_TT", AssetVariant.ModelName(model), parts.Count));
    }

    /// <summary>
    /// Context menu of an asset in the Model Selector: a variant of that asset; the selected asset parts of that model
    /// are switched to it.
    /// </summary>
    public void OnModelSelectorContext(string assetName)
    {
        var model = AssetVariant.NormalizeName(assetName);

        if (!Supported || model == null || View == null)
            return;

        if (ImGui.Selectable($"{LOC.Get("MAP_AssetVariant_Context")}##assetVariantSelectorContext"))
        {
            var parts = View.ViewportSelection.GetFilteredSelection<Entity>()
                .Where(EntityHelper.IsPartAsset)
                .Where(p => AssetVariant.NormalizeName(p.GetPropertyValue<string>("ModelName")) == model)
                .ToList();

            Begin(model, parts);
        }

        GUI.Tooltip(LOC.Get("MAP_AssetVariant_Selector_Context_TT", AssetVariant.ModelName(model)));
    }

    /// <summary>Opens the prompt for a variant of the asset; <paramref name="parts" /> are switched to it when made.</summary>
    public void Begin(string sourceName, List<Entity> parts)
    {
        if (Running)
        {
            ShowResult(new List<string> { LOC.Get("MAP_AssetVariant_Busy") }, false);
            return;
        }

        var plan = AssetVariant.CreatePlan(Project, sourceName, out var error);

        if (plan == null)
        {
            ShowResult(new List<string> { error }, false);
            return;
        }

        Plan = plan;
        Parts = parts ?? new();
        NameInput = plan.TargetName ?? "";
        plan.CopyParamRows = CFG.Current.MapEditor.MapEditor_AssetVariant_Copy_Param_Rows;
        FilesFor = null;
        OpenPrompt = true;
    }
    #endregion

    /// <summary>The prompt and result popups; drawn every frame by the view so they don't depend on a tool window.</summary>
    public void OnGui()
    {
        while (MainThreadQueue.TryDequeue(out var action))
            action();

        DisplayPrompt();
        DisplayResult();
    }

    #region Prompt
    private void DisplayPrompt()
    {
        var title = $"{LOC.Get("MAP_AssetVariant_Prompt_Title")}###assetVariantPrompt";

        if (OpenPrompt)
        {
            ImGui.OpenPopup(title);
            OpenPrompt = false;
        }

        var open = true;
        ImGui.SetNextWindowSize(new Vector2(560, 0) * DPI.UIScale(), ImGuiCond.Always);

        if (!ImGui.BeginPopupModal(title, ref open, ImGuiWindowFlags.AlwaysAutoResize))
            return;

        var plan = Plan;

        if (plan == null)
        {
            ImGui.CloseCurrentPopup();
            ImGui.EndPopup();
            return;
        }

        GUI.WrappedText(LOC.Get("MAP_AssetVariant_Prompt", AssetVariant.ModelName(plan.SourceName)));

        // Name
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_AssetVariant_Name_Header"), LOC.Get("MAP_AssetVariant_Name_Header_TT"));

        ImGui.SetNextItemWidth(160 * DPI.UIScale());
        ImGui.InputText("##assetVariantName", ref NameInput, 16);
        GUI.Tooltip(LOC.Get("MAP_AssetVariant_Name_TT"));

        ImGui.SameLine();
        if (ImGui.Button($"{LOC.Get("MAP_AssetVariant_Suggest")}##assetVariantSuggest"))
            NameInput = AssetVariant.SuggestName(Project, plan) ?? NameInput;
        GUI.Tooltip(LOC.Get("MAP_AssetVariant_Suggest_TT"));

        // The texture containers' names follow the group option (the asset's name is suggested with Suggest)
        if (ImGui.Checkbox($"{LOC.Get("MAP_AssetVariant_Use_Project_Group")}##assetVariantProjectGroup",
            ref CFG.Current.MapEditor.MapEditor_AssetVariant_Use_Project_Group))
            AssetVariant.UpdateTextureForkNames(Project, plan);
        GUI.Tooltip(LOC.Get("MAP_AssetVariant_Use_Project_Group_TT"));

        if (CFG.Current.MapEditor.MapEditor_AssetVariant_Use_Project_Group)
        {
            ImGui.SameLine();
            ImGui.SetNextItemWidth(90 * DPI.UIScale());
            if (ImGui.InputInt("##assetVariantProjectGroupNumber", ref CFG.Current.MapEditor.MapEditor_AssetVariant_Project_Group, 1, 10))
            {
                CFG.Current.MapEditor.MapEditor_AssetVariant_Project_Group = Math.Clamp(CFG.Current.MapEditor.MapEditor_AssetVariant_Project_Group, 0, 999);
                AssetVariant.UpdateTextureForkNames(Project, plan);
            }
            GUI.Tooltip(LOC.Get("MAP_AssetVariant_Project_Group_TT"));
        }

        var error = AssetVariant.ValidateName(Project, plan, NameInput);

        if (plan.TargetName == null && error != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_AssetVariant_Group_Full"));

        if (error != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, error);

        // Files
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_AssetVariant_Files_Header"), LOC.Get("MAP_AssetVariant_Files_Header_TT"));

        if (error == null)
        {
            UpdateFilesPreview(plan);

            foreach (var (_, target) in FilesPreview)
                GUI.WrappedText(target);

            GUI.WrappedText(LOC.Get("MAP_AssetVariant_Files_Note"));
        }

        DisplayMaterials(plan);

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_AssetVariant_Options_Header"), "");

        var copyRows = plan.CopyParamRows;
        if (ImGui.Checkbox($"{LOC.Get("MAP_AssetVariant_Copy_Param_Rows")}##assetVariantCopyRows", ref copyRows))
        {
            plan.CopyParamRows = copyRows;
            CFG.Current.MapEditor.MapEditor_AssetVariant_Copy_Param_Rows = copyRows;
        }
        GUI.Tooltip(LOC.Get("MAP_AssetVariant_Copy_Param_Rows_TT", AssetVariantFork.ParamRowId(plan.SourceName)));

        if (Parts.Count > 0)
        {
            ImGui.Checkbox($"{LOC.Get("MAP_AssetVariant_Swap_Parts", Parts.Count)}##assetVariantSwap",
                ref CFG.Current.MapEditor.MapEditor_AssetVariant_Swap_Parts);
            GUI.Tooltip(LOC.Get("MAP_AssetVariant_Swap_Parts_TT"));
        }

        // Actions
        GUI.Spacer();

        if (Project.VFS.ProjectFS == null)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_AssetVariant_Error_No_Project_FS"));

        var confirmed = false;
        var cancelled = false;

        GUI.ConditionalMultiButtonInput("assetVariantPromptActions",
            "assetVariantConfirm", LOC.Get("MAP_AssetVariant_Confirm"), LOC.Get("MAP_AssetVariant_Confirm_TT"),
            () => confirmed = true, error == null && !Running && Project.VFS.ProjectFS != null,

            "assetVariantCancel", LOC.Get("MAP_AssetVariant_Cancel"), LOC.Get("MAP_AssetVariant_Cancel_TT"),
            () => cancelled = true, true);

        if (confirmed || cancelled || !open)
            ImGui.CloseCurrentPopup();

        ImGui.EndPopup();

        if (confirmed)
        {
            plan.TargetName = NameInput;
            Make(plan, Parts, Parts.Count > 0 && CFG.Current.MapEditor.MapEditor_AssetVariant_Swap_Parts);
        }
        else if (cancelled || !open)
        {
            Plan = null;
            Parts = new();
        }
    }

    private void UpdateFilesPreview(AssetVariantPlan plan)
    {
        var name = AssetVariant.NormalizeName(NameInput);

        if (name == FilesFor)
            return;

        FilesFor = name;
        FilesPreview = AssetVariantFork.Files(plan.Layout, plan.SourceName, name, Project.VFS.FS.FileExists);
        AssetVariant.UpdateForkNames(Project, plan, name);
    }

    /// <summary>The source's materials, each shared with the source (default) or forked to a copy for the variant.</summary>
    private void DisplayMaterials(AssetVariantPlan plan)
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_AssetVariant_Materials_Header"), LOC.Get("MAP_AssetVariant_Materials_Header_TT"));

        if (plan.Materials.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_AssetVariant_Materials_None"));
            return;
        }

        var available = AssetVariant.MaterialsAvailable(Project, out _, out _);

        if (!available)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_AssetVariant_Materials_Unavailable"));

        if (available && plan.Materials.Any(m => m.CanFork))
        {
            if (ImGui.Button($"{LOC.Get("MAP_AssetVariant_Fork_All")}##assetVariantForkAll"))
                plan.Materials.Where(m => m.CanFork).ToList().ForEach(m => m.Fork = true);
            GUI.Tooltip(LOC.Get("MAP_AssetVariant_Fork_All_TT"));

            ImGui.SameLine();
            if (ImGui.Button($"{LOC.Get("MAP_AssetVariant_Share_All")}##assetVariantShareAll"))
                plan.Materials.ForEach(m => m.Fork = false);
            GUI.Tooltip(LOC.Get("MAP_AssetVariant_Share_All_TT"));
        }

        for (var i = 0; i < plan.Materials.Count; i++)
        {
            var material = plan.Materials[i];

            if (!material.CanFork || !available)
            {
                GUI.WrappedText(LOC.Get(material.CanFork ? "MAP_AssetVariant_Material_Shared" : "MAP_AssetVariant_Material_No_Matbin", material.ShortName));
                continue;
            }

            var fork = material.Fork;
            if (ImGui.Checkbox($"{material.ShortName}##assetVariantFork{i}", ref fork))
                material.Fork = fork;

            var users = material.OtherUsers < 0
                ? LOC.Get("MAP_AssetVariant_Material_Users_Unknown")
                : LOC.Get("MAP_AssetVariant_Material_Users", material.OtherUsers);
            GUI.Tooltip(LOC.Get("MAP_AssetVariant_Material_TT", material.Shader, users));

            ImGui.SameLine();
            if (material.Fork)
                GUI.WrappedText(LOC.Get("MAP_AssetVariant_Material_Fork_To", material.ForkName ?? "?"));
            else
                GUI.WrappedText(material.OtherUsers > 0
                    ? LOC.Get("MAP_AssetVariant_Material_Shared_With", material.OtherUsers)
                    : LOC.Get("MAP_AssetVariant_Material_Shared_Short"));

            if (material.Fork)
                DisplayTextures(plan, material, i);
        }
    }

    /// <summary>
    /// Under a forked material: the AET containers its MATBIN names, each shared (the copy still shows the source's
    /// textures) or forked to a container of its own. A container named by several forked materials is one choice.
    /// </summary>
    private void DisplayTextures(AssetVariantPlan plan, AssetVariantMaterial material, int materialIndex)
    {
        var textures = plan.Textures.Where(t => material.Containers.Contains(t.Container)).ToList();

        ImGui.Indent();

        if (textures.Count == 0)
            GUI.WrappedText(LOC.Get("MAP_AssetVariant_Textures_None"));

        foreach (var texture in textures)
        {
            var fork = texture.Fork;
            var label = LOC.Get("MAP_AssetVariant_Texture_Fork", texture.Container.ToUpperInvariant(), texture.TextureNames.Count);

            if (ImGui.Checkbox($"{label}##assetVariantTexture{materialIndex}_{texture.Container}", ref fork))
                texture.Fork = fork;
            GUI.Tooltip(LOC.Get("MAP_AssetVariant_Texture_Fork_TT", string.Join("\n", texture.TextureNames)));

            ImGui.SameLine();
            GUI.WrappedText(texture.Fork
                ? LOC.Get("MAP_AssetVariant_Material_Fork_To", texture.ForkName?.ToUpperInvariant() ?? LOC.Get("MAP_AssetVariant_Texture_Group_Full"))
                : LOC.Get("MAP_AssetVariant_Material_Shared_Short"));
        }

        ImGui.Unindent();
    }
    #endregion

    #region Make
    private void Make(AssetVariantPlan plan, List<Entity> parts, bool swap)
    {
        AssetVariant.Prepare(Project, plan);
        Plan = null;
        Parts = new();
        Running = true;

        Task.Run(() =>
        {
            AssetVariantResult result;

            try
            {
                result = AssetVariant.WriteFiles(Project, plan);
            }
            catch (Exception e)
            {
                result = new AssetVariantResult { Error = LOC.Get("MAP_AssetVariant_Failed", e.Message) };
                Smithbox.LogError(typeof(AssetVariantTool), result.Error, e);
            }

            MainThreadQueue.Enqueue(() => Finish(plan, parts, swap, result));
        });
    }

    /// <summary>After the write, on the main thread: file lists, param rows, part switch, result.</summary>
    private void Finish(AssetVariantPlan plan, List<Entity> parts, bool swap, AssetVariantResult result)
    {
        Running = false;
        var lines = new List<string>();

        // Whatever was written is registered, so the files aren't hidden from the editors even after an error
        AssetVariant.Register(Project, result);

        if (result.Error != null)
        {
            lines.Add(result.Error);
            if (result.Written.Count > 0)
                lines.Add(LOC.Get("MAP_AssetVariant_Partial", string.Join("\n", result.Written)));

            ShowResult(lines, false);
            return;
        }

        lines.Add(LOC.Get("MAP_AssetVariant_Done", AssetVariant.ModelName(plan.SourceName), AssetVariant.ModelName(plan.TargetName), result.Written.Count));

        var paramsAdded = false;

        if (plan.CopyParamRows)
        {
            lines.Add(AssetVariant.CopyParamRows(Project, plan, out paramsAdded));
        }

        // Before the part switch, so the variant's model finds its forked MATBINs when it loads
        var materialLine = AssetVariant.CopyMaterials(Project, plan, out var materialsAdded);
        if (materialLine != null)
            lines.Add(materialLine);

        if (plan.TextureForks.Count > 0)
            lines.Add(LOC.Get("MAP_AssetVariant_Textures_Forked",
                string.Join(", ", plan.TextureForks.Select(f => $"{f.Key.ToUpperInvariant()} -> {f.Value.ToUpperInvariant()}"))));

        if (swap && View != null)
        {
            // Parts deleted since the prompt opened are left out
            var live = parts.Where(p => p.Container?.Objects.Contains(p) == true).ToList();

            if (live.Count > 0)
            {
                View.ModelBrowserTool.SetObjectModel(AssetVariant.ModelName(plan.TargetName), FileSelectionType.Asset, "", live);
                lines.Add(LOC.Get("MAP_AssetVariant_Swapped", live.Count));
            }
        }

        // What it was made from, for the Model Editor's Variant tool and the Model Selector (V5, V6)
        AssetVariantLineage.Add(Project, new AssetVariantRecord
        {
            Variant = plan.TargetName,
            Source = plan.SourceName,
            Created = DateTime.Now,
            Materials = new Dictionary<string, string>(AssetVariant.ForkMap(plan), StringComparer.OrdinalIgnoreCase),
            Textures = new Dictionary<string, string>(plan.TextureForks, StringComparer.OrdinalIgnoreCase)
        });

        lines.Add(LOC.Get("MAP_AssetVariant_Undo_Note"));
        Smithbox.Log(typeof(AssetVariantTool), string.Join(" ", lines));

        ShowResult(lines, paramsAdded, materialsAdded);
        ResultTarget = plan.TargetName;
    }
    #endregion

    #region Result
    private bool ResultHasMaterials;

    private void ShowResult(List<string> lines, bool hasParams, bool hasMaterials = false)
    {
        ResultLines = lines;
        ResultHasParams = hasParams;
        ResultHasMaterials = hasMaterials;
        ResultTarget = null;
        OpenResult = true;
    }

    private void DisplayResult()
    {
        var title = $"{LOC.Get("MAP_AssetVariant_Result_Title")}###assetVariantResult";

        if (OpenResult)
        {
            ImGui.OpenPopup(title);
            OpenResult = false;
        }

        var open = true;
        ImGui.SetNextWindowSize(new Vector2(520, 0) * DPI.UIScale(), ImGuiCond.Always);

        if (!ImGui.BeginPopupModal(title, ref open, ImGuiWindowFlags.AlwaysAutoResize))
            return;

        foreach (var line in ResultLines)
            GUI.WrappedText(line);

        GUI.Spacer();

        var close = false;

        // Decision D1: the shared files are saved only when asked, and the tooltip says that it saves every edit there
        var saveParams = ResultHasParams && Project.Handler.ParamEditor != null;
        var saveMaterials = ResultHasMaterials && Project.Handler.MaterialEditor != null;

        if (saveParams || saveMaterials)
        {
            var label = saveParams && saveMaterials ? "MAP_AssetVariant_Save_Both"
                : saveParams ? "MAP_AssetVariant_Save_Params" : "MAP_AssetVariant_Save_Materials";

            if (ImGui.Button($"{LOC.Get(label)}##assetVariantSave"))
            {
                if (saveParams)
                    _ = Project.Handler.ParamEditor.SaveParams();

                if (saveMaterials)
                    Project.Handler.MaterialEditor.Save();

                close = true;
            }
            GUI.Tooltip(LOC.Get(label + "_TT"));

            ImGui.SameLine();
        }

        if (ResultTarget != null)
        {
            if (ImGui.Button($"{LOC.Get("MAP_AssetVariant_Open_Model_Editor")}##assetVariantOpenModel"))
            {
                EditorCommandQueue.AddCommand($"model/load/{ResultTarget}/Asset");
                close = true;
            }
            GUI.Tooltip(LOC.Get("MAP_AssetVariant_Open_Model_Editor_TT", AssetVariant.ModelName(ResultTarget)));

            ImGui.SameLine();
        }

        if (ImGui.Button($"{LOC.Get("MAP_AssetVariant_Close")}##assetVariantResultClose"))
            close = true;

        if (close || !open)
            ImGui.CloseCurrentPopup();

        ImGui.EndPopup();
    }
    #endregion
}
