using BehaviorEditorNS;
using Hexa.NET.ImGui;
using HKLib.hk2018;
using StudioCore.Configuration;
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
        if (Editor.Selection.SelectedObjects.Count < 1)
        {
            ImGui.Text("No havok objects have been selected.");
            return;
        }

        var operatingObject = Editor.Selection.SelectedObjects.First().HavokObject;

        DisplayHeader();

        // TODO:
        // Handle each havok object discretely, creating a unique display that suits it contents
        // i.e. hkbClipGenerator should just be the standard property editor
        // whilst CustomManualSelectorGenerator should display the list of generators associated with it and flatten some objects 

        if (Editor.Selection.SelectedObjects.Count > 1)
        {
            UIHelper.WrappedTextColored(UI.Current.ImGui_AliasName_Text, "Multiple havok objects have been selected.\nThe fields below represent only the first havok object, but any edits will apply to all selected objects.");
        }

        if (Project.ProjectType is ProjectType.ER)
        {
            if (operatingObject is CustomManualSelectorGenerator)
            {
                CustomManualSelectorGeneratorDisplay();
            }
            else
            {
                GenericFieldDisplay();
            }
        }
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

        // Jump Back
        // TODO: send editor command to select previous selection
    }

    // Generic display
    #region Generic
    public void GenericFieldDisplay()
    {
        var operatingObject = Editor.Selection.SelectedObjects.First().HavokObject;

        ImGui.BeginChild("fieldTableArea");

        var tblFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

        if (ImGui.BeginTable($"fieldTable", 2, tblFlags))
        {
            ImGui.TableSetupColumn("Name", ImGuiTableColumnFlags.WidthFixed);
            ImGui.TableSetupColumn("Value", ImGuiTableColumnFlags.WidthFixed);

            var nodeType = operatingObject.GetType();

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
                    Editor.FieldInput.DisplayFieldInput(operatingObject, i, field);
                }
            }

            ImGui.EndTable();
        }

        ImGui.EndChild();
    }
    #endregion

    #region CustomManualSelectorGenerator
    public void CustomManualSelectorGeneratorDisplay()
    {
        GenericFieldDisplay();

        // Display primitives/enums with reflection as before

        // Display class instances as a quick link: (i.e. hkbTransitionEffect)

        // Display generator list entries: a collapisble header for each entry
    }
    #endregion
}
