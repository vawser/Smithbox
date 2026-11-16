#nullable enable
using Andre.Formats;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.ProjectileEditor;

/// <summary>
/// The rows the editor can show: a bullet param's, or a spell param's, picked at the top and filtered by ID or name.
/// </summary>
public class ProjectileListView
{
    private readonly ProjectileEditorView View;

    private string? ParamName;
    private string Filter = "";

    // The rows that pass the filter, made again when the param, the filter or the param's rows change
    private List<Param.Row> Rows = new();
    private (Param? Param, int Count, string Filter) RowsFor;

    public bool ScrollToSelection;

    public ProjectileListView(ProjectileEditorView view)
    {
        View = view;
    }

    public void ShowParam(string paramName)
    {
        ParamName = paramName;
    }

    public void Display()
    {
        var project = View.Project;
        var paramNames = ProjectileParams.GetProjectileParams(project);

        if (paramNames.Count == 0)
        {
            GUI.WrappedText(LOC.Get("PROJECTILE_List_No_Params"));
            return;
        }

        // The player's bullets first, where the game has a param of its own for them
        if (ParamName == null || !paramNames.Contains(ParamName))
            ParamName = paramNames.FirstOrDefault(name => name is "Bullet" or "BulletParam") ?? paramNames[0];

        ImGui.SetNextItemWidth(-1);

        if (ImGui.BeginCombo("##projectileParam", ParamName))
        {
            foreach (var name in paramNames)
            {
                var label = ProjectileParams.IsSpellParam(project, name) ? $"{name} ({LOC.Get("PROJECTILE_List_Spell")})" : name;

                if (ImGui.Selectable($"{label}##{name}", name == ParamName))
                    ParamName = name;
            }

            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("PROJECTILE_List_Param_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##projectileFilter", LOC.Get("PROJECTILE_List_Filter_Hint"), ref Filter, 256);

        var param = ProjectileParams.GetParam(project, ParamName);

        if (param == null)
            return;

        UpdateRows(param);

        ImGui.TextDisabled(LOC.Get("PROJECTILE_List_Count", Rows.Count, param.Rows.Count));

        if (ImGui.BeginChild("projectileRows", default, ImGuiChildFlags.Borders))
        {
            DisplayRows();
        }

        ImGui.EndChild();
    }

    private void UpdateRows(Param param)
    {
        var key = (param, param.Rows.Count, Filter);

        if (key == RowsFor)
            return;

        RowsFor = key;

        var filter = Filter.Trim();

        Rows = filter.Length == 0
            ? param.Rows.ToList()
            : param.Rows.Where(row => Matches(row, filter)).ToList();
    }

    private static bool Matches(Param.Row row, string filter)
    {
        return row.ID.ToString().Contains(filter, StringComparison.Ordinal) ||
               row.Name?.Contains(filter, StringComparison.OrdinalIgnoreCase) == true;
    }

    private void DisplayRows()
    {
        var selected = View.Simulator.Row;
        var selectedIndex = ScrollToSelection && selected != null && View.Simulator.ParamName == ParamName
            ? Rows.IndexOf(selected)
            : -1;

        // The row to scroll to is brought into the clipper's range, then into view
        if (selectedIndex >= 0)
        {
            ImGui.SetScrollY(selectedIndex * ImGui.GetTextLineHeightWithSpacing() - ImGui.GetWindowHeight() * 0.4f);
            ScrollToSelection = false;
        }

        var clipper = new ImGuiListClipper();
        clipper.Begin(Rows.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var row = Rows[i];
                var isSelected = row == selected;

                if (ImGui.Selectable($"{ProjectileParams.RowLabel(row.ID, row.Name)}##projectileRow{i}", isSelected))
                    View.Select(ParamName!, row);

                if (ImGui.BeginPopupContextItem($"projectileRowContext{i}"))
                {
                    if (ImGui.Selectable($"{LOC.Get("PROJECTILE_Open_In_Param_Editor")}##openInParamEditor"))
                        EditorCommandQueue.AddCommand($"param/select/-1/{ParamName}/{row.ID}");

                    ImGui.EndPopup();
                }
            }
        }

        clipper.End();
    }
}
