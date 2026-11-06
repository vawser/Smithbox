using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// A text input for a field path, with a button that lists the fields of the selected map object to pick from
/// </summary>
public class MsbFieldPicker
{
    private readonly MapEditorView View;
    private readonly MsbQueryMatcher Matcher;

    private string Filter = "";
    private bool ExactFilter = false;

    // The fields of the object the list was last opened for
    private object FieldsFor;
    private List<MsbQueryField> Fields;

    public MsbFieldPicker(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Matcher = new MsbQueryMatcher(project);
    }

    /// <summary>
    /// Draws the input and its button on one line. Returns true when the text changed, by typing or by a pick.
    /// Deactivated is set when the input is left after an edit, or a field is picked.
    /// </summary>
    public bool Display(string id, ref string text, string hint, string tooltip, out bool deactivated)
    {
        var changed = false;
        deactivated = false;

        var buttonWidth = DPI.IconButtonSize.X + ImGui.GetStyle().ItemSpacing.X;

        ImGui.SetNextItemWidth(-buttonWidth);

        if (ImGui.InputTextWithHint($"##{id}", hint, ref text, 255))
        {
            changed = true;
        }
        GUI.Tooltip(tooltip);

        deactivated = ImGui.IsItemDeactivatedAfterEdit();

        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Bars}##{id}_pick", DPI.IconButtonSize))
        {
            ImGui.OpenPopup($"##{id}_pickPopup");
        }
        GUI.Tooltip(LOC.Get("MAP_SearchTool_Field_Pick_TT"));

        ImGui.SetNextWindowSize(new Vector2(420, 360) * DPI.UIScale(), ImGuiCond.Appearing);

        if (ImGui.BeginPopup($"##{id}_pickPopup"))
        {
            if (DisplayList(id, out var picked))
            {
                text = picked;
                changed = true;
                deactivated = true;

                ImGui.CloseCurrentPopup();
            }

            ImGui.EndPopup();
        }

        return changed;
    }

    private bool DisplayList(string id, out string picked)
    {
        picked = null;

        var selected = View.ViewportSelection.GetSelection().OfType<MsbEntity>()
            .FirstOrDefault(e => e.WrappedObject is SoulsFormats.IMsbEntry);

        if (selected == null)
        {
            GUI.WrappedText(LOC.Get("MAP_SearchTool_Field_Pick_No_Selection"));
            return false;
        }

        if (FieldsFor != selected.WrappedObject)
        {
            FieldsFor = selected.WrappedObject;
            Fields = Matcher.GetFields(selected.WrappedObject);
        }

        GUI.WrappedText(LOC.Get("MAP_SearchTool_Field_Pick_Hint", selected.Name, selected.WrappedObject.GetType().Name));

        EditorFilters.DisplayFramedListFilter($"{id}_pickFilter", ref Filter, ref ExactFilter);

        ImGui.BeginChild($"##{id}_pickList", new Vector2(0, 0), ImGuiChildFlags.Borders);

        for (int i = 0; i < Fields.Count; i++)
        {
            var field = Fields[i];

            if (!EditorFilters.IsMatch(Filter, field.Path, ExactFilter, field.DisplayName))
                continue;

            var label = field.DisplayName != field.Name ? $"{field.Path}  ({field.DisplayName})" : field.Path;

            if (ImGui.Selectable($"{label}  = {field.Shown}##{id}_pick{i}"))
            {
                picked = field.Path;
            }
        }

        ImGui.EndChild();

        return picked != null;
    }
}
