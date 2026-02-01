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
    public ParamComparisonTools ParamComparisonTools;
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

        ParamComparisonTools = new(editor, project);
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
            if (ImGui.MenuItem("Sort Rows"))
            {
                if (activeView.Selection.ActiveParamExists())
                {
                    ParamRowTools.SortRows(activeView);
                }
            }
            UIHelper.Tooltip("This will sort the rows by ID. WARNING: this is not recommended as row index can be important.");

            ImGui.EndMenu();
        }
    }

    public void Draw()
    {
        if (!CFG.Current.Interface_ParamEditor_ToolWindow)
            return;

        if (ImGui.Begin("Tools##toolWindow_ParamEditor", UIHelper.GetMainWindowFlags()))
        {
            if (ImGui.BeginMenuBar())
            {
                if (ImGui.BeginMenu("View"))
                {
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
                if (CFG.Current.ParamEditor_Show_Tool_Param_Upgrader)
                {
                    ParamUpgrader.Display();
                }

                //if (CFG.Current.ParamEditor_Show_Tool_Param_Merger)
                //{
                //    ParamMerger.Display();
                //}

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
                    }
                }

                if (CFG.Current.ParamEditor_Show_Tool_Mass_Edit)
                {
                    if (ImGui.CollapsingHeader("Mass Edit"))
                    {
                        if (ImGui.BeginTabBar("massEditTabs"))
                        {

                            if (ImGui.BeginTabItem("Command Palette"))
                            {
                                activeView.MassEdit.DisplayMassEditMenu();

                                ImGui.EndTabItem();
                            }


                            if (ImGui.BeginTabItem("Templates"))
                            {
                                activeView.MassEdit.TemplateMenu.DisplayMenu();

                                ImGui.EndTabItem();
                            }

                            ImGui.EndTabBar();
                        }
                    }
                }
            }

        }

        ImGui.End();
    }
}
