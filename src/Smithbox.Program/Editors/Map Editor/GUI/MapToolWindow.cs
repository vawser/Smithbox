using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Renderer;

namespace StudioCore.Editors.MapEditor;

public class MapToolWindow
{
    private MapEditorScreen Editor;
    private ProjectEntry Project;

    public CommonActionTool CommonActionTool;
    public SelectActionTool SelectActionTool;
    public VisibilityActionTool VisibilityActionTool;
    public MapDataTransferTool DataTransferTool;

    public MapToolWindow(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        CommonActionTool = new(editor, project);
        SelectActionTool = new(editor, project);
        VisibilityActionTool = new(editor, project);
        DataTransferTool = new(editor, project);
    }

    public void DisplayDropdown()
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (activeView == null)
            return;

        if (ImGui.BeginMenu("Tools"))
        {
            DataTransferTool.DisplayDropdown();

            if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR)
            {
                if (ImGui.MenuItem("World Map"))
                {
                    activeView.WorldMapTool.DisplayMenuOption();
                }
                UIHelper.Tooltip($"Open a world map with a visual representation of the map tiles.\nShortcut: {InputManager.GetHint(KeybindID.MapEditor_Toggle_World_Map_Menu)}");
            }

            if (ImGui.BeginMenu("Miscellaneous"))
            {
                activeView.EditorVisibilityAction.OnToolMenu();

                ///--------------------
                /// Generate Navigation Data
                ///--------------------
                if (Editor.Project.Descriptor.ProjectType is ProjectType.DES || Editor.Project.Descriptor.ProjectType is ProjectType.DS1 || Editor.Project.Descriptor.ProjectType is ProjectType.DS1R)
                {
                    if (ImGui.BeginMenu("Navigation Data"))
                    {
                        if (ImGui.MenuItem("Generate"))
                        {
                            activeView.ActionHandler.GenerateNavigationData();
                        }

                        ImGui.EndMenu();
                    }
                }

                ///--------------------
                /// Entity ID Checker
                ///--------------------
                if (Editor.Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.AC6)
                {
                    activeView.EntityIdCheckAction.OnToolMenu();
                }

                ///--------------------
                /// Name Map Objects
                ///--------------------
                // Tool for AC6 since its maps come with unnamed Regions and Events
                if (Editor.Project.Descriptor.ProjectType is ProjectType.AC6)
                {
                    activeView.EntityRenameAction.OnToolMenu();
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }

    public void Display()
    {
        if (!CFG.Current.Interface_MapEditor_ToolWindow)
            return;

        var activeView = Editor.ViewHandler.ActiveView;

        if (activeView == null)
            return;

        activeView.DuplicateToMapAction.OnGui();
        activeView.MoveToMapAction.OnGui();
        activeView.SelectAllAction.OnGui();
        activeView.AdjustToGridAction.OnGui();

        if (ImGui.Begin("Tools##ToolConfigureWindow_MapEditor", UIHelper.GetMainWindowFlags()))
        {
            FocusManager.SetFocus(EditorFocusContext.MapEditor_Tools);

            if (ImGui.BeginMenuBar())
            {
                ViewMenu();

                ImGui.EndMenuBar();
            }

            if(CFG.Current.Interface_MapEditor_Tool_Common_Action)
            {
                if (ImGui.CollapsingHeader("Common Actions"))
                {
                    CommonActionTool.Display();
                }
            }

            if (CFG.Current.Interface_MapEditor_Tool_Select_Action)
            {
                if (ImGui.CollapsingHeader("Selection"))
                {
                    SelectActionTool.Display();
                }
            }

            if (CFG.Current.Interface_MapEditor_Tool_Visbility_Action)
            {
                if (ImGui.CollapsingHeader("Visibility"))
                {
                    VisibilityActionTool.Display();
                }
            }

            if (CFG.Current.Interface_MapEditor_Tool_Search)
            {
                if (ImGui.CollapsingHeader("Search"))
                {
                    ImGui.BeginChild("searchSection", ImGuiChildFlags.Borders);

                    ImGui.BeginTabBar("searchTabs");

                    if (ImGui.BeginTabItem("Local##localSearch"))
                    {
                        activeView.LocalSearchView.OnToolWindow();

                        ImGui.EndTabItem();
                    }

                    if (ImGui.BeginTabItem("Global##globalSearch"))
                    {
                        activeView.GlobalSearchTool.OnToolWindow();

                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();

                    ImGui.EndChild();
                }
            }

            if (CFG.Current.Interface_MapEditor_Tool_PropertyMassEdit)
            {
                if (ImGui.CollapsingHeader("Mass Edit"))
                {
                    activeView.MassEditTool.OnToolWindow();
                }
            }

            if (CFG.Current.Interface_MapEditor_Tool_DisplayGroups)
            {
                if (ImGui.CollapsingHeader("Render Groups"))
                {
                    activeView.DisplayGroupTool.OnToolWindow();
                }
            }

            if (CFG.Current.Interface_MapEditor_Tool_Prefab)
            {
                if (ImGui.CollapsingHeader("Prefabs"))
                {
                    activeView.PrefabTool.OnToolWindow();
                }
            }

            //if (CFG.Current.Interface_MapEditor_Tool_Data_Transfer)
            //{
            //    if (ImGui.CollapsingHeader("Data Transfer"))
            //    {
            //        DataTransferTool.Display();
            //    }
            //}

            if (CFG.Current.Interface_MapEditor_Tool_GridConfiguration)
            {
                if (ImGui.CollapsingHeader("Map Grid"))
                {
                    activeView.MapGridTool.OnToolWindow();
                }
            }

            if (CFG.Current.Interface_MapEditor_Tool_ModelSelector)
            {
                if (ImGui.CollapsingHeader("Model Selector"))
                {
                    activeView.ModelSelectorTool.OnToolWindow();
                }
            }

            if (CFG.Current.Interface_MapEditor_Tool_Validation)
            {
                if (ImGui.CollapsingHeader("Validation"))
                {
                    ImGui.BeginChild("validationSection", ImGuiChildFlags.Borders);

                    ImGui.BeginTabBar("validationTabs");

                    if (!(activeView.Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S))
                    {
                        if (ImGui.BeginTabItem("Entity ID##entityIdSearch"))
                        {
                            activeView.EntityIdentifierTool.OnToolWindow();

                            ImGui.EndTabItem();
                        }
                    }

                    if (ImGui.BeginTabItem("MSB Validation##msbValidation"))
                    {
                        activeView.MapValidatorTool.OnToolWindow();

                        ImGui.EndTabItem();
                    }

                    ImGui.EndTabBar();

                    ImGui.EndChild();
                }
            }

            if (CFG.Current.Interface_MapEditor_ResourceList)
            {
                activeView.ResourceListTool.Display("mapEditor", activeView.Universe);
            }

            // TODO: Rendering tab is we ever overhaul the rendering
        }

        ImGui.End();
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Common Actions"))
            {
                CFG.Current.Interface_MapEditor_Tool_Common_Action = !CFG.Current.Interface_MapEditor_Tool_Common_Action;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_Common_Action);

            if (ImGui.MenuItem("Viewport"))
            {
                CFG.Current.Interface_MapEditor_Tool_Viewport = !CFG.Current.Interface_MapEditor_Tool_Viewport;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_Viewport);

            if (ImGui.MenuItem("Data Transfer"))
            {
                CFG.Current.Interface_MapEditor_Tool_Data_Transfer = !CFG.Current.Interface_MapEditor_Tool_Data_Transfer;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_Data_Transfer);

            if (ImGui.MenuItem("Prefabs"))
            {
                CFG.Current.Interface_MapEditor_Tool_Prefab = !CFG.Current.Interface_MapEditor_Tool_Prefab;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_Prefab);

            if (ImGui.MenuItem("Selection Groups"))
            {
                CFG.Current.Interface_MapEditor_Tool_SelectionGroups = !CFG.Current.Interface_MapEditor_Tool_SelectionGroups;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_SelectionGroups);

            if (ImGui.MenuItem("Search"))
            {
                CFG.Current.Interface_MapEditor_Tool_Search = !CFG.Current.Interface_MapEditor_Tool_Search;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_Search);

            if (ImGui.MenuItem("Mass Edit"))
            {
                CFG.Current.Interface_MapEditor_Tool_PropertyMassEdit = !CFG.Current.Interface_MapEditor_Tool_PropertyMassEdit;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_PropertyMassEdit);

            if (ImGui.MenuItem("Map Grid"))
            {
                CFG.Current.Interface_MapEditor_Tool_GridConfiguration = !CFG.Current.Interface_MapEditor_Tool_GridConfiguration;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_GridConfiguration);

            if (ImGui.MenuItem("Model Selector"))
            {
                CFG.Current.Interface_MapEditor_Tool_ModelSelector = !CFG.Current.Interface_MapEditor_Tool_ModelSelector;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_ModelSelector);

            if (ImGui.MenuItem("Display Groups"))
            {
                CFG.Current.Interface_MapEditor_Tool_DisplayGroups = !CFG.Current.Interface_MapEditor_Tool_DisplayGroups;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_DisplayGroups);

            ImGui.EndMenu();
        }
    }
}
