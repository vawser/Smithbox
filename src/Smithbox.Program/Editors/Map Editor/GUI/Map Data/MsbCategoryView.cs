using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor;

/// <summary>
/// The 'params' view: select which base category (Model, Part, etc), and then sub-list with the individual cateogories
/// </summary>
public class MsbCategoryView
{
    public MapEditorView View;
    public ProjectEntry Project;

    private MapDataSelection Selection => View.MapDataView.Selection;

    private string BaseCategoryListFilter = "";
    private bool ExactBaseCategoryListFilter = false;

    private string SubCategoryListFilter = "";
    private bool ExactSubCategoryListFilter = false;

    private Dictionary<string, Type> _baseCategories;

    public MsbCategoryView(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;
    }

    public void Display()
    {
        var baseCategories = _baseCategories ??= MsbCategories.GetBaseCategories(View);

        GUI.TitleHeader("Base Categories", "");

        var subCategories = GetSubCategoriesForCurrentBase(baseCategories);

        DisplayBaseCategoryList(baseCategories);

        GUI.TitleHeader("Sub Categories", "");

        DisplaySubCategoryList(subCategories);
    }

    private void DisplayBaseCategoryList(Dictionary<string, Type> baseCategories)
    {
        ImGui.BeginChild($"framedListFilter_BaseCategoryFilter", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar("BaseCategoryFilter", ref BaseCategoryListFilter, ref ExactBaseCategoryListFilter);

        // Toggle: Empty Display
        GUI.DisplayToggleButton("emptyDisplayToggle", Icons.Book,
            ref CFG.Current.MapEditor.MapDataEditor_CategoryView_Display_Empty_Base_Categories,
            "MAPDAT_CategoryView_Toggle_Empty_Hide",
            "MAPDAT_CategoryView_Toggle_Empty_Show",
            "MAPDAT_CategoryView_Toggle_Empty_TT");

        ImGui.EndChild();

        // List box
        ImGui.BeginChild("##BaseCategoryList", new Vector2(0, 200), ImGuiChildFlags.Borders);

        if (Selection.SourceMsb is null)
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

                var count = GetBaseCategoryCount(name, type);

                // Skip empty base categories if the toggle is off
                if (!CFG.Current.MapEditor.MapDataEditor_CategoryView_Display_Empty_Base_Categories && count == 0)
                {
                    continue;
                }

                bool selected = Selection.SelectedBaseCategory == name;

                var right = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X;

                if (ImGui.Selectable($"{name}##baseCategory_{name}", selected))
                {
                    SelectBaseCategory(name, type, baseCategories);
                }

                DisplayCount(count, right);

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

        }
    }

    private void DisplaySubCategoryList(Dictionary<string, Type> subCategories)
    {
        ImGui.BeginChild($"framedListFilter_SubCategoryFilter", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);

        EditorFilters.DisplaySearchbar("SubCategoryFilter", ref SubCategoryListFilter, ref ExactSubCategoryListFilter);

        // Toggle: Empty Display
        GUI.DisplayToggleButton("emptyDisplayToggle", Icons.Book,
            ref CFG.Current.MapEditor.MapDataEditor_CategoryView_Display_Empty_Sub_Categories,
            "MAPDAT_CategoryView_Toggle_Empty_Sub_Hide",
            "MAPDAT_CategoryView_Toggle_Empty_Sub_Show",
            "MAPDAT_CategoryView_Toggle_Empty_Sub_TT");

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

                var count = View.MapDataView.EntryView.GetCachedEntryCount(type);

                // Skip empty sub-categories if the toggle is off
                if (!CFG.Current.MapEditor.MapDataEditor_CategoryView_Display_Empty_Sub_Categories && count == 0)
                {
                    continue;
                }

                bool selected = Selection.SelectedSubCategory == name;

                var right = ImGui.GetCursorPosX() + ImGui.GetContentRegionAvail().X;

                if (ImGui.Selectable($"{name}##subCategory_{name}", selected))
                {
                    SelectSubCategory(name, type);
                }

                DisplayCount(count, right);
            }
        }

        ImGui.EndChild();
    }

    /// <summary>
    /// The entries of a base category: those of its sub-categories, or of its own type for a direct category
    /// </summary>
    private int GetBaseCategoryCount(string name, Type type)
    {
        var subCats = MsbCategories.GetSubCategories(Project, name);

        ICollection<Type> types = subCats.Count > 0 ? subCats.Values : new[] { type };

        return View.MapDataView.EntryView.GetCachedEntryCount(types);
    }

    /// <summary>
    /// Shows a category's entry count, right-aligned on the line of its selectable
    /// </summary>
    private void DisplayCount(int count, float right)
    {
        var text = count.ToString("N0");
        var width = ImGui.CalcTextSize(text).X;

        ImGui.SameLine(right - width);
        ImGui.TextDisabled(text);
    }

    /// <summary>
    /// Selects the category that lists the entry: the one of its own type, or else the most derived one that
    /// holds it (Enemy before Part). Returns false when no category lists it.
    /// </summary>
    public bool SelectCategoryFor(object entry)
    {
        if (entry == null)
            return false;

        var baseCategories = _baseCategories ??= MsbCategories.GetBaseCategories(View);
        var entryType = entry.GetType();

        (string baseName, Type baseType, string subName, Type subType) best = default;

        foreach (var (baseName, baseType) in baseCategories)
        {
            var subCategories = MsbCategories.GetSubCategories(Project, baseName);
            var candidates = subCategories.Count > 0
                ? subCategories.Select(kvp => (kvp.Key, kvp.Value))
                : new[] { (baseName, baseType) };

            foreach (var (subName, subType) in candidates)
            {
                if (!subType.IsAssignableFrom(entryType))
                    continue;

                // A more derived type is a closer match
                if (best.subType == null || best.subType.IsAssignableFrom(subType))
                {
                    best = (baseName, baseType, subName, subType);
                }
            }
        }

        if (best.subType == null)
            return false;

        if (Selection.SelectedSubCategoryType == best.subType)
            return true;

        SelectBaseCategory(best.baseName, best.baseType, baseCategories);

        if (!IsDirectCategory(best.baseName, MsbCategories.GetSubCategories(Project, best.baseName)))
        {
            SelectSubCategory(best.subName, best.subType);
        }

        // The list filters could hide the category that was picked
        BaseCategoryListFilter = "";
        SubCategoryListFilter = "";

        return true;
    }

    private void SelectSubCategory(string name, Type type)
    {
        Selection.SelectedSubCategory = name;
        Selection.SelectedSubCategoryType = type;
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
