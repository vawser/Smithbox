using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapDataEditor;

/// <summary>
/// The 'params' view: select which base category (Model, Part, etc), and then sub-list with the individual cateogories
/// </summary>
public class MsbCategoryView
{
    public MapDataEditorView View;
    public ProjectEntry Project;

    private MapDataSelection Selection => View.Selection;

    private string BaseCategoryListFilter = "";
    private bool ExactBaseCategoryListFilter = false;

    private string SubCategoryListFilter = "";
    private bool ExactSubCategoryListFilter = false;

    public MsbCategoryView(MapDataEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {
        var baseCategories = MsbCategories.GetBaseCategories(Project);

        GUI.SimpleHeader("Base Categories", "");

        var subCategories = GetSubCategoriesForCurrentBase(baseCategories);

        DisplayBaseCategoryList(baseCategories);

        GUI.SimpleHeader("Sub Categories", "");

        DisplaySubCategoryList(subCategories);
    }

    private void DisplayBaseCategoryList(Dictionary<string, Type> baseCategories)
    {
        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild($"framedListFilter_BaseCategoryFilter", searchHeight, ImGuiChildFlags.Borders);

        EditorFilters.DisplayListFilter("BaseCategoryFilter", ref BaseCategoryListFilter, ref ExactBaseCategoryListFilter);

        // Toggle Empty Display
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Bars}"))
        {
            CFG.Current.MapDataEditor_CategoryView_Display_Empty_Base_Categories = !CFG.Current.MapDataEditor_CategoryView_Display_Empty_Base_Categories;
        }

        var emptyBaseDisplay = "Hide Empty Base Categories";
        if (CFG.Current.MapDataEditor_CategoryView_Display_Empty_Base_Categories)
            emptyBaseDisplay = "Display Empty Base Categories";

        GUI.Tooltip($"Toggle the display of base categories with no entries.\nCurrent Mode: {emptyBaseDisplay}");

        ImGui.EndChild();

        // List box
        ImGui.BeginChild("##BaseCategoryList", new Vector2(0, 200), ImGuiChildFlags.Borders);

        if (Selection.SelectedMap is null)
        {
            ImGui.TextDisabled("No map selected.");
        }
        else if (baseCategories.Count > 0)
        {
            foreach (var (name, type) in baseCategories)
            {
                if (!EditorFilters.IsMatch(BaseCategoryListFilter, name, ExactBaseCategoryListFilter, type.ToString()))
                {
                    continue;
                }

                // Skip empty base categories if the toggle is off
                if (!CFG.Current.MapDataEditor_CategoryView_Display_Empty_Base_Categories)
                {
                    var subCats = MsbCategories.GetSubCategories(Project, name);
                    bool anyEntries = subCats.Any(kvp => View.MsbEditor.EntryView.GetCachedEntryCount(kvp.Value) > 0);

                    // Also count the base itself for direct categories
                    if (!anyEntries && View.MsbEditor.EntryView.GetCachedEntryCount(type) == 0)
                        continue;
                }

                bool selected = Selection.SelectedBaseCategory == name;

                if (ImGui.Selectable(name, selected))
                {
                    SelectBaseCategory(name, type, baseCategories);
                }

                // Right-click context menu placeholder
                //if (ImGui.BeginPopupContextItem($"##BaseCtx_{name}"))
                //{
                //    ImGui.TextDisabled(name);
                //    ImGui.EndPopup();
                //}
            }
        }

        ImGui.EndChild();
    }

    private void SelectBaseCategory(string name, Type type, Dictionary<string, Type> baseCategories)
    {
        bool sameBase = Selection.SelectedBaseCategory == name;

        Selection.SelectedBaseCategory = name;
        Selection.SelectedBaseCategoryType = type;
        Selection.ResetMsbEntrySelection();

        // Reset sub-selection whenever the base changes.
        if (!sameBase)
        {
            Selection.SelectedSubCategory = null;
            Selection.SelectedSubCategoryType = null;
            SubCategoryListFilter = "";
        }

        var subCategories = MsbCategories.GetSubCategories(Project, name);

        if (IsDirectCategory(name, subCategories))
        {
            if (subCategories.Count == 1)
            {
                var only = subCategories.First();
                Selection.SelectedSubCategory = only.Key;
                Selection.SelectedSubCategoryType = only.Value;
            }
            else
            {
                Selection.SelectedSubCategory = name;
                Selection.SelectedSubCategoryType = type;
            }

            Selection.SelectMapEntry = true;
        }
    }

    private void DisplaySubCategoryList(Dictionary<string, Type> subCategories)
    {
        var searchHeight = new Vector2(0, 36) * DPI.UIScale();
        ImGui.BeginChild($"framedListFilter_SubCategoryFilter", searchHeight, ImGuiChildFlags.Borders);

        EditorFilters.DisplayListFilter("SubCategoryFilter", ref SubCategoryListFilter, ref ExactSubCategoryListFilter);

        // Toggle Empty Display
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.Bars}"))
        {
            CFG.Current.MapDataEditor_CategoryView_Display_Empty_Sub_Categories = !CFG.Current.MapDataEditor_CategoryView_Display_Empty_Sub_Categories;
        }

        var emptyBaseDisplay = "Hide Empty Sub Categories";
        if (CFG.Current.MapDataEditor_CategoryView_Display_Empty_Sub_Categories)
            emptyBaseDisplay = "Display Empty Sub Categories";

        GUI.Tooltip($"Toggle the display of base categories with no entries.\nCurrent Mode: {emptyBaseDisplay}");

        ImGui.EndChild();

        float listHeight = ImGui.GetContentRegionAvail().Y;
        ImGui.BeginChild("##SubCategoryList", new Vector2(0, listHeight), ImGuiChildFlags.Borders);

        bool hasSubPane = subCategories.Count > 0 &&
                            !IsDirectCategory(Selection.SelectedBaseCategory, subCategories);

        if (!hasSubPane)
        {
            ImGui.TextDisabled("No base category selected.");
        }
        else
        {
            foreach (var (name, type) in subCategories)
            {
                if (!EditorFilters.IsMatch(SubCategoryListFilter, name, ExactSubCategoryListFilter, type.ToString()))
                {
                    continue;
                }

                // Skip empty sub-categories if the toggle is off
                if (!CFG.Current.MapDataEditor_CategoryView_Display_Empty_Sub_Categories)
                {
                    if (View.MsbEditor.EntryView.GetCachedEntryCount(type) == 0)
                        continue;
                }

                bool selected = Selection.SelectedSubCategory == name;

                if (ImGui.Selectable(name, selected))
                {
                    SelectSubCategory(name, type);
                }
            }
        }

        ImGui.EndChild();
    }

    private void SelectSubCategory(string name, Type type)
    {
        Selection.SelectedSubCategory = name;
        Selection.SelectedSubCategoryType = type;
        Selection.ResetMsbEntrySelection();
    }

    private Dictionary<string, Type> GetSubCategoriesForCurrentBase(Dictionary<string, Type> baseCategories)
    {
        if (Selection.SelectedBaseCategory is null)
            return new Dictionary<string, Type>();

        return MsbCategories.GetSubCategories(Project, Selection.SelectedBaseCategory);
    }
    private bool IsDirectCategory(string baseName, Dictionary<string, Type> subCategories)
    {
        if (subCategories.Count == 0)
            return true;

        if (subCategories.Count == 1 && subCategories.ContainsKey(baseName))
            return true;

        return false;
    }
}
