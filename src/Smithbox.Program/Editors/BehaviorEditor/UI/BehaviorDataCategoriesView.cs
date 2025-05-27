using BehaviorEditorNS;
using Hexa.NET.ImGui;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BehaviorEditorNS;

public class BehaviorDataCategoriesView
{
    public BehaviorEditorScreen Editor;
    public ProjectEntry Project;

    public BehaviorDataCategoriesView(BehaviorEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void OnGui()
    {
        DisplayHeader();
        DisplayCategories();
    }
    private void DisplayHeader()
    {
        var curInput = Editor.Filters.DataCategoriesInput;
        ImGui.InputText($"Search##searchBar_DataCategories", ref curInput, 255);
        if (ImGui.IsItemDeactivatedAfterEdit())
        {
            Editor.Filters.DataCategoriesInput = curInput;
        }

        ImGui.SameLine();
        ImGui.Checkbox($"##searchIgnoreCase_DataCategories", ref Editor.Filters.DataCategoriesInput_IgnoreCase);
        UIHelper.Tooltip("If enabled, the search will ignore case.");
    }

    public void DisplayCategories()
    {
        if (Editor.Selection.SelectedInternalFileKey == "")
            return;

        ImGui.BeginChild("BehaviorCategorySection");

        foreach (var entry in Editor.Selection.DisplayCategories)
        {
            var name = entry.Key;
            var objects = entry.Value;

            var isSelected = Editor.Selection.IsCategorySelected(name);

            if (Editor.Filters.IsBasicMatch(
                ref Editor.Filters.DataCategoriesInput, Editor.Filters.DataCategoriesInput_IgnoreCase,
                name, ""))
            {
                if (ImGui.Selectable($"{name}", isSelected))
                {
                    Editor.Selection.SelectCategory(name, objects);
                }

                // Arrow Selection
                if (ImGui.IsItemHovered() && Editor.Selection.ForceSelectObjectCategory)
                {
                    Editor.Selection.ForceSelectObjectCategory = false;
                    Editor.Selection.SelectCategory(name, objects);
                }
                if (ImGui.IsItemFocused() && (InputTracker.GetKey(Veldrid.Key.Up) || InputTracker.GetKey(Veldrid.Key.Down)))
                {
                    Editor.Selection.ForceSelectObjectCategory = true;
                }
            }
        }

        ImGui.EndChild();
    }
}
