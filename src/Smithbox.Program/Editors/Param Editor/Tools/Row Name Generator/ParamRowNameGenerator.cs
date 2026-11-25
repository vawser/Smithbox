using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.ParamEditor;

/// <summary>
/// Tool window section, Tools menu entry and row context menu entry for <see cref="RowNameGeneratorCore"/>.
/// The tool window previews the generated names before they're applied; the menu entries apply directly
/// (one undoable step).
/// </summary>
public class ParamRowNameGenerator
{
    public ParamEditorView View;
    public ProjectEntry Project;

    public RowNameGenOptions Options = new();

    // Preview of the last Generate, and the bank it was built from (a reload makes it stale)
    private List<GeneratedRowName> Preview;
    private ParamBank PreviewBank;
    private int PreviewParamCount;

    // A run waiting for its characters' time acts and behaviors to be read
    private RowNameAnimationLoad ActiveLoad;
    private Dictionary<string, List<Andre.Formats.Param.Row>> PendingTargets;
    private ParamBank PendingBank;
    private bool PendingApply;

    private bool IsLoading => ActiveLoad != null;

    public ParamRowNameGenerator(ParamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    #region Tool Window
    public void Display()
    {
        if (!ToolSections.Header($"{LOC.Get("PARAM_RowNameGen_Header")}##rowNameGenHeader"))
            return;

        ImGui.BeginChild("RowNameGenSection", ImGuiChildFlags.Borders);

        GUI.WrappedText(LOC.Get("PARAM_RowNameGen_Hint"));

        // Sources
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_RowNameGen_Header_Sources"),
            LOC.Get("PARAM_RowNameGen_Header_Sources_TT"));

        DisplaySourceOptions();

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_RowNameGen_Header_Options"),
            LOC.Get("PARAM_RowNameGen_Header_Options_TT"));

        DisplayGeneralOptions();

        // Generate
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_RowNameGen_Header_Generate"),
            LOC.Get("PARAM_RowNameGen_Header_Generate_TT"));

        GUI.ConditionalMultiButtonInput("rowNameGenActions",
            "genSelectedRows",
            LOC.Get("PARAM_RowNameGen_Action_Selected_Rows"),
            LOC.Get("PARAM_RowNameGen_Action_Selected_Rows_TT"),
            () => StartRun(RowNameGenScope.SelectedRows, false),
            !IsLoading && View.Selection.RowSelectionExists(),

            "genSelectedParam",
            LOC.Get("PARAM_RowNameGen_Action_Selected_Param"),
            LOC.Get("PARAM_RowNameGen_Action_Selected_Param_TT"),
            () => StartRun(RowNameGenScope.SelectedParam, false),
            !IsLoading && View.Selection.GetActiveParam() != null,

            "genAllParams",
            LOC.Get("PARAM_RowNameGen_Action_All_Params"),
            LOC.Get("PARAM_RowNameGen_Action_All_Params_TT"),
            () => StartRun(RowNameGenScope.AllParams, false),
            !IsLoading);

        DisplayProgress();

        DisplayPreview();

        ImGui.EndChild();
    }

    private void DisplaySourceOptions()
    {
        ImGui.Checkbox($"{LOC.Get("PARAM_RowNameGen_Source_Param_Rules")}##rowNameGen_paramRules", ref Options.UseParamRules);
        GUI.Tooltip(LOC.Get("PARAM_RowNameGen_Source_Param_Rules_TT"));

        ImGui.Checkbox($"{LOC.Get("PARAM_RowNameGen_Source_Row_Text")}##rowNameGen_rowText", ref Options.UseRowText);
        GUI.Tooltip(LOC.Get("PARAM_RowNameGen_Source_Row_Text_TT"));

        ImGui.Checkbox($"{LOC.Get("PARAM_RowNameGen_Source_Animations")}##rowNameGen_animations", ref Options.UseAnimations);
        GUI.Tooltip(LOC.Get("PARAM_RowNameGen_Source_Animations_TT"));

        ImGui.Checkbox($"{LOC.Get("PARAM_RowNameGen_Source_Incoming")}##rowNameGen_incoming", ref Options.UseIncomingReferences);
        GUI.Tooltip(LOC.Get("PARAM_RowNameGen_Source_Incoming_TT"));

        ImGui.Checkbox($"{LOC.Get("PARAM_RowNameGen_Source_Text_Refs")}##rowNameGen_textRefs", ref Options.UseTextReferences);
        GUI.Tooltip(LOC.Get("PARAM_RowNameGen_Source_Text_Refs_TT"));

        ImGui.Checkbox($"{LOC.Get("PARAM_RowNameGen_Source_Param_Refs")}##rowNameGen_paramRefs", ref Options.UseParamReferences);
        GUI.Tooltip(LOC.Get("PARAM_RowNameGen_Source_Param_Refs_TT"));

        ImGui.Checkbox($"{LOC.Get("PARAM_RowNameGen_Source_Aliases")}##rowNameGen_aliases", ref Options.UseAliases);
        GUI.Tooltip(LOC.Get("PARAM_RowNameGen_Source_Aliases_TT"));
    }

    private void DisplayGeneralOptions()
    {
        ImGui.Checkbox($"{LOC.Get("PARAM_RowNameGen_Option_Only_Unnamed")}##rowNameGen_onlyUnnamed", ref Options.OnlyUnnamed);
        GUI.Tooltip(LOC.Get("PARAM_RowNameGen_Option_Only_Unnamed_TT"));

        ImGui.Checkbox($"{LOC.Get("PARAM_RowNameGen_Option_Chain")}##rowNameGen_chain", ref Options.ChainGeneratedNames);
        GUI.Tooltip(LOC.Get("PARAM_RowNameGen_Option_Chain_TT"));

        ImGui.Checkbox($"{LOC.Get("PARAM_RowNameGen_Option_Prefer_Incoming")}##rowNameGen_preferIncoming", ref Options.PreferIncoming);
        GUI.Tooltip(LOC.Get("PARAM_RowNameGen_Option_Prefer_Incoming_TT"));

        ImGui.Checkbox($"{LOC.Get("PARAM_RowNameGen_Option_Source_Label")}##rowNameGen_sourceLabel", ref Options.IncludeSourceLabel);
        GUI.Tooltip(LOC.Get("PARAM_RowNameGen_Option_Source_Label_TT"));

        ImGui.Checkbox($"{LOC.Get("PARAM_RowNameGen_Option_Animation_ID")}##rowNameGen_animationId", ref Options.IncludeAnimationId);
        GUI.Tooltip(LOC.Get("PARAM_RowNameGen_Option_Animation_ID_TT"));

        ImGui.Checkbox($"{LOC.Get("PARAM_RowNameGen_Option_ItemLot_Map")}##rowNameGen_itemLotMap", ref Options.ItemLotMapPrefix);
        GUI.Tooltip(LOC.Get("PARAM_RowNameGen_Option_ItemLot_Map_TT"));

        ImGui.BeginDisabled(IsLoading);

        if (ImGui.Button($"{LOC.Get("PARAM_RowNameGen_Action_Reread_Animations")}##rowNameGen_reread"))
        {
            RowNameAnimationIndex.Clear(Project);
        }
        GUI.Tooltip(LOC.Get("PARAM_RowNameGen_Action_Reread_Animations_TT"));

        ImGui.EndDisabled();
    }

    private void DisplayProgress()
    {
        var load = ActiveLoad;

        if (load == null)
            return;

        GUI.Spacer();

        var total = Math.Max(1, load.Total);
        var completed = Math.Min(load.Completed, total);

        ImGui.ProgressBar((float)completed / total, new Vector2(-1, 0),
            LOC.Get("PARAM_RowNameGen_Progress", completed, load.Total, load.Current));

        if (ImGui.Button($"{LOC.Get("PARAM_RowNameGen_Action_Cancel")}##rowNameGen_cancel"))
        {
            load.Cancel.Cancel();
        }
        GUI.Tooltip(LOC.Get("PARAM_RowNameGen_Action_Cancel_TT"));
    }

    private void DisplayPreview()
    {
        if (Preview == null)
            return;

        // The bank was reloaded since the preview was built: its rows are gone
        if (PreviewBank != View.GetPrimaryBank())
        {
            ClearPreview();
            return;
        }

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("PARAM_RowNameGen_Header_Preview"),
            LOC.Get("PARAM_RowNameGen_Header_Preview_TT"));

        if (Preview.Count == 0)
        {
            GUI.WrappedText(LOC.Get("PARAM_RowNameGen_Preview_None"));

            if (ImGui.Button($"{LOC.Get("PARAM_RowNameGen_Action_Clear")}##rowNameGen_clearEmpty"))
                ClearPreview();

            return;
        }

        GUI.WrappedText(LOC.Get("PARAM_RowNameGen_Preview_Summary", Preview.Count, PreviewParamCount));

        if (ImGui.Button($"{LOC.Get("PARAM_RowNameGen_Action_Apply")}##rowNameGen_apply"))
        {
            ApplyResults(Preview);
            ClearPreview();
            return;
        }
        GUI.Tooltip(LOC.Get("PARAM_RowNameGen_Action_Apply_TT"));

        ImGui.SameLine();

        if (ImGui.Button($"{LOC.Get("PARAM_RowNameGen_Action_Clear")}##rowNameGen_clear"))
        {
            ClearPreview();
            return;
        }
        GUI.Tooltip(LOC.Get("PARAM_RowNameGen_Action_Clear_TT"));

        var tableFlags = ImGuiTableFlags.Borders | ImGuiTableFlags.RowBg | ImGuiTableFlags.ScrollY
            | ImGuiTableFlags.Resizable | ImGuiTableFlags.SizingStretchProp;

        var height = Math.Min(Preview.Count + 1, 16) * ImGui.GetTextLineHeightWithSpacing() + 8f;

        if (ImGui.BeginTable("rowNameGenPreviewTable", 4, tableFlags, new Vector2(0, height)))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn(LOC.Get("PARAM_RowNameGen_Column_Param"), ImGuiTableColumnFlags.None, 1.2f);
            ImGui.TableSetupColumn(LOC.Get("PARAM_RowNameGen_Column_ID"), ImGuiTableColumnFlags.None, 0.6f);
            ImGui.TableSetupColumn(LOC.Get("PARAM_RowNameGen_Column_Name"), ImGuiTableColumnFlags.None, 2f);
            ImGui.TableSetupColumn(LOC.Get("PARAM_RowNameGen_Column_Source"), ImGuiTableColumnFlags.None, 1f);
            ImGui.TableHeadersRow();

            var clipper = new ImGuiListClipper();
            clipper.Begin(Preview.Count);

            while (clipper.Step())
            {
                for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var entry = Preview[i];

                    ImGui.TableNextRow();

                    ImGui.TableSetColumnIndex(0);
                    if (ImGui.Selectable($"{entry.Param}##rowNameGenEntry{i}", false, ImGuiSelectableFlags.SpanAllColumns))
                    {
                        EditorCommandQueue.AddCommand($@"param/select/-1/{entry.Param}/{entry.Row.ID}");
                    }
                    if (ImGui.IsItemHovered())
                    {
                        var tooltip = entry.Detail;

                        if (!string.IsNullOrEmpty(entry.OldName))
                            tooltip += "\n" + LOC.Get("PARAM_RowNameGen_Preview_Old_Name", entry.OldName);

                        ImGui.SetTooltip(tooltip);
                    }

                    ImGui.TableSetColumnIndex(1);
                    ImGui.TextUnformatted($"{entry.Row.ID}");

                    ImGui.TableSetColumnIndex(2);
                    ImGui.TextUnformatted(entry.NewName);

                    ImGui.TableSetColumnIndex(3);
                    ImGui.TextUnformatted(GetSourceName(entry.Source));
                }
            }

            clipper.End();

            ImGui.EndTable();
        }
    }

    private void SetPreview(List<GeneratedRowName> results)
    {
        Preview = results;
        PreviewBank = View.GetPrimaryBank();
        PreviewParamCount = Preview.Select(e => e.Param).Distinct().Count();
    }

    private void ClearPreview()
    {
        Preview = null;
        PreviewBank = null;
        PreviewParamCount = 0;
    }
    #endregion

    #region Menus
    /// <summary>Tools > Row Names > Generate: applies straight away.</summary>
    public void DisplayDropdown()
    {
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_RowNameGen_Menu_Header")}##rowNameGenMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("PARAM_RowNameGen_Action_Selected_Param")}##rowNameGenMenu_selectedParam",
                "", false, !IsLoading && View.Selection.GetActiveParam() != null))
            {
                StartRun(RowNameGenScope.SelectedParam, true);
            }
            GUI.Tooltip(LOC.Get("PARAM_RowNameGen_Menu_Selected_Param_TT"));

            if (ImGui.MenuItem($"{LOC.Get("PARAM_RowNameGen_Action_All_Params")}##rowNameGenMenu_allParams",
                "", false, !IsLoading))
            {
                StartRun(RowNameGenScope.AllParams, true);
            }
            GUI.Tooltip(LOC.Get("PARAM_RowNameGen_Menu_All_Params_TT"));

            ImGui.EndMenu();
        }
    }

    /// <summary>Row list context menu, beside Name Manipulation: names the selected rows straight away.</summary>
    public void DisplayRowContextMenu(string imguiKey)
    {
        if (imguiKey != "name")
            return;

        if (ImGui.Selectable($"{LOC.Get("PARAM_RowNameGen_Context_Action")}##rowNameGenContextAction", false,
            !IsLoading && View.Selection.RowSelectionExists()
                ? ImGuiSelectableFlags.None
                : ImGuiSelectableFlags.Disabled))
        {
            StartRun(RowNameGenScope.SelectedRows, true);
        }
        GUI.Tooltip(LOC.Get("PARAM_RowNameGen_Context_Action_TT"));
    }
    #endregion

    #region Run
    /// <summary>
    /// Captures the rows of the scope, then generates straight away, or once the time acts and behaviors of the
    /// characters those rows need have been read in the background (finished by <see cref="Update"/>).
    /// </summary>
    private void StartRun(RowNameGenScope scope, bool apply)
    {
        if (IsLoading)
            return;

        Dictionary<string, List<Andre.Formats.Param.Row>> targets;
        HashSet<string> characters;

        try
        {
            var core = new RowNameGeneratorCore(View, Options);
            targets = core.GetTargets(scope);
            characters = core.GetAnimationCharacters(targets);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_RowNameGen_Log_Failed"), e);
            return;
        }

        var missing = characters.Where(c => !RowNameAnimationIndex.IsLoaded(Project, c)).ToList();

        if (missing.Count > 0 && Project.Handler.TimeActData != null)
        {
            PendingTargets = targets;
            PendingBank = View.GetPrimaryBank();
            PendingApply = apply;
            ActiveLoad = RowNameAnimationIndex.StartLoad(Project, missing);

            Smithbox.Log(this, LOC.Get("PARAM_RowNameGen_Log_Reading_Animations", missing.Count));
            return;
        }

        Finish(targets, apply);
    }

    /// <summary>Called every frame by the Param Editor</summary>
    public void Update()
    {
        var load = ActiveLoad;

        if (load == null || !load.IsDone)
            return;

        ActiveLoad = null;

        var targets = PendingTargets;
        var bank = PendingBank;
        PendingTargets = null;
        PendingBank = null;

        if (load.Cancel.IsCancellationRequested)
        {
            Smithbox.Log(this, LOC.Get("PARAM_RowNameGen_Log_Cancelled"));
            return;
        }

        // The params were reloaded while reading: the captured rows are gone
        if (bank != View.GetPrimaryBank())
            return;

        Finish(targets, PendingApply);
    }

    private void Finish(Dictionary<string, List<Andre.Formats.Param.Row>> targets, bool apply)
    {
        List<GeneratedRowName> results;

        try
        {
            results = new RowNameGeneratorCore(View, Options).Generate(targets);
        }
        catch (Exception e)
        {
            Smithbox.LogError(this, LOC.Get("PARAM_RowNameGen_Log_Failed"), e);
            results = new List<GeneratedRowName>();
        }

        if (apply)
            ApplyResults(results);
        else
            SetPreview(results);
    }

    private void ApplyResults(List<GeneratedRowName> results)
    {
        if (results.Count == 0)
        {
            Smithbox.Log(this, LOC.Get("PARAM_RowNameGen_Log_None"));
            return;
        }

        var count = RowNameGeneratorCore.Apply(View.Editor, results);

        if (count > 0)
        {
            var paramCount = results.Select(e => e.Param).Distinct().Count();
            Smithbox.Log(this, LOC.Get("PARAM_RowNameGen_Log_Applied", count, paramCount));
        }
        else
        {
            Smithbox.Log(this, LOC.Get("PARAM_RowNameGen_Log_Not_Applied"));
        }
    }

    private static string GetSourceName(RowNameGenSource source)
    {
        return source switch
        {
            RowNameGenSource.RowText => LOC.Get("PARAM_RowNameGen_Source_Short_Row_Text"),
            RowNameGenSource.Animation => LOC.Get("PARAM_RowNameGen_Source_Short_Animation"),
            RowNameGenSource.ParamRule => LOC.Get("PARAM_RowNameGen_Source_Short_Param_Rule"),
            RowNameGenSource.IncomingReference => LOC.Get("PARAM_RowNameGen_Source_Short_Incoming"),
            RowNameGenSource.TextReference => LOC.Get("PARAM_RowNameGen_Source_Short_Text_Ref"),
            RowNameGenSource.ParamReference => LOC.Get("PARAM_RowNameGen_Source_Short_Param_Ref"),
            RowNameGenSource.Alias => LOC.Get("PARAM_RowNameGen_Source_Short_Alias"),
            _ => ""
        };
    }
    #endregion
}
