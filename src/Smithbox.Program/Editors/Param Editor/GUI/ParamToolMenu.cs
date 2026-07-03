using CsvHelper.Configuration.Attributes;
using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.ParamEditor;


public class ParamToolMenu
{
    public ParamEditorScreen Editor;
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
    public ParamUpgrader ParamUpgrader;
    public ParamListCategories ParamListCategories;
    public ParamPinGroups PinGroups;
    public ParamDeltaPatcher DeltaPatcher;

    public ParamToolMenu(ParamEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        FieldNameFinder = new(editor, project);
        FieldValueFinder = new(editor, project);
        RowNameFinder = new(editor, project);
        RowIdFinder = new(editor, project);
        ValueSetFinder = new(editor, project);
        IdSetFinder = new(editor, project);

        RowNameTool = new(editor, project);
        DataConverterTool = new(editor, project);
        SortTool = new(editor, project);

        ParamComparisonTool = new(editor, project);
        ParamReloader = new(editor, project);
        ParamMerger = new(editor, project);
        ItemGib = new(editor, project);
        ParamUpgrader = new(editor, project);
        ParamListCategories = new(editor, project);
        PinGroups = new(editor, project);
        DeltaPatcher = new(editor, project);
    }

    public void DisplayMenu()
    {
        var activeView = Editor.ViewHandler.ActiveView;

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
        if (!CFG.Current.Interface_ParamEditor_ToolWindow)
            return;

        ImGui.SetNextWindowClass(ref UIHelper.DockGroup_ParamEditor);
        if (ImGui.Begin($"{LOC.Get("PARAM_Window_Tool_Window")}###toolWindow_ParamEditor", UIHelper.GetMainWindowFlags()))
        {
            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.RootAndChildWindows))
            {
                FocusManager.SetFocus(EditorFocusContext.ParamEditor_Tools);
            }

            if (ImGui.BeginMenuBar())
            {
                // View
                if (ImGui.BeginMenu($"{LOC.Get("PARAM_Tools_Header_View")}##viewMenuHeader"))
                {
                    // Sort Rows
                    if (ImGui.MenuItem($"{LOC.Get("PARAM_Tools_View_Sort_Rows")}##viewToggle_SortRows"))
                    {
                        CFG.Current.ParamEditor_Show_Tool_Sort_Rows = !CFG.Current.ParamEditor_Show_Tool_Sort_Rows;
                    }
                    UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_Show_Tool_Sort_Rows);

                    // Row Names
                    if (ImGui.MenuItem($"{LOC.Get("PARAM_Tools_View_Row_Names")}##viewToggle_RowNames"))
                    {
                        CFG.Current.ParamEditor_Show_Tool_Row_Names = !CFG.Current.ParamEditor_Show_Tool_Row_Names;
                    }
                    UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_Show_Tool_Row_Names);

                    // Data Transfer
                    if (ImGui.MenuItem($"{LOC.Get("PARAM_Tools_View_Data_Transfer")}##viewToggle_DataTransfer"))
                    {
                        CFG.Current.ParamEditor_Show_Tool_Data_Converter = !CFG.Current.ParamEditor_Show_Tool_Data_Converter;
                    }
                    UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_Show_Tool_Data_Converter);

                    // Data Comparison
                    if (ImGui.MenuItem($"{LOC.Get("PARAM_Tools_View_Data_Comparison")}##viewToggle_DataComparison"))
                    {
                        CFG.Current.ParamEditor_Show_Tool_Data_Comparison = !CFG.Current.ParamEditor_Show_Tool_Data_Comparison;
                    }
                    UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_Show_Tool_Data_Comparison);

                    // Data Finders
                    if (ImGui.MenuItem($"{LOC.Get("PARAM_Tools_View_Data_Finders")}##viewToggle_DataFinders"))
                    {
                        CFG.Current.ParamEditor_Show_Tool_Data_Finders = !CFG.Current.ParamEditor_Show_Tool_Data_Finders;
                    }
                    UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_Show_Tool_Data_Finders);

                    // Param Upgrader
                    if (ImGui.MenuItem($"{LOC.Get("PARAM_Tools_View_Param_Upgrader")}##viewToggle_ParamUpgrader"))
                    {
                        CFG.Current.ParamEditor_Show_Tool_Param_Upgrader = !CFG.Current.ParamEditor_Show_Tool_Param_Upgrader;
                    }
                    UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_Show_Tool_Param_Upgrader);

                    // Param Merger
                    if (ImGui.MenuItem($"{LOC.Get("PARAM_Tools_View_Param_Merger")}##viewToggle_ParamMerger"))
                    {
                        CFG.Current.ParamEditor_Show_Tool_Param_Merger = !CFG.Current.ParamEditor_Show_Tool_Param_Merger;
                    }
                    UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_Show_Tool_Param_Merger);

                    // Param Delta Patcher
                    if (ImGui.MenuItem($"{LOC.Get("PARAM_Tools_View_Param_Delta_Patcher")}##viewToggle_ParamDeltaPatcher"))
                    {
                        CFG.Current.ParamEditor_Show_Tool_Param_Delta_Patcher = !CFG.Current.ParamEditor_Show_Tool_Param_Delta_Patcher;
                    }
                    UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_Show_Tool_Param_Delta_Patcher);

                    // Param Reloader
                    if (ImGui.MenuItem($"{LOC.Get("PARAM_Tools_View_Param_Reloader")}##viewToggle_ParamReloader"))
                    {
                        CFG.Current.ParamEditor_Show_Tool_Param_Reloader = !CFG.Current.ParamEditor_Show_Tool_Param_Reloader;
                    }
                    UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_Show_Tool_Param_Reloader);

                    // Item Gib
                    if (ImGui.MenuItem($"{LOC.Get("PARAM_Tools_View_Item_Gib")}##viewToggle_ItemGib"))
                    {
                        CFG.Current.ParamEditor_Show_Tool_Item_Gib = !CFG.Current.ParamEditor_Show_Tool_Item_Gib;
                    }
                    UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_Show_Tool_Item_Gib);

                    // Mass Edit
                    if (ImGui.MenuItem($"{LOC.Get("PARAM_Tools_View_Mass_Edit")}##viewToggle_MassEdit"))
                    {
                        CFG.Current.ParamEditor_Show_Tool_Mass_Edit = !CFG.Current.ParamEditor_Show_Tool_Mass_Edit;
                    }
                    UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_Show_Tool_Mass_Edit);

                    // Param List Categories
                    if (ImGui.MenuItem($"{LOC.Get("PARAM_Tools_View_Param_List_Categories")}##viewToggle_ParamListCategories"))
                    {
                        CFG.Current.ParamEditor_Show_Tool_Param_List_Categories = !CFG.Current.ParamEditor_Show_Tool_Param_List_Categories;
                    }
                    UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_Show_Tool_Param_List_Categories);

                    // Pin Groups
                    if (ImGui.MenuItem($"{LOC.Get("PARAM_Tools_View_Pin_Groups")}##viewToggle_PinGroups"))
                    {
                        CFG.Current.ParamEditor_Show_Tool_Pin_Groups = !CFG.Current.ParamEditor_Show_Tool_Pin_Groups;
                    }
                    UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_Show_Tool_Pin_Groups);

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }

            var activeView = Editor.ViewHandler.ActiveView;

            if (activeView != null)
            {
                if (CFG.Current.ParamEditor_Show_Tool_Sort_Rows)
                {
                    SortTool.Display();
                }
                if (CFG.Current.ParamEditor_Show_Tool_Row_Names)
                {
                    RowNameTool.Display();
                }
                if (CFG.Current.ParamEditor_Show_Tool_Data_Converter)
                {
                    DataConverterTool.Display();
                }
                if (CFG.Current.ParamEditor_Show_Tool_Data_Comparison)
                {
                    ParamComparisonTool.Display();
                }

                if (CFG.Current.ParamEditor_Show_Tool_Param_Upgrader)
                {
                    ParamUpgrader.Display();
                }

                if (CFG.Current.ParamEditor_Show_Tool_Param_Merger)
                {
                    ParamMerger.Display();
                }

                if (CFG.Current.ParamEditor_Show_Tool_Param_Delta_Patcher)
                {
                    DeltaPatcher.Display();
                }

                DeltaPatcher.ExportProgressModal.Draw();
                DeltaPatcher.ImportProgressModal.Draw();

                DeltaPatcher.ImportPreviewModal.Draw();
                DeltaPatcher.ExportPreviewModal.Draw();

                if (CFG.Current.ParamEditor_Show_Tool_Param_Reloader)
                {
                    ParamReloader.Display();
                }

                if (CFG.Current.ParamEditor_Show_Tool_Item_Gib)
                {
                    ItemGib.Display();
                }

                if (CFG.Current.ParamEditor_Show_Tool_Param_List_Categories)
                {
                    ParamListCategories.Display();
                }

                if (CFG.Current.ParamEditor_Show_Tool_Pin_Groups)
                {
                    PinGroups.Display();
                }

                if (CFG.Current.ParamEditor_Show_Tool_Data_Finders)
                {
                    if (ImGui.CollapsingHeader($"{LOC.Get("PARAM_Tools_Header_Data_Finders")}##dataFindersMenuHeader"))
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

                if (CFG.Current.ParamEditor_Show_Tool_Mass_Edit)
                {
                    if (ImGui.CollapsingHeader($"{LOC.Get("PARAM_Tools_Header_Mass_Edit")}##massEditMenuHeader"))
                    {
                        ImGui.BeginChild("MassEditToolSection", ImGuiChildFlags.Borders);

                        activeView.MassEdit.ToolMenu.Display();

                        ImGui.EndChild();
                    }
                }
            }

        }

        ImGui.End();
    }
}
