using ImGuiNET;
using Octokit;
using StudioCore.Configuration;
using StudioCore.Interface.Settings;
using StudioCore.Interface.Tabs;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Veldrid;
using static StudioCore.Interface.Windows.SettingsWindow;

namespace StudioCore.Interface.Windows;
public class KeybindWindow
{
    private KeyBind _currentKeyBind;
    public bool MenuOpenState;

    private CommonKeybindTab CommonKeybinds;
    private ViewportKeybindTab ViewportKeybinds;
    private MapEditorKeybindTab MapEditorKeybinds;
    private ModelEditorKeybindTab ModelEditorKeybinds;
    private ParamEditorKeybindTab ParamEditorKeybinds;
    private TextEditorKeybindTab TextEditorKeybinds;
    private GparamEditorKeybindTab GparamEditorKeybinds;
    private TimeActEditorKeybindTab TimeActEditorKeybinds;
    private TextureViewerKeybindTab TextureViewerKeybinds;

    public KeybindWindow()
    {
        CommonKeybinds = new CommonKeybindTab();
        ViewportKeybinds = new ViewportKeybindTab();
        MapEditorKeybinds = new MapEditorKeybindTab();
        ModelEditorKeybinds = new ModelEditorKeybindTab();
        ParamEditorKeybinds = new ParamEditorKeybindTab();
        TextEditorKeybinds = new TextEditorKeybindTab();
        GparamEditorKeybinds = new GparamEditorKeybindTab();
        TimeActEditorKeybinds = new TimeActEditorKeybindTab();
        TextureViewerKeybinds = new TextureViewerKeybindTab();
    }

    public void SaveSettings()
    {
        CFG.Save();
    }

    public void ToggleMenuVisibility()
    {
        MenuOpenState = !MenuOpenState;
    }

    public void Display()
    {
        var scale = Smithbox.GetUIScale();
        if (!MenuOpenState)
            return;

        ImGui.SetNextWindowSize(new Vector2(900.0f, 800.0f) * scale, ImGuiCond.FirstUseEver);
        ImGui.PushStyleColor(ImGuiCol.WindowBg, CFG.Current.Imgui_Moveable_MainBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, CFG.Current.Imgui_Moveable_TitleBg);
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, CFG.Current.Imgui_Moveable_TitleBg_Active);
        ImGui.PushStyleColor(ImGuiCol.ChildBg, CFG.Current.Imgui_Moveable_ChildBg);
        ImGui.PushStyleColor(ImGuiCol.Text, CFG.Current.ImGui_Default_Text_Color);
        ImGui.PushStyleVar(ImGuiStyleVar.WindowPadding, new Vector2(10.0f, 10.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.ItemSpacing, new Vector2(5.0f, 5.0f) * scale);
        ImGui.PushStyleVar(ImGuiStyleVar.IndentSpacing, 20.0f * scale);

        if (ImGui.Begin("Keybinds##KeybindWindow", ref MenuOpenState, ImGuiWindowFlags.NoDocking))
        {
            ImGui.Columns(2);

            ImGui.BeginChild("keybindTabList");

            var arr = Enum.GetValues(typeof(SelectedKeybindTab));
            for (int i = 0; i < arr.Length; i++)
            {
                var tab = (SelectedKeybindTab)arr.GetValue(i);

                if (ImGui.Selectable(tab.GetDisplayName(), tab == SelectedTab))
                {
                    SelectedTab = tab;
                }
            }
            ImGui.EndChild();

            ImGui.NextColumn();

            ImGui.BeginChild("keybindTab");
            switch (SelectedTab)
            {
                case SelectedKeybindTab.Common:
                    CommonKeybinds.Display();
                    break;
                case SelectedKeybindTab.Viewport:
                    ViewportKeybinds.Display();
                    break;
                case SelectedKeybindTab.MapEditor:
                    MapEditorKeybinds.Display();
                    break;
                case SelectedKeybindTab.ModelEditor:
                    ModelEditorKeybinds.Display();
                    break;
                case SelectedKeybindTab.ParamEditor:
                    ParamEditorKeybinds.Display();
                    break;
                case SelectedKeybindTab.TextEditor:
                    TextEditorKeybinds.Display();
                    break;
                case SelectedKeybindTab.GparamEditor:
                    GparamEditorKeybinds.Display();
                    break;
                case SelectedKeybindTab.TimeActEditor:
                    TimeActEditorKeybinds.Display();
                    break;
                case SelectedKeybindTab.TextureViewer:
                    TextureViewerKeybinds.Display();
                    break;
            }
            ImGui.EndChild();

            ImGui.Columns(1);
        }

        ImGui.End();

        ImGui.PopStyleVar(3);
        ImGui.PopStyleColor(5);
    }

    private SelectedKeybindTab SelectedTab = SelectedKeybindTab.Common;

    public enum SelectedKeybindTab
    {
        [Display(Name = "Common")] Common,
        [Display(Name = "Viewport")] Viewport,
        [Display(Name = "Map Editor")] MapEditor,
        [Display(Name = "Model Editor")] ModelEditor,
        [Display(Name = "Param Editor")] ParamEditor,
        [Display(Name = "Text Editor")] TextEditor,
        [Display(Name = "GPARAM Editor")] GparamEditor,
        [Display(Name = "Time Act Editor")] TimeActEditor,
        [Display(Name = "Texture Viewer")] TextureViewer
    }
}

public class CommonKeybindTab
{
    public CommonKeybindTab() { }

    public void Display()
    {
        ImGui.Separator();
        ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "Keybinds");
        ImGui.Separator();

        if (ImGui.CollapsingHeader("Core", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.CORE_CreateNewEntry = InputTracker.KeybindLine(0,
                KeyBindings.Current.CORE_CreateNewEntry,
                KeyBindings.Default.CORE_CreateNewEntry);

            KeyBindings.Current.CORE_DeleteSelectedEntry = InputTracker.KeybindLine(1,
                KeyBindings.Current.CORE_DeleteSelectedEntry,
                KeyBindings.Default.CORE_DeleteSelectedEntry);

            KeyBindings.Current.CORE_DuplicateSelectedEntry = InputTracker.KeybindLine(2,
                KeyBindings.Current.CORE_DuplicateSelectedEntry,
                KeyBindings.Default.CORE_DuplicateSelectedEntry);

            KeyBindings.Current.CORE_RedoAction = InputTracker.KeybindLine(3,
                KeyBindings.Current.CORE_RedoAction,
                KeyBindings.Default.CORE_RedoAction);

            KeyBindings.Current.CORE_UndoAction = InputTracker.KeybindLine(4,
                KeyBindings.Current.CORE_UndoAction,
                KeyBindings.Default.CORE_UndoAction);

            KeyBindings.Current.CORE_SaveAll = InputTracker.KeybindLine(5,
                KeyBindings.Current.CORE_SaveAll,
                KeyBindings.Default.CORE_SaveAll);

            KeyBindings.Current.CORE_Save = InputTracker.KeybindLine(6,
                KeyBindings.Current.CORE_Save,
                KeyBindings.Default.CORE_Save);
        }

        if (ImGui.CollapsingHeader("Windows", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.CORE_ConfigurationWindow = InputTracker.KeybindLine(7,
                KeyBindings.Current.CORE_ConfigurationWindow,
                KeyBindings.Default.CORE_ConfigurationWindow);

            KeyBindings.Current.CORE_HelpWindow = InputTracker.KeybindLine(8,
                KeyBindings.Current.CORE_HelpWindow,
                KeyBindings.Default.CORE_HelpWindow);

            KeyBindings.Current.CORE_KeybindsWindow = InputTracker.KeybindLine(9,
                KeyBindings.Current.CORE_KeybindsWindow,
                KeyBindings.Default.CORE_KeybindsWindow);
        }
    }
}

public class ViewportKeybindTab
{
    public ViewportKeybindTab() { }

    public void Display()
    {
        ImGui.Separator();
        ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "Keybinds");
        ImGui.Separator();

        if (ImGui.CollapsingHeader("Core", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.VIEWPORT_CameraForward = InputTracker.KeybindLine(0,
                KeyBindings.Current.VIEWPORT_CameraForward,
                KeyBindings.Default.VIEWPORT_CameraForward);

            KeyBindings.Current.VIEWPORT_CameraBack = InputTracker.KeybindLine(1,
                KeyBindings.Current.VIEWPORT_CameraBack,
                KeyBindings.Default.VIEWPORT_CameraBack);

            KeyBindings.Current.VIEWPORT_CameraUp = InputTracker.KeybindLine(2,
                KeyBindings.Current.VIEWPORT_CameraUp,
                KeyBindings.Default.VIEWPORT_CameraUp);

            KeyBindings.Current.VIEWPORT_CameraDown = InputTracker.KeybindLine(3,
                KeyBindings.Current.VIEWPORT_CameraDown,
                KeyBindings.Default.VIEWPORT_CameraDown);

            KeyBindings.Current.VIEWPORT_CameraLeft = InputTracker.KeybindLine(4,
                KeyBindings.Current.VIEWPORT_CameraLeft,
                KeyBindings.Default.VIEWPORT_CameraLeft);

            KeyBindings.Current.VIEWPORT_CameraRight = InputTracker.KeybindLine(5,
                KeyBindings.Current.VIEWPORT_CameraRight,
                KeyBindings.Default.VIEWPORT_CameraRight);

            KeyBindings.Current.VIEWPORT_CameraReset = InputTracker.KeybindLine(6,
                KeyBindings.Current.VIEWPORT_CameraReset,
                KeyBindings.Default.VIEWPORT_CameraReset);
        }

        if (ImGui.CollapsingHeader("Gizmos", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.VIEWPORT_GizmoRotationMode = InputTracker.KeybindLine(7,
                KeyBindings.Current.VIEWPORT_GizmoRotationMode,
                KeyBindings.Default.VIEWPORT_GizmoRotationMode);

            KeyBindings.Current.VIEWPORT_GizmoOriginMode = InputTracker.KeybindLine(8,
                KeyBindings.Current.VIEWPORT_GizmoOriginMode,
                KeyBindings.Default.VIEWPORT_GizmoOriginMode);

            KeyBindings.Current.VIEWPORT_GizmoSpaceMode = InputTracker.KeybindLine(9,
                KeyBindings.Current.VIEWPORT_GizmoSpaceMode,
                KeyBindings.Default.VIEWPORT_GizmoSpaceMode);

            KeyBindings.Current.VIEWPORT_GizmoTranslationMode = InputTracker.KeybindLine(10,
                KeyBindings.Current.VIEWPORT_GizmoTranslationMode,
                KeyBindings.Default.VIEWPORT_GizmoTranslationMode);
        }

        if (ImGui.CollapsingHeader("Grid", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.VIEWPORT_LowerGrid = InputTracker.KeybindLine(11,
                KeyBindings.Current.VIEWPORT_LowerGrid,
                KeyBindings.Default.VIEWPORT_LowerGrid);

            KeyBindings.Current.VIEWPORT_RaiseGrid = InputTracker.KeybindLine(12,
                KeyBindings.Current.VIEWPORT_RaiseGrid,
                KeyBindings.Default.VIEWPORT_RaiseGrid);

            KeyBindings.Current.VIEWPORT_SetGridToSelectionHeight = InputTracker.KeybindLine(13,
                KeyBindings.Current.VIEWPORT_SetGridToSelectionHeight,
                KeyBindings.Default.VIEWPORT_SetGridToSelectionHeight);
        }

        if (ImGui.CollapsingHeader("Selection", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.VIEWPORT_RenderOutline = InputTracker.KeybindLine(14,
                KeyBindings.Current.VIEWPORT_RenderOutline,
                KeyBindings.Default.VIEWPORT_RenderOutline);
        }
    }
}

public class MapEditorKeybindTab
{
    public MapEditorKeybindTab() { }

    public void Display()
    {
        ImGui.Separator();
        ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "Keybinds");
        ImGui.Separator();

        if (ImGui.CollapsingHeader("Core", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.MAP_DuplicateToMap = InputTracker.KeybindLine(0,
                KeyBindings.Current.MAP_DuplicateToMap,
                KeyBindings.Default.MAP_DuplicateToMap);

            KeyBindings.Current.MAP_CreateMapObject = InputTracker.KeybindLine(1,
                KeyBindings.Current.MAP_CreateMapObject,
                KeyBindings.Default.MAP_CreateMapObject);

            KeyBindings.Current.MAP_GoToInList = InputTracker.KeybindLine(2,
                KeyBindings.Current.MAP_GoToInList,
                KeyBindings.Default.MAP_GoToInList);

            KeyBindings.Current.MAP_MoveToCamera = InputTracker.KeybindLine(3,
                KeyBindings.Current.MAP_MoveToCamera,
                KeyBindings.Default.MAP_MoveToCamera);

            KeyBindings.Current.MAP_FrameSelection = InputTracker.KeybindLine(4,
                KeyBindings.Current.MAP_FrameSelection,
                KeyBindings.Default.MAP_FrameSelection);

            KeyBindings.Current.MAP_RotateSelectionXAxis = InputTracker.KeybindLine(5,
                KeyBindings.Current.MAP_RotateSelectionXAxis,
                KeyBindings.Default.MAP_RotateSelectionXAxis);

            KeyBindings.Current.MAP_RotateSelectionYAxis = InputTracker.KeybindLine(6,
                KeyBindings.Current.MAP_RotateSelectionYAxis,
                KeyBindings.Default.MAP_RotateSelectionYAxis);

            KeyBindings.Current.MAP_PivotSelectionYAxis = InputTracker.KeybindLine(7,
                KeyBindings.Current.MAP_PivotSelectionYAxis,
                KeyBindings.Default.MAP_PivotSelectionYAxis);

            KeyBindings.Current.MAP_ResetRotation = InputTracker.KeybindLine(8,
                KeyBindings.Current.MAP_ResetRotation,
                KeyBindings.Default.MAP_ResetRotation);

            KeyBindings.Current.MAP_FlipSelectionVisibility = InputTracker.KeybindLine(9,
                KeyBindings.Current.MAP_FlipSelectionVisibility,
                KeyBindings.Default.MAP_FlipSelectionVisibility);

            KeyBindings.Current.MAP_FlipAllVisibility = InputTracker.KeybindLine(10,
                KeyBindings.Current.MAP_FlipAllVisibility,
                KeyBindings.Default.MAP_FlipAllVisibility);

            KeyBindings.Current.MAP_EnableSelectionVisibility = InputTracker.KeybindLine(11,
                KeyBindings.Current.MAP_EnableSelectionVisibility,
                KeyBindings.Default.MAP_EnableSelectionVisibility);

            KeyBindings.Current.MAP_EnableAllVisibility = InputTracker.KeybindLine(12,
                KeyBindings.Current.MAP_EnableAllVisibility,
                KeyBindings.Default.MAP_EnableAllVisibility);

            KeyBindings.Current.MAP_DisableSelectionVisibility = InputTracker.KeybindLine(13,
                KeyBindings.Current.MAP_DisableSelectionVisibility,
                KeyBindings.Default.MAP_DisableSelectionVisibility);

            KeyBindings.Current.MAP_DisableAllVisibility = InputTracker.KeybindLine(14,
                KeyBindings.Current.MAP_DisableAllVisibility,
                KeyBindings.Default.MAP_DisableAllVisibility);

            KeyBindings.Current.MAP_MakeDummyObject = InputTracker.KeybindLine(15,
                KeyBindings.Current.MAP_MakeDummyObject,
                KeyBindings.Default.MAP_MakeDummyObject);

            KeyBindings.Current.MAP_MakeNormalObject = InputTracker.KeybindLine(16,
                KeyBindings.Current.MAP_MakeNormalObject,
                KeyBindings.Default.MAP_MakeNormalObject);

            KeyBindings.Current.MAP_ScrambleSelection = InputTracker.KeybindLine(17,
                KeyBindings.Current.MAP_ScrambleSelection,
                KeyBindings.Default.MAP_ScrambleSelection);

            KeyBindings.Current.MAP_ReplicateSelection = InputTracker.KeybindLine(18,
                KeyBindings.Current.MAP_ReplicateSelection,
                KeyBindings.Default.MAP_ReplicateSelection);

            KeyBindings.Current.MAP_SetSelectionToGrid = InputTracker.KeybindLine(19,
                KeyBindings.Current.MAP_SetSelectionToGrid,
                KeyBindings.Default.MAP_SetSelectionToGrid);
        }

        if (ImGui.CollapsingHeader("Order", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.MAP_MoveObjectUp = InputTracker.KeybindLine(20,
                KeyBindings.Current.MAP_MoveObjectUp,
                KeyBindings.Default.MAP_MoveObjectUp);

            KeyBindings.Current.MAP_MoveObjectDown = InputTracker.KeybindLine(21,
                KeyBindings.Current.MAP_MoveObjectDown,
                KeyBindings.Default.MAP_MoveObjectDown);

            KeyBindings.Current.MAP_MoveObjectTop = InputTracker.KeybindLine(22,
                KeyBindings.Current.MAP_MoveObjectTop,
                KeyBindings.Default.MAP_MoveObjectTop);

            KeyBindings.Current.MAP_MoveObjectBottom = InputTracker.KeybindLine(23,
                KeyBindings.Current.MAP_MoveObjectBottom,
                KeyBindings.Default.MAP_MoveObjectBottom);
        }

        if (ImGui.CollapsingHeader("Prefabs", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.MAP_ExportPrefab = InputTracker.KeybindLine(24,
                KeyBindings.Current.MAP_ExportPrefab,
                KeyBindings.Default.MAP_ExportPrefab);

            KeyBindings.Current.MAP_ImportPrefab = InputTracker.KeybindLine(25,
                KeyBindings.Current.MAP_ImportPrefab,
                KeyBindings.Default.MAP_ImportPrefab);
        }

        if (ImGui.CollapsingHeader("Render Groups", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.MAP_GetDisplayGroup = InputTracker.KeybindLine(26,
                KeyBindings.Current.MAP_GetDisplayGroup,
                KeyBindings.Default.MAP_GetDisplayGroup);

            KeyBindings.Current.MAP_GetDrawGroup = InputTracker.KeybindLine(27,
                KeyBindings.Current.MAP_GetDrawGroup,
                KeyBindings.Default.MAP_GetDrawGroup);

            KeyBindings.Current.MAP_SetDisplayGroup = InputTracker.KeybindLine(28,
                KeyBindings.Current.MAP_SetDisplayGroup,
                KeyBindings.Default.MAP_SetDisplayGroup);

            KeyBindings.Current.MAP_SetDrawGroup = InputTracker.KeybindLine(29,
                KeyBindings.Current.MAP_SetDrawGroup,
                KeyBindings.Default.MAP_SetDrawGroup);

            KeyBindings.Current.MAP_HideAllDisplayGroups = InputTracker.KeybindLine(30,
                KeyBindings.Current.MAP_HideAllDisplayGroups,
                KeyBindings.Default.MAP_HideAllDisplayGroups);

            KeyBindings.Current.MAP_ShowAllDisplayGroups = InputTracker.KeybindLine(31,
                KeyBindings.Current.MAP_ShowAllDisplayGroups,
                KeyBindings.Default.MAP_ShowAllDisplayGroups);

            KeyBindings.Current.MAP_SelectDisplayGroupHighlights = InputTracker.KeybindLine(32,
                KeyBindings.Current.MAP_SelectDisplayGroupHighlights,
                KeyBindings.Default.MAP_SelectDisplayGroupHighlights);
        }

        if (ImGui.CollapsingHeader("Selection Groups", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.MAP_CreateSelectionGroup = InputTracker.KeybindLine(33,
                KeyBindings.Current.MAP_CreateSelectionGroup,
                KeyBindings.Default.MAP_CreateSelectionGroup);

            KeyBindings.Current.MAP_SelectionGroup_0 = InputTracker.KeybindLine(34,
                KeyBindings.Current.MAP_SelectionGroup_0,
                KeyBindings.Default.MAP_SelectionGroup_0);

            KeyBindings.Current.MAP_SelectionGroup_1 = InputTracker.KeybindLine(35,
                KeyBindings.Current.MAP_SelectionGroup_1,
                KeyBindings.Default.MAP_SelectionGroup_1);

            KeyBindings.Current.MAP_SelectionGroup_2 = InputTracker.KeybindLine(36,
                KeyBindings.Current.MAP_SelectionGroup_2,
                KeyBindings.Default.MAP_SelectionGroup_2);

            KeyBindings.Current.MAP_SelectionGroup_3 = InputTracker.KeybindLine(37,
                KeyBindings.Current.MAP_SelectionGroup_3,
                KeyBindings.Default.MAP_SelectionGroup_3);

            KeyBindings.Current.MAP_SelectionGroup4 = InputTracker.KeybindLine(38,
                KeyBindings.Current.MAP_SelectionGroup4,
                KeyBindings.Default.MAP_SelectionGroup4);

            KeyBindings.Current.MAP_SelectionGroup5 = InputTracker.KeybindLine(39,
                KeyBindings.Current.MAP_SelectionGroup5,
                KeyBindings.Default.MAP_SelectionGroup5);

            KeyBindings.Current.MAP_SelectionGroup6 = InputTracker.KeybindLine(40,
                KeyBindings.Current.MAP_SelectionGroup6,
                KeyBindings.Default.MAP_SelectionGroup6);

            KeyBindings.Current.MAP_SelectionGroup7 = InputTracker.KeybindLine(41,
                KeyBindings.Current.MAP_SelectionGroup7,
                KeyBindings.Default.MAP_SelectionGroup7);

            KeyBindings.Current.MAP_SelectionGroup8 = InputTracker.KeybindLine(42,
                KeyBindings.Current.MAP_SelectionGroup8,
                KeyBindings.Default.MAP_SelectionGroup8);

            KeyBindings.Current.MAP_SelectionGroup9 = InputTracker.KeybindLine(43,
                KeyBindings.Current.MAP_SelectionGroup9,
                KeyBindings.Default.MAP_SelectionGroup9);

            KeyBindings.Current.MAP_SelectionGroup10 = InputTracker.KeybindLine(44,
                KeyBindings.Current.MAP_SelectionGroup10,
                KeyBindings.Default.MAP_SelectionGroup10);
        }

        if (ImGui.CollapsingHeader("Visualisation", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.MAP_TogglePatrolRouteRendering = InputTracker.KeybindLine(45,
                KeyBindings.Current.MAP_TogglePatrolRouteRendering,
                KeyBindings.Default.MAP_TogglePatrolRouteRendering);
        }

        if (ImGui.CollapsingHeader("World Map", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.MAP_ToggleERMapVanilla = InputTracker.KeybindLine(46,
                KeyBindings.Current.MAP_ToggleERMapVanilla,
                KeyBindings.Default.MAP_ToggleERMapVanilla);

            KeyBindings.Current.MAP_ToggleERMapSOTE = InputTracker.KeybindLine(47,
                KeyBindings.Current.MAP_ToggleERMapSOTE,
                KeyBindings.Default.MAP_ToggleERMapSOTE);

            KeyBindings.Current.MAP_DragWorldMap = InputTracker.KeybindLine(48,
                KeyBindings.Current.MAP_DragWorldMap,
                KeyBindings.Default.MAP_DragWorldMap);
        }
    }
}

public class ModelEditorKeybindTab
{
    public ModelEditorKeybindTab() { }

    public void Display()
    {
        ImGui.Separator();
        ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "Keybinds");
        ImGui.Separator();

        if (ImGui.CollapsingHeader("Core", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.MODEL_ToggleVisibility = InputTracker.KeybindLine(0,
                KeyBindings.Current.MODEL_ToggleVisibility,
                KeyBindings.Default.MODEL_ToggleVisibility);

            KeyBindings.Current.MODEL_Multiselect = InputTracker.KeybindLine(1,
                KeyBindings.Current.MODEL_Multiselect,
                KeyBindings.Default.MODEL_Multiselect);

            KeyBindings.Current.MODEL_MultiselectRange = InputTracker.KeybindLine(2,
                KeyBindings.Current.MODEL_MultiselectRange,
                KeyBindings.Default.MODEL_MultiselectRange);
        
            KeyBindings.Current.MODEL_ExportModel = InputTracker.KeybindLine(3,
                KeyBindings.Current.MODEL_ExportModel,
                KeyBindings.Default.MODEL_ExportModel);
        }
    }
}

public class ParamEditorKeybindTab
{
    public ParamEditorKeybindTab() { }

    public void Display()
    {
        ImGui.Separator();
        ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "Keybinds");
        ImGui.Separator();

        if (ImGui.CollapsingHeader("Core", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.PARAM_SelectAll = InputTracker.KeybindLine(0,
                KeyBindings.Current.PARAM_SelectAll,
                KeyBindings.Default.PARAM_SelectAll);

            KeyBindings.Current.PARAM_GoToSelectedRow = InputTracker.KeybindLine(1,
                KeyBindings.Current.PARAM_GoToSelectedRow,
                KeyBindings.Default.PARAM_GoToSelectedRow);

            KeyBindings.Current.PARAM_GoToRowID = InputTracker.KeybindLine(2,
                KeyBindings.Current.PARAM_GoToRowID,
                KeyBindings.Default.PARAM_GoToRowID);

            KeyBindings.Current.PARAM_Sort = InputTracker.KeybindLine(23,
                KeyBindings.Current.PARAM_Sort,
                KeyBindings.Default.PARAM_Sort);

            KeyBindings.Current.PARAM_CopyToClipboard = InputTracker.KeybindLine(3,
                KeyBindings.Current.PARAM_CopyToClipboard,
                KeyBindings.Default.PARAM_CopyToClipboard);

            KeyBindings.Current.PARAM_PasteClipboard = InputTracker.KeybindLine(4,
                KeyBindings.Current.PARAM_PasteClipboard,
                KeyBindings.Default.PARAM_PasteClipboard);

            KeyBindings.Current.PARAM_SearchParam = InputTracker.KeybindLine(6,
                KeyBindings.Current.PARAM_SearchParam,
                KeyBindings.Default.PARAM_SearchParam);

            KeyBindings.Current.PARAM_SearchRow = InputTracker.KeybindLine(7,
                KeyBindings.Current.PARAM_SearchRow,
                KeyBindings.Default.PARAM_SearchRow);

            KeyBindings.Current.PARAM_SearchField = InputTracker.KeybindLine(8,
                KeyBindings.Current.PARAM_SearchField,
                KeyBindings.Default.PARAM_SearchField);
        }

        if (ImGui.CollapsingHeader("Mass Edit", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.PARAM_ViewMassEdit = InputTracker.KeybindLine(5,
                KeyBindings.Current.PARAM_ViewMassEdit,
                KeyBindings.Default.PARAM_ViewMassEdit);

            KeyBindings.Current.PARAM_ExecuteMassEdit = InputTracker.KeybindLine(22,
                KeyBindings.Current.PARAM_ExecuteMassEdit,
                KeyBindings.Default.PARAM_ExecuteMassEdit);
        }

        if (ImGui.CollapsingHeader("CSV", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.PARAM_ImportCSV = InputTracker.KeybindLine(9,
                KeyBindings.Current.PARAM_ImportCSV,
                KeyBindings.Default.PARAM_ImportCSV);

            KeyBindings.Current.PARAM_ExportCSV = InputTracker.KeybindLine(10,
                KeyBindings.Current.PARAM_ExportCSV,
                KeyBindings.Default.PARAM_ExportCSV);
        }

        if (ImGui.CollapsingHeader("Param Reloader", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.PARAM_ReloadParam = InputTracker.KeybindLine(11,
                KeyBindings.Current.PARAM_ReloadParam,
                KeyBindings.Default.PARAM_ReloadParam);

            KeyBindings.Current.PARAM_ReloadAllParams = InputTracker.KeybindLine(12,
                KeyBindings.Current.PARAM_ReloadAllParams,
                KeyBindings.Default.PARAM_ReloadAllParams);
        }

        if (ImGui.CollapsingHeader("Pin Groups", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.PARAM_CreateParamGroup = InputTracker.KeybindLine(13,
                KeyBindings.Current.PARAM_CreateParamGroup,
                KeyBindings.Default.PARAM_CreateParamGroup);

            KeyBindings.Current.PARAM_CreateRowGroup = InputTracker.KeybindLine(14,
                KeyBindings.Current.PARAM_CreateRowGroup,
                KeyBindings.Default.PARAM_CreateRowGroup);

            KeyBindings.Current.PARAM_CreateFieldGroup = InputTracker.KeybindLine(15,
                KeyBindings.Current.PARAM_CreateFieldGroup,
                KeyBindings.Default.PARAM_CreateFieldGroup);

            KeyBindings.Current.PARAM_ClearPinnedParams = InputTracker.KeybindLine(16,
                KeyBindings.Current.PARAM_ClearPinnedParams,
                KeyBindings.Default.PARAM_ClearPinnedParams);

            KeyBindings.Current.PARAM_ClearPinnedRows = InputTracker.KeybindLine(17,
                KeyBindings.Current.PARAM_ClearPinnedRows,
                KeyBindings.Default.PARAM_ClearPinnedRows);

            KeyBindings.Current.PARAM_ClearPinnedFields = InputTracker.KeybindLine(18,
                KeyBindings.Current.PARAM_ClearPinnedFields,
                KeyBindings.Default.PARAM_ClearPinnedFields);

            KeyBindings.Current.PARAM_OnlyShowPinnedParams = InputTracker.KeybindLine(19,
                KeyBindings.Current.PARAM_OnlyShowPinnedParams,
                KeyBindings.Default.PARAM_OnlyShowPinnedParams);

            KeyBindings.Current.PARAM_OnlyShowPinnedRows = InputTracker.KeybindLine(20,
                KeyBindings.Current.PARAM_OnlyShowPinnedRows,
                KeyBindings.Default.PARAM_OnlyShowPinnedRows);

            KeyBindings.Current.PARAM_OnlyShowPinnedFields = InputTracker.KeybindLine(21,
                KeyBindings.Current.PARAM_OnlyShowPinnedFields,
                KeyBindings.Default.PARAM_OnlyShowPinnedFields);
        }
    }
}

public class TextEditorKeybindTab
{
    public TextEditorKeybindTab() { }

    public void Display()
    {
        ImGui.Separator();
        ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "Keybinds");
        ImGui.Separator();

        if (ImGui.CollapsingHeader("Core", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.TEXT_FocusSearch = InputTracker.KeybindLine(0,
                KeyBindings.Current.TEXT_FocusSearch,
                KeyBindings.Default.TEXT_FocusSearch);

            KeyBindings.Current.TEXT_SyncDescriptions = InputTracker.KeybindLine(1,
                KeyBindings.Current.TEXT_SyncDescriptions,
                KeyBindings.Default.TEXT_SyncDescriptions);
        }
    }
}
public class GparamEditorKeybindTab
{
    public GparamEditorKeybindTab() { }

    public void Display()
    {
        ImGui.Separator();
        ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "Keybinds");
        ImGui.Separator();

        if (ImGui.CollapsingHeader("Core", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.GPARAM_ExecuteQuickEdit = InputTracker.KeybindLine(0,
                KeyBindings.Current.GPARAM_ExecuteQuickEdit,
                KeyBindings.Default.GPARAM_ExecuteQuickEdit);

            KeyBindings.Current.GPARAM_GenerateQuickEdit = InputTracker.KeybindLine(1,
                KeyBindings.Current.GPARAM_GenerateQuickEdit,
                KeyBindings.Default.GPARAM_GenerateQuickEdit);

            KeyBindings.Current.GPARAM_ClearQuickEdit = InputTracker.KeybindLine(2,
                KeyBindings.Current.GPARAM_ClearQuickEdit,
                KeyBindings.Default.GPARAM_ClearQuickEdit);
        }
    }
}

public class TimeActEditorKeybindTab
{
    public TimeActEditorKeybindTab() { }

    public void Display()
    {
        ImGui.Separator();
        ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "Keybinds");
        ImGui.Separator();

        if (ImGui.CollapsingHeader("Core", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.TIMEACT_Multiselect = InputTracker.KeybindLine(0,
                KeyBindings.Current.TIMEACT_Multiselect,
                KeyBindings.Default.TIMEACT_Multiselect);

            KeyBindings.Current.TIMEACT_MultiselectRange = InputTracker.KeybindLine(1,
                KeyBindings.Current.TIMEACT_MultiselectRange,
                KeyBindings.Default.TIMEACT_MultiselectRange);
        }
    }
}

public class TextureViewerKeybindTab
{
    public TextureViewerKeybindTab() { }

    public void Display()
    {
        ImGui.Separator();
        ImguiUtils.WrappedTextColored(CFG.Current.ImGui_Benefit_Text_Color, "Keybinds");
        ImGui.Separator();

        if (ImGui.CollapsingHeader("Core", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.TEXTURE_ExportTexture = InputTracker.KeybindLine(0,
                KeyBindings.Current.TEXTURE_ExportTexture,
                KeyBindings.Default.TEXTURE_ExportTexture);

            KeyBindings.Current.TEXTURE_ZoomMode = InputTracker.KeybindLine(0,
                KeyBindings.Current.TEXTURE_ZoomMode,
                KeyBindings.Default.TEXTURE_ZoomMode);

            KeyBindings.Current.TEXTURE_ResetZoomLevel = InputTracker.KeybindLine(0,
                KeyBindings.Current.TEXTURE_ResetZoomLevel,
                KeyBindings.Default.TEXTURE_ResetZoomLevel);
        }
    }
}