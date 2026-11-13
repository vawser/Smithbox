using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// Map Validation: checks the active map with <see cref="MapCheck" />, the same check File › Save runs, and lists
/// the problems by kind. Its own checks (by property name) were retired for the shared ones; the old version is in
/// .archive/MapValidatorTool_PreSaveCheck.cs.txt.
/// </summary>
public class MapValidatorTool
{
    private MapEditorView View;
    private ProjectEntry Project;

    private List<MapCheckIssue> Issues = new();
    private MapContainer CheckedMap;

    private bool FirstValidate = false;

    private static readonly (MapCheckKind Kind, string Key)[] Tabs =
    {
        (MapCheckKind.BrokenReference, "MAP_Validator_Tab_Broken_References"),
        (MapCheckKind.DS2DuplicateID, "MAP_Validator_Tab_DS2_Duplicate_IDs"),
        (MapCheckKind.DuplicateName, "MAP_Validator_Tab_Duplicate_Names"),
        (MapCheckKind.DuplicateEntityID, "MAP_Validator_Tab_Duplicate_Entity_IDs"),
        (MapCheckKind.MissingParamRow, "MAP_Validator_Tab_Missing_Param_Rows")
    };

    public MapValidatorTool(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void OnToolWindow()
    {
        GUI.WrappedText(LOC.Get("MAP_Validator_Hint"));

        var map = View.Selection.SelectedMapContainer;

        if (map == null)
        {
            GUI.WrappedText(LOC.Get("MAP_Validator_No_Map"));
            return;
        }

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Validator_Actions_Header"), LOC.Get("MAP_Validator_Actions_Header_TT"));

        GUI.MultiButtonInput("validateActions",
            "validateMap", LOC.Get("MAP_Validator_Validate"), LOC.Get("MAP_Validator_Validate_TT", map.Name), ValidateMap);

        if (!FirstValidate)
            return;

        // Results
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("MAP_Validator_Results_Header", CheckedMap?.Name ?? ""), LOC.Get("MAP_Validator_Results_Header_TT"));

        if (Issues.Count == 0)
        {
            GUI.WrappedText(LOC.Get("MAP_Validator_No_Issues"));
            return;
        }

        ImGui.BeginChild("ValidationTabs", ImGuiChildFlags.Borders);

        if (ImGui.BeginTabBar("##mapValidationTabBar"))
        {
            foreach (var (kind, key) in Tabs)
            {
                DisplayTab(kind, key);
            }

            ImGui.EndTabBar();
        }

        ImGui.EndChild();
    }

    public void ValidateMap()
    {
        CheckedMap = View.Selection.SelectedMapContainer;
        Issues = CheckedMap != null ? MapCheck.Run(Project, new[] { CheckedMap }) : new();

        FirstValidate = true;
    }

    private void DisplayTab(MapCheckKind kind, string key)
    {
        var entries = Issues.Where(e => e.Kind == kind).ToList();

        if (entries.Count == 0)
            return;

        if (ImGui.BeginTabItem($"{LOC.Get(key, entries.Count)}##validationTab_{kind}"))
        {
            var clipper = new ImGuiListClipper();
            clipper.Begin(entries.Count);

            while (clipper.Step())
            {
                for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                {
                    var entry = entries[i];
                    var current = entry.Map.Objects.Contains(entry.Entity);

                    if (ImGui.Selectable($"{entry.Entity.PrettyName}: {entry.Describe()}##validationEntry_{kind}_{i}", false,
                        current ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
                    {
                        FocusEntry(entry);
                    }
                }
            }

            clipper.End();

            ImGui.EndTabItem();
        }
    }

    private void FocusEntry(MapCheckIssue entry)
    {
        View.ViewportSelection.ClearSelection();
        View.ViewportSelection.AddSelection(entry.Entity);
        View.FrameAction.ApplyViewportFrame();
    }
}
