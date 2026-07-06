using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Keybinds;
using StudioCore.Renderer;

namespace StudioCore.Editors.MapEditor;

public class MapToolWindow
{
    private MapEditorView View;
    private ProjectEntry Project;

    public CommonActionTool CommonActionTool;
    public SelectActionTool SelectActionTool;
    public VisibilityActionTool VisibilityActionTool;
    public MapDataTransferTool DataTransferTool;

    public MapToolWindow(MapEditorView view, ProjectEntry project)
    {
        View = view;
        Project = project;

        CommonActionTool = new(view, project);
        SelectActionTool = new(view, project);
        VisibilityActionTool = new(view, project);
        DataTransferTool = new(view, project);
    }

    public void DisplayDropdown()
    {
        if (ImGui.BeginMenu("Tools"))
        {
            DataTransferTool.DisplayDropdown();

            if (Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR)
            {
                if (ImGui.MenuItem("World Map"))
                {
                    View.WorldMapTool.DisplayMenuOption();
                }
                UIHelper.Tooltip($"Open a world map with a visual representation of the map tiles.\nShortcut: {InputManager.GetHint(KeybindID.MapEditor_Toggle_World_Map_Menu)}");
            }

            if (ImGui.BeginMenu("Miscellaneous"))
            {
                View.EditorVisibilityAction.OnToolMenu();

                ///--------------------
                /// Generate Navigation Data
                ///--------------------
                if (View.Editor.Project.Descriptor.ProjectType is ProjectType.DES || View.Editor.Project.Descriptor.ProjectType is ProjectType.DS1 || View.Editor.Project.Descriptor.ProjectType is ProjectType.DS1R)
                {
                    if (ImGui.BeginMenu("Navigation Data"))
                    {
                        if (ImGui.MenuItem("Generate"))
                        {
                            View.ActionHandler.GenerateNavigationData();
                        }

                        ImGui.EndMenu();
                    }
                }

                ///--------------------
                /// Entity ID Checker
                ///--------------------
                if (View.Editor.Project.Descriptor.ProjectType is ProjectType.DS3 or ProjectType.SDT or ProjectType.ER or ProjectType.AC6)
                {
                    View.EntityIdCheckAction.OnToolMenu();
                }

                ///--------------------
                /// Name Map Objects
                ///--------------------
                // Tool for AC6 since its maps come with unnamed Regions and Events
                if (View.Editor.Project.Descriptor.ProjectType is ProjectType.AC6)
                {
                    View.EntityRenameAction.OnToolMenu();
                }

                ImGui.EndMenu();
            }

            ImGui.EndMenu();
        }
    }

    public void Display()
    {
        View.DuplicateToMapAction.OnGui();
        View.MoveToMapAction.OnGui();
        View.SelectAllAction.OnGui();
        View.AdjustToGridAction.OnGui();

        if (!CFG.Current.Interface_MapEditor_ToolWindow)
            return;

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
                    View.LocalSearchView.OnToolWindow();

                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Global##globalSearch"))
                {
                    View.GlobalSearchTool.OnToolWindow();

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
                View.MassEditTool.OnToolWindow();
            }
        }

        if (CFG.Current.Interface_MapEditor_Tool_DisplayGroups)
        {
            if (ImGui.CollapsingHeader("Render Groups"))
            {
                View.DisplayGroupTool.OnToolWindow();
            }
        }

        if (CFG.Current.Interface_MapEditor_Tool_Prefab)
        {
            if (ImGui.CollapsingHeader("Prefabs"))
            {
                View.PrefabTool.OnToolWindow();
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
                View.MapGridTool.OnToolWindow();
            }
        }

        if (CFG.Current.Interface_MapEditor_Tool_ModelSelector)
        {
            if (ImGui.CollapsingHeader("Model Selector"))
            {
                View.ModelSelectorTool.OnToolWindow();
            }
        }

        if (CFG.Current.Interface_MapEditor_Tool_Validation)
        {
            if (ImGui.CollapsingHeader("Validation"))
            {
                ImGui.BeginChild("validationSection", ImGuiChildFlags.Borders);

                ImGui.BeginTabBar("validationTabs");

                if (!(View.Project.Descriptor.ProjectType is ProjectType.DS2 or ProjectType.DS2S))
                {
                    if (ImGui.BeginTabItem("Entity ID##entityIdSearch"))
                    {
                        View.EntityIdentifierTool.OnToolWindow();

                        ImGui.EndTabItem();
                    }
                }

                if (ImGui.BeginTabItem("MSB Validation##msbValidation"))
                {
                    View.MapValidatorTool.OnToolWindow();

                    ImGui.EndTabItem();
                }

                ImGui.EndTabBar();

                ImGui.EndChild();
            }
        }

        if (CFG.Current.Interface_MapEditor_ResourceList)
        {
            View.ResourceListTool.Display("mapEditor", View.Universe);
        }

        //if (CFG.Current.Interface_MapEditor_Tool_AssetBrowser)
        //{
        //    if (ImGui.CollapsingHeader("Asset Browser"))
        //    {
        //        activeView.AssetBrowser.Display();
        //    }
        //}
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

            if (ImGui.MenuItem("Display Groups"))
            {
                CFG.Current.Interface_MapEditor_Tool_AssetBrowser = !CFG.Current.Interface_MapEditor_Tool_AssetBrowser;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_AssetBrowser);

            ImGui.EndMenu();
        }
    }
}
