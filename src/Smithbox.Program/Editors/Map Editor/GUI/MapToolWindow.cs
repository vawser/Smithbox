using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.MapEditor;

public class MapToolWindow
{
    private MapEditorScreen Editor;
    private ProjectEntry Project;

    public MapToolWindow(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    public void DisplayMenu()
    {
        var activeView = Editor.ViewHandler.ActiveView;

        if (activeView == null)
            return;

        if (ImGui.BeginMenu("Tools"))
        {
            ///--------------------
            /// Color Picker
            ///--------------------
            if (ImGui.MenuItem("Color Picker"))
            {
                ColorPicker.ShowColorPicker = !ColorPicker.ShowColorPicker;
            }

            ImGui.Separator();

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
    }

    public void OnGui()
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

            var windowHeight = ImGui.GetWindowHeight();
            var windowWidth = ImGui.GetWindowWidth();

            if (ImGui.BeginMenuBar())
            {
                ViewMenu();

                ImGui.EndMenuBar();
            }

            if (CFG.Current.Interface_MapEditor_Tool_Create)
            {
                activeView.CreateAction.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_Duplicate)
            {
                activeView.DuplicateAction.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_DuplicateToMap)
            {
                activeView.DuplicateToMapAction.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_PullToCamera)
            {
                activeView.PullToCameraAction.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_Rotate)
            {
                activeView.RotateAction.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_Scramble)
            {
                activeView.ScrambleAction.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_Replicate)
            {
                activeView.ReplicateAction.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_Prefab)
            {
                activeView.PrefabTool.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_SelectionGroups)
            {
                activeView.SelectionGroupTool.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_MovementIncrements)
            {
                activeView.PositionIncrementTool.OnToolWindow();   
            }

            if (CFG.Current.Interface_MapEditor_Tool_RotationIncrements)
            {
                activeView.RotationIncrementTool.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_LocalPropertySearch)
            {
                activeView.LocalSearchView.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_GlobalPropertySearch)
            {
                activeView.GlobalSearchTool.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_PropertyMassEdit)
            {
                activeView.MassEditTool.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_GridConfiguration)
            {
                activeView.MapGridTool.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_ModelSelector)
            {
                activeView.ModelSelectorTool.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_DisplayGroups)
            {
                activeView.DisplayGroupTool.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_EntityIdentifier)
            {
                activeView.EntityIdentifierTool.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_MapValidator)
            {
                activeView.MapValidatorTool.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_MapModelInsight)
            {
                activeView.MapModelInsightTool.OnToolWindow();
            }

#if DEBUG
            if (FeatureFlags.EnableNavmeshBuilder)
            {
                activeView.NavmeshBuilderTool.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_WorldMapLayoutGenerator)
            {
                activeView.WorldMapLayoutTool.OnToolWindow();
            }
#endif
        }

        ImGui.End();
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu("View"))
        {
            if (ImGui.MenuItem("Create"))
            {
                CFG.Current.Interface_MapEditor_Tool_Create = !CFG.Current.Interface_MapEditor_Tool_Create;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_Create);

            if (ImGui.MenuItem("Duplicate"))
            {
                CFG.Current.Interface_MapEditor_Tool_Duplicate = !CFG.Current.Interface_MapEditor_Tool_Duplicate;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_Duplicate);

            if (ImGui.MenuItem("Duplicate to Map"))
            {
                CFG.Current.Interface_MapEditor_Tool_DuplicateToMap = !CFG.Current.Interface_MapEditor_Tool_DuplicateToMap;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_DuplicateToMap);

            if (ImGui.MenuItem("Move to Camera"))
            {
                CFG.Current.Interface_MapEditor_Tool_PullToCamera = !CFG.Current.Interface_MapEditor_Tool_PullToCamera;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_PullToCamera);

            if (ImGui.MenuItem("Rotate"))
            {
                CFG.Current.Interface_MapEditor_Tool_Rotate = !CFG.Current.Interface_MapEditor_Tool_Rotate;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_Rotate);

            if (ImGui.MenuItem("Scramble"))
            {
                CFG.Current.Interface_MapEditor_Tool_Scramble = !CFG.Current.Interface_MapEditor_Tool_Scramble;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_Scramble);

            if (ImGui.MenuItem("Replicate"))
            {
                CFG.Current.Interface_MapEditor_Tool_Replicate = !CFG.Current.Interface_MapEditor_Tool_Replicate;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_Replicate);

            if (ImGui.MenuItem("Prefab"))
            {
                CFG.Current.Interface_MapEditor_Tool_Prefab = !CFG.Current.Interface_MapEditor_Tool_Prefab;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_Prefab);

            if (ImGui.MenuItem("Selection Groups"))
            {
                CFG.Current.Interface_MapEditor_Tool_SelectionGroups = !CFG.Current.Interface_MapEditor_Tool_SelectionGroups;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_SelectionGroups);

            if (ImGui.MenuItem("Position Increments"))
            {
                CFG.Current.Interface_MapEditor_Tool_MovementIncrements = !CFG.Current.Interface_MapEditor_Tool_MovementIncrements;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_MovementIncrements);

            if (ImGui.MenuItem("Rotation Increments"))
            {
                CFG.Current.Interface_MapEditor_Tool_RotationIncrements = !CFG.Current.Interface_MapEditor_Tool_RotationIncrements;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_RotationIncrements);

            if (ImGui.MenuItem("Local Property Search"))
            {
                CFG.Current.Interface_MapEditor_Tool_LocalPropertySearch = !CFG.Current.Interface_MapEditor_Tool_LocalPropertySearch;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_LocalPropertySearch);

            if (ImGui.MenuItem("Global Property Search"))
            {
                CFG.Current.Interface_MapEditor_Tool_GlobalPropertySearch = !CFG.Current.Interface_MapEditor_Tool_GlobalPropertySearch;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_GlobalPropertySearch);

            if (ImGui.MenuItem("Property Mass Edit"))
            {
                CFG.Current.Interface_MapEditor_Tool_PropertyMassEdit = !CFG.Current.Interface_MapEditor_Tool_PropertyMassEdit;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_PropertyMassEdit);

            if (ImGui.MenuItem("Map Grid Configuration"))
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

            if (ImGui.MenuItem("Entity Identifiers"))
            {
                CFG.Current.Interface_MapEditor_Tool_EntityIdentifier = !CFG.Current.Interface_MapEditor_Tool_EntityIdentifier;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_EntityIdentifier);

            if (ImGui.MenuItem("Map Validator"))
            {
                CFG.Current.Interface_MapEditor_Tool_MapValidator = !CFG.Current.Interface_MapEditor_Tool_MapValidator;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_MapValidator);

            if (ImGui.MenuItem("Map Model Insight"))
            {
                CFG.Current.Interface_MapEditor_Tool_MapModelInsight = !CFG.Current.Interface_MapEditor_Tool_MapModelInsight;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_MapModelInsight);

#if DEBUG
            if (ImGui.MenuItem("Treasure Maker"))
            {
                CFG.Current.Interface_MapEditor_Tool_TreasureMaker = !CFG.Current.Interface_MapEditor_Tool_TreasureMaker;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_TreasureMaker);

            if (ImGui.MenuItem("World Map Layout Generator"))
            {
                CFG.Current.Interface_MapEditor_Tool_WorldMapLayoutGenerator = !CFG.Current.Interface_MapEditor_Tool_WorldMapLayoutGenerator;
            }
            UIHelper.ShowActiveStatus(CFG.Current.Interface_MapEditor_Tool_WorldMapLayoutGenerator);
#endif
            ImGui.EndMenu();
        }
    }
}
