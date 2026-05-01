using Hexa.NET.ImGui;
using Octokit;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using static SoulsFormats.GPARAM;

namespace StudioCore.Editors.GparamEditor;
public class GparamFieldList
{
    private GparamEditorView Parent;
    private ProjectEntry Project;

    public GparamFieldList(GparamEditorView view, ProjectEntry project)
    {
        Parent = view;
        Project = project;
    }

    /// <summary>
    /// The main UI for the event parameter view
    /// </summary>
    public void Display()
    {
        DisplayHeader();

        // Fields
        ImGui.BeginChild("GparamFieldsSection", ImGuiChildFlags.Borders);
        Parent.Selection.SwitchWindowContext(GparamEditorContext.Field);

        if (Parent.Selection.IsGparamGroupSelected())
        {
            GPARAM.Param data = Parent.Selection.GetSelectedGparamGroup();

            for (int i = 0; i < data.Fields.Count; i++)
            {
                GPARAM.IField entry = data.Fields[i];

                var name = entry.Key;
                var groupId = Parent.Selection.GetSelectedGparamGroup().Key;
                var fieldId = entry.Key;
                var fieldName = GparamMetaUtils.GetFieldName(Project, groupId, fieldId);

                if (fieldName != null)
                {
                    name = fieldName;
                }

                if (Parent.Filters.IsFieldFilterMatch(entry.Name, ""))
                {
                    // Field row
                    if (ImGui.Selectable($@"[{i}] {name}##{entry.Key}{i}", i == Parent.Selection._selectedParamFieldKey))
                    {
                        Parent.Selection.SetGparamField(i, entry);
                    }

                    // Arrow Selection
                    if (ImGui.IsItemHovered() && Parent.Selection.SelectGparamField)
                    {
                        Parent.Selection.SelectGparamField = false;
                        Parent.Selection.SetGparamField(i, entry);
                    }

                    if (ImGui.IsItemFocused())
                    {
                        if (InputManager.HasArrowSelection())
                        {
                            Parent.Selection.SelectGparamField = true;
                        }
                    }
                }

                Parent.ContextMenu.FieldContextMenu(i);
            }
        }

        ImGui.EndChild();
    }

    public void DisplayHeader()
    {
        UIHelper.SimpleHeader("Fields", "");

        // Search
        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild("GparamFieldSearchSection", searchHeight, ImGuiChildFlags.Borders);

        Parent.Filters.DisplayFieldFilterSearch();

        ImGui.EndChild();

    }
}

