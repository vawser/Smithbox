using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The Save Check window: what <see cref="MapCheck" /> found in the maps a save was about to write. It isn't modal,
/// so problems can be fixed in the editor while it is open, then checked again. Opened by File › Save when an issue
/// stops the save, and by File › Check Maps Now. Plan: Documentation/Map Editor Save Check.md.
/// </summary>
public class MapSaveCheckWindow
{
    private readonly MapEditorScreen Editor;
    private readonly ProjectEntry Project;

    private bool IsOpen;
    private bool FocusOnOpen;

    private List<MapCheckIssue> Issues = new();

    // The maps that were checked, so Check Again looks at the same ones
    private List<MapContainer> Maps = new();

    // Set while drawing the list, applied after it, so the list isn't changed while it is drawn
    private MapCheckIssue PendingClear;

    // Vanilla maps have many duplicate entity IDs (over 1500 in DB_NT's nine maps), so warnings can be hidden to
    // see the errors
    private bool ShowWarnings = true;

    // The rows shown, rebuilt when the issues or the toggle change
    private List<MapCheckIssue> Shown = new();
    private (List<MapCheckIssue> Issues, bool ShowWarnings) ShownFor;

    public MapSaveCheckWindow(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void Open(List<MapCheckIssue> issues, List<MapContainer> maps)
    {
        Issues = issues;
        Maps = maps;
        IsOpen = true;
        FocusOnOpen = true;
    }

    public void Close()
    {
        IsOpen = false;
    }

    private void CheckAgain()
    {
        Issues = MapCheck.Run(Project, Maps.Where(m => m?.LoadState is MapContentLoadState.Loaded));
    }

    private bool StopsSave => MapCheck.StopsSave(Issues, CFG.Current.MapEditor_SaveCheck_Stop_On_Warnings);

    public void Display()
    {
        if (!IsOpen)
            return;

        ImGui.SetNextWindowSize(new Vector2(900, 500) * DPI.UIScale(), ImGuiCond.FirstUseEver);

        if (FocusOnOpen)
        {
            ImGui.SetNextWindowFocus();
            FocusOnOpen = false;
        }

        if (ImGui.Begin($"{LOC.Get("MAP_SaveCheck_Window_Title")}###mapSaveCheckWindow", ref IsOpen, ImGuiWindowFlags.NoDocking))
        {
            GUI.WrappedText(LOC.Get("MAP_SaveCheck_Hint"));

            DisplaySummary();
            DisplayActions();
            DisplayList();
        }

        ImGui.End();

        if (PendingClear is { Slot: ReferenceSlot slot } issue)
        {
            PendingClear = null;

            var view = issue.Map.OwnerView ?? Editor.ViewHandler.ActiveView;
            view?.ViewportActionManager.ExecuteAction(new ClearReferencesAction(new[] { slot }));

            CheckAgain();
        }
    }

    private void DisplaySummary()
    {
        var errors = Issues.Count(i => i.Severity is MapCheckSeverity.Error);
        var warnings = Issues.Count - errors;
        var maps = Issues.Select(i => i.Map).Distinct().Count();

        // Summary
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_SaveCheck_Summary_Header"), LOC.Get("MAP_SaveCheck_Summary_Header_TT"));

        if (Issues.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_SaveCheck_Summary_None", Maps.Count));
        }
        else
        {
            var color = errors > 0 ? UI.Current.ImGui_Warning_Text_Color : UI.Current.ImGui_Default_Text_Color;
            GUI.WrappedTextColored(color, LOC.Get("MAP_SaveCheck_Summary", errors, warnings, maps));

            if (!StopsSave)
            {
                GUI.WrappedText(LOC.Get("MAP_SaveCheck_Summary_Can_Save"));
            }
        }
    }

    private void DisplayActions()
    {
        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_SaveCheck_Actions_Header"), LOC.Get("MAP_SaveCheck_Actions_Header_TT"));

        // Save is offered as Save Anyway while something would stop it
        var saveTitle = StopsSave ? LOC.Get("MAP_SaveCheck_Save_Anyway") : LOC.Get("MAP_SaveCheck_Save");
        var saveTooltip = StopsSave ? LOC.Get("MAP_SaveCheck_Save_Anyway_TT") : LOC.Get("MAP_SaveCheck_Save_TT");

        GUI.MultiButtonInput("mapSaveCheckActions",
            "mapSaveCheckAgain", LOC.Get("MAP_SaveCheck_Check_Again"), LOC.Get("MAP_SaveCheck_Check_Again_TT"),
            CheckAgain,

            "mapSaveCheckSave", saveTitle, saveTooltip,
            () =>
            {
                IsOpen = false;
                Editor.Save(skipCheck: true);
            },

            "mapSaveCheckClose", LOC.Get("MAP_SaveCheck_Close"), LOC.Get("MAP_SaveCheck_Close_TT"),
            () => IsOpen = false);
    }

    private void DisplayList()
    {
        if (Issues.Count == 0)
            return;

        // Issues
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_SaveCheck_List_Header"), LOC.Get("MAP_SaveCheck_List_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("MAP_SaveCheck_Show_Warnings")}##mapSaveCheckShowWarnings", ref ShowWarnings);
        GUI.Tooltip(LOC.Get("MAP_SaveCheck_Show_Warnings_TT"));

        if (!ReferenceEquals(ShownFor.Issues, Issues) || ShownFor.ShowWarnings != ShowWarnings)
        {
            Shown = ShowWarnings ? Issues : Issues.Where(i => i.Severity is MapCheckSeverity.Error).ToList();
            ShownFor = (Issues, ShowWarnings);
        }

        ImGui.BeginChild("mapSaveCheckList", ImGuiChildFlags.Borders);

        var flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.SizingStretchProp | ImGuiTableFlags.ScrollY;

        if (ImGui.BeginTable("mapSaveCheckTable", 5, flags))
        {
            ImGui.TableSetupScrollFreeze(0, 1);
            ImGui.TableSetupColumn(LOC.Get("MAP_SaveCheck_Column_Severity"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(LOC.Get("MAP_SaveCheck_Column_Map"), ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn(LOC.Get("MAP_SaveCheck_Column_Object"), ImGuiTableColumnFlags.WidthStretch, 1.0f);
            ImGui.TableSetupColumn(LOC.Get("MAP_SaveCheck_Column_Problem"), ImGuiTableColumnFlags.WidthStretch, 2.0f);
            ImGui.TableSetupColumn("##fix", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableHeadersRow();

            var clipper = new ImGuiListClipper();
            clipper.Begin(Shown.Count);

            while (clipper.Step())
            {
                for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    DisplayRow(Shown[i], i);
                }
            }

            clipper.End();

            ImGui.EndTable();
        }

        ImGui.EndChild();
    }

    private void DisplayRow(MapCheckIssue issue, int index)
    {
        // An object deleted, or a map unloaded, since the check
        var current = issue.Map.LoadState is MapContentLoadState.Loaded && issue.Map.Objects.Contains(issue.Entity);

        ImGui.TableNextRow();

        // Severity
        ImGui.TableNextColumn();
        if (issue.Severity is MapCheckSeverity.Error)
        {
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("MAP_SaveCheck_Severity_Error"));
        }
        else
        {
            ImGui.Text(LOC.Get("MAP_SaveCheck_Severity_Warning"));
        }

        // Map
        ImGui.TableNextColumn();
        ImGui.Text(issue.Map.Name);

        // Object
        ImGui.TableNextColumn();
        if (ImGui.Selectable($"{issue.Entity.PrettyName}##mapSaveCheckRow{index}", false,
            current ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
        {
            SelectIssue(issue);
        }
        GUI.Tooltip(current ? LOC.Get("MAP_SaveCheck_Select_TT") : LOC.Get("MAP_SaveCheck_Stale_TT"));

        // Problem: one line, as the clipper needs rows of one height; the tooltip has it in full
        ImGui.TableNextColumn();
        var description = issue.Describe();
        ImGui.TextUnformatted(description);
        GUI.Tooltip(description);

        // Fix
        ImGui.TableNextColumn();
        if (issue.Slot != null && current)
        {
            if (ImGui.Button($"{LOC.Get("MAP_SaveCheck_Clear")}##mapSaveCheckClear{index}"))
            {
                PendingClear = issue;
            }
            GUI.Tooltip(LOC.Get("MAP_SaveCheck_Clear_TT", issue.Detail));
        }
    }

    /// <summary>
    /// Selects and frames the object in the view that loaded its map, which holds its selection
    /// </summary>
    private void SelectIssue(MapCheckIssue issue)
    {
        var view = issue.Map.OwnerView ?? Editor.ViewHandler.ActiveView;

        if (view == null)
            return;

        view.ViewportSelection.ClearSelection();
        view.ViewportSelection.AddSelection(issue.Entity);
        view.ViewportSelection.GotoTreeTarget = issue.Entity;

        view.FrameAction.ApplyViewportFrame();
    }
}
