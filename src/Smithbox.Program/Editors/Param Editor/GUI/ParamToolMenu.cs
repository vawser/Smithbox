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

        if (ImGui.BeginMenu("Tools"))
        {
            SortTool.DisplayDropDown();

            if (ImGui.BeginMenu("Data Comparison"))
            {
                ParamComparisonTool.DisplayDropdown();

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Data Transfer"))
            {
                DataConverterTool.DisplayDropdown();

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Row Names"))
            {
                RowNameTool.DisplayDropdown();

                ImGui.EndMenu();
            }

            if (ImGui.BeginMenu("Memory"))
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

        if (ImGui.Begin("Tools##toolWindow_ParamEditor", UIHelper.GetMainWindowFlags()))
        {
            FocusManager.SetFocus(EditorFocusContext.ParamEditor_Tools);

            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("View"))
                {
                    if (ImGui.MenuItem("Sort Rows"))
                    {
                        CFG.Current.ParamEditor_Show_Tool_Sort_Rows = !CFG.Current.ParamEditor_Show_Tool_Sort_Rows;
                    }
                    UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_Show_Tool_Sort_Rows);

                    if (ImGui.MenuItem("Row Names"))
                    {
                        CFG.Current.ParamEditor_Show_Tool_Row_Names = !CFG.Current.ParamEditor_Show_Tool_Row_Names;
                    }
                    UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_Show_Tool_Row_Names);

                    if (ImGui.MenuItem("Data Transfer"))
                    {
                        CFG.Current.ParamEditor_Show_Tool_Data_Converter = !CFG.Current.ParamEditor_Show_Tool_Data_Converter;
                    }
                    UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_Show_Tool_Data_Converter);

                    if (ImGui.MenuItem("Data Comparison"))
                    {
                        CFG.Current.ParamEditor_Show_Tool_Data_Comparison = !CFG.Current.ParamEditor_Show_Tool_Data_Comparison;
                    }
                    UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_Show_Tool_Data_Comparison);

                    if (ImGui.MenuItem("Data Finders"))
                    {
                        CFG.Current.ParamEditor_Show_Tool_Data_Finders = !CFG.Current.ParamEditor_Show_Tool_Data_Finders;
                    }
                    UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_Show_Tool_Data_Finders);

                    if (ImGui.MenuItem("Param Upgrader"))
                    {
                        CFG.Current.ParamEditor_Show_Tool_Param_Upgrader = !CFG.Current.ParamEditor_Show_Tool_Param_Upgrader;
                    }
                    UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_Show_Tool_Param_Upgrader);

                    //if (ImGui.MenuItem("Param Merger"))
                    //{
                    //    CFG.Current.ParamEditor_Show_Tool_Param_Merger = !CFG.Current.ParamEditor_Show_Tool_Param_Merger;
                    //}
                    //UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_Show_Tool_Param_Merger);

                    if (ImGui.MenuItem("Param Delta Patcher"))
                    {
                        CFG.Current.ParamEditor_Show_Tool_Param_Delta_Patcher = !CFG.Current.ParamEditor_Show_Tool_Param_Delta_Patcher;
                    }
                    UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_Show_Tool_Param_Delta_Patcher);

                    if (ImGui.MenuItem("Param Reloader"))
                    {
                        CFG.Current.ParamEditor_Show_Tool_Param_Reloader = !CFG.Current.ParamEditor_Show_Tool_Param_Reloader;
                    }
                    UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_Show_Tool_Param_Reloader);

                    if (ImGui.MenuItem("Item Gib"))
                    {
                        CFG.Current.ParamEditor_Show_Tool_Item_Gib = !CFG.Current.ParamEditor_Show_Tool_Item_Gib;
                    }
                    UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_Show_Tool_Item_Gib);

                    if (ImGui.MenuItem("Mass Edit"))
                    {
                        CFG.Current.ParamEditor_Show_Tool_Mass_Edit = !CFG.Current.ParamEditor_Show_Tool_Mass_Edit;
                    }
                    UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_Show_Tool_Mass_Edit);

                    if (ImGui.MenuItem("Param List Categories"))
                    {
                        CFG.Current.ParamEditor_Show_Tool_Param_List_Categories = !CFG.Current.ParamEditor_Show_Tool_Param_List_Categories;
                    }
                    UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_Show_Tool_Param_List_Categories);

                    if (ImGui.MenuItem("Pin Groups"))
                    {
                        CFG.Current.ParamEditor_Show_Tool_Pin_Groups = !CFG.Current.ParamEditor_Show_Tool_Pin_Groups;
                    }
                    UIHelper.ShowActiveStatus(CFG.Current.ParamEditor_Show_Tool_Pin_Groups);

                    ImGui.EndMenu();
                }

                ImGui.EndMenuBar();
            }

            FocusManager.SetFocus(EditorFocusContext.ParamEditor_Tools);

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
                    if (ImGui.CollapsingHeader("Data Finders"))
                    {
                        ImGui.BeginChild("DataFinderToolSection", ImGuiChildFlags.Borders);

                        if (ImGui.BeginTabBar("dataFinderTabs"))
                        {

                            if (ImGui.BeginTabItem("Field Names"))
                            {
                                FieldNameFinder.Display();

                                ImGui.EndTabItem();
                            }

                            if (ImGui.BeginTabItem("Field Values"))
                            {
                                FieldValueFinder.Display();

                                ImGui.EndTabItem();
                            }

                            if (ImGui.BeginTabItem("Row Names"))
                            {
                                RowNameFinder.Display();

                                ImGui.EndTabItem();
                            }

                            if (ImGui.BeginTabItem("Row IDs"))
                            {
                                RowIdFinder.Display();

                                ImGui.EndTabItem();
                            }

                            if (ImGui.BeginTabItem("Field Value Sets"))
                            {
                                ValueSetFinder.Display();

                                ImGui.EndTabItem();
                            }

                            if (ImGui.BeginTabItem("Row ID Sets"))
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
                    if (ImGui.CollapsingHeader("Mass Edit"))
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
