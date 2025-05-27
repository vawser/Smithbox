using BehaviorEditorNS;
using Hexa.NET.ImGui;
using StudioCore.Core;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace BehaviorEditorNS;

public class BehaviorFieldView
{
    public BehaviorEditorScreen Editor;
    public ProjectEntry Project;

    public BehaviorFieldView(BehaviorEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void OnGui()
    {
        if (Editor.Selection.SelectedFieldObject == null)
        {
            ImGui.Text("No haovk object has been selected.");
            return;
        }

        DisplayHeader();

        // TODO:
        // Handle each havok object discretely, creating a unique display that suits it contents
        // i.e. hkbClipGenerator should just be the standard property editor
        // whilst CustomManualSelectorGenerator should display the list of generators associated with it and flatten some objects 

        DisplayFields();
    }

    public void DisplayHeader()
    {
        var curInput = Editor.Filters.FieldListInput;
        ImGui.InputText($"Search##searchBar_FieldList", ref curInput, 255);
        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            Editor.Filters.FieldListInput = curInput;
        }

        ImGui.SameLine();
        ImGui.Checkbox($"##searchIgnoreCase_FieldList", ref Editor.Filters.FieldListInput_IgnoreCase);
        UIHelper.Tooltip("If enabled, the search will ignore case.");
    }

    // Generic display
    public void DisplayFields()
    {
        ImGui.BeginChild("fieldTableArea");

        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

        if (ImGui.BeginTable($"fieldTable", 2, tblFlags))
        {
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthFixed);

            var nodeType = Editor.Selection.SelectedFieldObject.GetType();

            FieldInfo[] array = nodeType.GetFields(BindingFlags.Public | BindingFlags.Instance);
            for (int i = 0; i < array.Length; i++)
            {
                FieldInfo? field = array[i];

                if (Editor.Filters.IsBasicMatch(
                    ref Editor.Filters.FieldListInput, Editor.Filters.FieldListInput_IgnoreCase,
                    field.Name, ""))
                {
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.Text($"{field.Name}");

                    ImGui.TableSetColumnIndex(1);
                    Editor.FieldInput.DisplayFieldInput(Editor.Selection.SelectedFieldObject, i, field);
                }
            }

            ImGui.EndTable();
        }

        ImGui.EndChild();
    }
}
