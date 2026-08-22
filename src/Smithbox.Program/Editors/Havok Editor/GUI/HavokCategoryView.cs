using Hexa.NET.ImGui;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using StudioCore.Editors.Common;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.HavokEditor;

public class HavokCategoryView
{
    public HavokEditorView View;
    public ProjectEntry Project;

    public string CategoryFilter = "";
    public bool ExactCategoryFilter = false;

    public HavokCategoryView(HavokEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Draw()
    {
        GUI.SimpleHeader(
            LOC.Get("HAVOK_CategoryView_Header"),
            LOC.Get("HAVOK_CategoryView_Header_TT"));

        DisplayHeader();

        ImGui.BeginChild("havokCategorySection", ImGuiChildFlags.Borders);

        foreach (var category in Enum.GetValues(typeof(HavokCategoryMode)))
        {
            var curCategory = (HavokCategoryMode)category;
            var selected = View.Selection.CategoryMode == curCategory;

            var displayName = LOC.Get(curCategory.GetDisplayName());

            // Normal filter
            var isMatch = EditorFilters.IsMatch(CategoryFilter, displayName, ExactCategoryFilter);

            if (!isMatch)
                continue;

            if (ImGui.Selectable($"{displayName}##categoryEntry", selected))
            {
                View.Selection.ClearSelection();
                View.Selection.CategoryMode = curCategory;
            }
        }

        ImGui.EndChild();
    }

    public void DisplayHeader()
    {
        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild($"framedList_HavokCategoryList", searchHeight, ImGuiChildFlags.Borders);

        EditorFilters.DisplayListFilter("havokCategorySearch", ref CategoryFilter, ref ExactCategoryFilter);

        ImGui.EndChild();
    }
}