using Hexa.NET.ImGui;
using StudioCore.Application;
using System.Numerics;

namespace StudioCore.Editors.MapEditor;

public class ToolWindow
{
    private MapEditorScreen Editor;
    private MapActionHandler Handler;

    public ToolWindow(MapEditorScreen screen, MapActionHandler handler)
    {
        Editor = screen;
        Handler = handler;
    }

    public void OnProjectChanged()
    {

    }

    public void OnGui()
    {
        if (Editor.Project.ProjectType == ProjectType.Undefined)
            return;

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_Default_Text_Color);
        ImGui.SetNextWindowSize(new Vector2(300.0f, 200.0f) * DPI.UIScale(), ImGuiCond.FirstUseEver);

        if (ImGui.Begin("Tool Window##ToolConfigureWindow_MapEditor", ImGuiWindowFlags.MenuBar))
        {
            Editor.FocusManager.SwitchMapEditorContext(MapEditorContext.ToolWindow);

            var windowHeight = ImGui.GetWindowHeight();
            var windowWidth = ImGui.GetWindowWidth();

            if (ImGui.BeginMenuBar())
            {
                ViewMenu();

                ImGui.EndMenuBar();
            }

            if (CFG.Current.Interface_MapEditor_Tool_Create)
            {
                Editor.CreateAction.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_Duplicate)
            {
                Editor.DuplicateAction.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_DuplicateToMap)
            {
                Editor.DuplicateToMapAction.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_PullToCamera)
            {
                Editor.PullToCameraAction.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_Rotate)
            {
                Editor.RotateAction.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_Scramble)
            {
                Editor.ScrambleAction.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_Replicate)
            {
                Editor.ReplicateAction.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_Prefab)
            {
                Editor.PrefabTool.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_SelectionGroups)
            {
                Editor.SelectionGroupTool.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_MovementIncrements)
            {
                Editor.MovementCycleConfigTool.OnToolWindow();   
            }

            if (CFG.Current.Interface_MapEditor_Tool_RotationIncrements)
            {
                Editor.RotationCycleConfigTool.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_LocalPropertySearch)
            {
                Editor.LocalSearchView.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_GlobalPropertySearch)
            {
                Editor.GlobalSearchTool.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_PropertyMassEdit)
            {
                Editor.MassEditTool.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_GridConfiguration)
            {
                Editor.MapGridTool.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_ModelSelector)
            {
                Editor.ModelSelectorTool.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_DisplayGroups)
            {
                Editor.DisplayGroupTool.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_EntityIdentifier)
            {
                Editor.EntityIdentifierTool.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_MapValidator)
            {
                Editor.MapValidatorTool.OnToolWindow();
            }
            

#if DEBUG
            if (FeatureFlags.EnableNavmeshBuilder)
            {
                Editor.NavmeshBuilderTool.OnToolWindow();
            }

            if (CFG.Current.Interface_MapEditor_Tool_WorldMapLayoutGenerator)
            {
                Editor.WorldMapLayoutTool.OnToolWindow();
            }
#endif
        }

        ImGui.End();
        ImGui.PopStyleColor(1);
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

            if (ImGui.MenuItem("Movement Increments"))
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
