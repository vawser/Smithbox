using CsvHelper.Configuration.Attributes;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.ParamEditor;


public class ParamToolMenu
{
    public ParamEditorView View;
    public ProjectEntry Project;

    // Finders
    public FieldNameFinder FieldNameFinder;
    public FieldValueFinder FieldValueFinder;
    public RowNameFinder RowNameFinder;
    public RowIdFinder RowIdFinder;
    public ValueSetFinder ValueSetFinder;
    public IdSetFinder IdSetFinder;

    // Param Utilities
    public ParamRowNameTool RowNameTool;
    public ParamDataTransferTool DataConverterTool;
    public ParamSortTool SortTool;
    public ParamComparisonTool ParamComparisonTool;
    public ParamReloader ParamReloader;
    public ParamMerger ParamMerger;
    public ItemGib ItemGib;
    public SpEffectGib SpEffectGib;
    public ParamUpgrader ParamUpgrader;
    public ParamListCategories ParamListCategories;
    public ParamPinGroups PinGroups;
    public ParamDeltaPatcher DeltaPatcher;
    public ParamRowNameManipulationTool RowNameManipulationTool;
    public ParamModifiedFilter ModifiedFilter;
    public ParamEditHistoryTool EditHistory;
    public ParamWeaponGenerator WeaponGenerator;
    public ParamAcAssemblyTool AcAssembly;

    private ToolSections Sections = new();

    public ParamToolMenu(ParamEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        EditHistory = new(view);

        FieldNameFinder = new(view, project);
        FieldValueFinder = new(view, project);
        RowNameFinder = new(view, project);
        RowIdFinder = new(view, project);
        ValueSetFinder = new(view, project);
        IdSetFinder = new(view, project);

        RowNameTool = new(view, project);
        DataConverterTool = new(view, project);
        SortTool = new(view, project);

        ParamComparisonTool = new(view, project);
        ParamReloader = new(view, project);
        ParamMerger = new(view, project);
        ItemGib = new(view, project);
        SpEffectGib = new(view, project, ItemGib);
        ParamUpgrader = new(view, project);
        ParamListCategories = new(view, project);
        PinGroups = new(view, project);
        DeltaPatcher = new(view, project);
        RowNameManipulationTool = new(view, project);
        ModifiedFilter = new(view, project);
        WeaponGenerator = new(view, project);
        AcAssembly = new(view, project);
    }

    public void DisplayMenu()
    {
        // Tools
        if (ImGui.BeginMenu($"{LOC.Get("PARAM_Tools_Header_Tools")}##toolsMenuHeader"))
        {
            SortTool.DisplayDropDown();

            // Data Comparison
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_Tools_Header_Data_Comparison")}##dataComparisonMenuHeader"))
            {
                ParamComparisonTool.DisplayDropdown();

                ImGui.EndMenu();
            }

            // Data Transfer
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_Tools_Header_Data_Transfer")}##dataTransferMenuHeader"))
            {
                DataConverterTool.DisplayDropdown();

                ImGui.EndMenu();
            }

            // Row Names
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_Tools_Header_Row_Names")}##rowNamesMenuHeader"))
            {
                RowNameTool.DisplayDropdown();

                ImGui.EndMenu();
            }

            // Memory
            if (ImGui.BeginMenu($"{LOC.Get("PARAM_Tools_Header_Memory")}##memoryMenuHeader"))
            {
                ParamReloader.DisplayDropdown();

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }

    public void Draw()
    {
        Sections.Begin();

        // General
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_General"));

        EditHistory.OnToolWindow();

        // Search & Edit
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Search"));

        DisplayDataFinders();

        if (ToolSections.Header($"{LOC.Get("PARAM_Tools_Header_Mass_Edit")}##massEditMenuHeader"))
        {
            ImGui.BeginChild("MassEditToolSection", ImGuiChildFlags.Borders);

            View.MassEdit.ToolMenu.Display();

            ImGui.EndChild();
        }

        ModifiedFilter.Display();

        ParamComparisonTool.Display();

        // Rows
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Rows"));

        SortTool.Display();
        RowNameTool.Display();

        if (ToolSections.Header($"{LOC.Get("PARAM_Tools_Header_Row_Name_Manipulation")}##rowNameManipMenuHeader"))
        {
            ImGui.BeginChild("RowNameManipToolSection", ImGuiChildFlags.Borders);

            RowNameManipulationTool.Display();

            ImGui.EndChild();
        }

        WeaponGenerator.Display();

        // AC6 only
        AcAssembly.Display();

        // Data
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Data"));

        DataConverterTool.Display();

        ParamUpgrader.Display();

        ParamMerger.Display();

        DeltaPatcher.Display();

        DeltaPatcher.ExportProgressModal.Draw();
        DeltaPatcher.ImportProgressModal.Draw();
        DeltaPatcher.MergeProgressModal.Draw();

        DeltaPatcher.ImportPreviewModal.Draw();
        DeltaPatcher.ExportPreviewModal.Draw();

        // Organization
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Organization"));

        ParamListCategories.Display();

        PinGroups.Display();

        // Game Memory
        ToolSections.Category(LOC.Get("EDITOR_ToolCategory_Game_Memory"));

        ParamReloader.Display();

        ItemGib.Display();

        SpEffectGib.Display();

        Sections.End();
    }

    private void DisplayDataFinders()
    {
        if (ToolSections.Header($"{LOC.Get("PARAM_Tools_Header_Data_Finders")}##dataFindersMenuHeader"))
        {
            ImGui.BeginChild("DataFinderToolSection", ImGuiChildFlags.Borders);

            if (ImGui.BeginTabBar("dataFinderTabs"))
            {

                if (ImGui.BeginTabItem($"{LOC.Get("PARAM_DataFinder_Tab_Field_Names")}##tab_fieldNames"))
                {
                    FieldNameFinder.Display();

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem($"{LOC.Get("PARAM_DataFinder_Tab_Field_Values")}##tab_fieldValues"))
                {
                    FieldValueFinder.Display();

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem($"{LOC.Get("PARAM_DataFinder_Tab_Row_Names")}##tab_rowNames"))
                {
                    RowNameFinder.Display();

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem($"{LOC.Get("PARAM_DataFinder_Tab_Row_IDs")}##tab_rowIds"))
                {
                    RowIdFinder.Display();

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem($"{LOC.Get("PARAM_DataFinder_Tab_Field_Value_Sets")}##tab_fieldValueSets"))
                {
                    ValueSetFinder.Display();

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem($"{LOC.Get("PARAM_DataFinder_Tab_Row_ID_Sets")}##tab_rowIdSets"))
                {
                    IdSetFinder.Display();

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();
            }

            ImGui.EndChild();
        }
    }
}
