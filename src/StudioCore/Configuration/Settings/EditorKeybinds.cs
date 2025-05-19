using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hexa.NET.ImGui;
using StudioCore.Configuration.Windows;

namespace StudioCore.Configuration.Settings;

//------------------------------------------
// Common
//------------------------------------------
#region Common
public class CommonKeybindTab
{
    public CommonKeybindTab() { }

    public void Display()
    {
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

            KeyBindings.Current.CORE_RedoContinuousAction = InputTracker.KeybindLine(11,
                KeyBindings.Current.CORE_RedoContinuousAction,
                KeyBindings.Default.CORE_RedoContinuousAction);

            KeyBindings.Current.CORE_UndoAction = InputTracker.KeybindLine(4,
                KeyBindings.Current.CORE_UndoAction,
                KeyBindings.Default.CORE_UndoAction);

            KeyBindings.Current.CORE_UndoContinuousAction = InputTracker.KeybindLine(10,
                KeyBindings.Current.CORE_UndoContinuousAction,
                KeyBindings.Default.CORE_UndoContinuousAction);

            KeyBindings.Current.CORE_SaveAll = InputTracker.KeybindLine(5,
                KeyBindings.Current.CORE_SaveAll,
                KeyBindings.Default.CORE_SaveAll);

            KeyBindings.Current.CORE_Save = InputTracker.KeybindLine(6,
                KeyBindings.Current.CORE_Save,
                KeyBindings.Default.CORE_Save);
        }
    }
}
#endregion 

//------------------------------------------
// Viewport
//------------------------------------------
#region Viewport
public class ViewportKeybindTab
{
    public ViewportKeybindTab() { }

    public void Display()
    {
        if (ImGui.CollapsingHeader("Camera", ImGuiTreeNodeFlags.DefaultOpen))
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

            KeyBindings.Current.VIEWPORT_ToggleRenderType = InputTracker.KeybindLine(15,
                KeyBindings.Current.VIEWPORT_ToggleRenderType,
                KeyBindings.Default.VIEWPORT_ToggleRenderType);
        }
    }
}
#endregion

//------------------------------------------
// Map Editor
//------------------------------------------
#region Map Editor
public class MapEditorKeybindTab
{
    public MapEditorKeybindTab() { }

    public void Display()
    {
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

            KeyBindings.Current.MAP_RotateFixedAngle = InputTracker.KeybindLine(50,
                KeyBindings.Current.MAP_RotateFixedAngle,
                KeyBindings.Default.MAP_RotateFixedAngle);

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

            KeyBindings.Current.MAP_DisableGamePresence = InputTracker.KeybindLine(51,
                KeyBindings.Current.MAP_DisableGamePresence,
                KeyBindings.Default.MAP_DisableGamePresence);

            KeyBindings.Current.MAP_EnableGamePresence = InputTracker.KeybindLine(52,
                KeyBindings.Current.MAP_EnableGamePresence,
                KeyBindings.Default.MAP_EnableGamePresence);

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

        if (ImGui.CollapsingHeader("Rotation Increment", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.MAP_RotateSelectionXAxis = InputTracker.KeybindLine(80,
                KeyBindings.Current.MAP_RotateSelectionXAxis,
                KeyBindings.Default.MAP_RotateSelectionXAxis);

            KeyBindings.Current.MAP_NegativeRotateSelectionXAxis = InputTracker.KeybindLine(81,
                KeyBindings.Current.MAP_NegativeRotateSelectionXAxis,
                KeyBindings.Default.MAP_NegativeRotateSelectionXAxis);

            KeyBindings.Current.MAP_RotateSelectionYAxis = InputTracker.KeybindLine(82,
                KeyBindings.Current.MAP_RotateSelectionYAxis,
                KeyBindings.Default.MAP_RotateSelectionYAxis);

            KeyBindings.Current.MAP_NegativeRotateSelectionYAxis = InputTracker.KeybindLine(83,
                KeyBindings.Current.MAP_NegativeRotateSelectionYAxis,
                KeyBindings.Default.MAP_NegativeRotateSelectionYAxis);

            KeyBindings.Current.MAP_PivotSelectionYAxis = InputTracker.KeybindLine(84,
                KeyBindings.Current.MAP_PivotSelectionYAxis,
                KeyBindings.Default.MAP_PivotSelectionYAxis);

            KeyBindings.Current.MAP_NegativePivotSelectionYAxis = InputTracker.KeybindLine(85,
                KeyBindings.Current.MAP_NegativePivotSelectionYAxis,
                KeyBindings.Default.MAP_NegativePivotSelectionYAxis);

            KeyBindings.Current.MAP_SwitchDegreeIncrementType = InputTracker.KeybindLine(86,
                KeyBindings.Current.MAP_SwitchDegreeIncrementType,
                KeyBindings.Default.MAP_SwitchDegreeIncrementType);

            KeyBindings.Current.MAP_SwitchDegreeIncrementTypeBackward = InputTracker.KeybindLine(87,
                KeyBindings.Current.MAP_SwitchDegreeIncrementTypeBackward,
                KeyBindings.Default.MAP_SwitchDegreeIncrementTypeBackward);

        }

        if (ImGui.CollapsingHeader("Movement Increment", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.MAP_KeyboardMove_PositiveX = InputTracker.KeybindLine(70,
                KeyBindings.Current.MAP_KeyboardMove_PositiveX,
                KeyBindings.Default.MAP_KeyboardMove_PositiveX);

            KeyBindings.Current.MAP_KeyboardMove_NegativeX = InputTracker.KeybindLine(71,
                KeyBindings.Current.MAP_KeyboardMove_NegativeX,
                KeyBindings.Default.MAP_KeyboardMove_NegativeX);

            KeyBindings.Current.MAP_KeyboardMove_PositiveY = InputTracker.KeybindLine(72,
                KeyBindings.Current.MAP_KeyboardMove_PositiveY,
                KeyBindings.Default.MAP_KeyboardMove_PositiveY);

            KeyBindings.Current.MAP_KeyboardMove_NegativeY = InputTracker.KeybindLine(73,
                KeyBindings.Current.MAP_KeyboardMove_NegativeY,
                KeyBindings.Default.MAP_KeyboardMove_NegativeY);

            KeyBindings.Current.MAP_KeyboardMove_PositiveZ = InputTracker.KeybindLine(74,
                KeyBindings.Current.MAP_KeyboardMove_PositiveZ,
                KeyBindings.Default.MAP_KeyboardMove_PositiveZ);

            KeyBindings.Current.MAP_KeyboardMove_NegativeZ = InputTracker.KeybindLine(75,
                KeyBindings.Current.MAP_KeyboardMove_NegativeZ,
                KeyBindings.Default.MAP_KeyboardMove_NegativeZ);

            KeyBindings.Current.MAP_KeyboardMove_CycleIncrement = InputTracker.KeybindLine(76,
                KeyBindings.Current.MAP_KeyboardMove_CycleIncrement,
                KeyBindings.Default.MAP_KeyboardMove_CycleIncrement);

            KeyBindings.Current.MAP_KeyboardMove_CycleIncrementBackward = InputTracker.KeybindLine(77,
                KeyBindings.Current.MAP_KeyboardMove_CycleIncrementBackward,
                KeyBindings.Default.MAP_KeyboardMove_CycleIncrementBackward);
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
            KeyBindings.Current.MAP_ToggleWorldMap = InputTracker.KeybindLine(46,
                KeyBindings.Current.MAP_ToggleWorldMap,
                KeyBindings.Default.MAP_ToggleWorldMap);
        }
    }
}
#endregion

//------------------------------------------
// Model Editor
//------------------------------------------
#region Model Editor
public class ModelEditorKeybindTab
{
    public ModelEditorKeybindTab() { }

    public void Display()
    {
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
#endregion

//------------------------------------------
// Param Editor
//------------------------------------------
#region Param Editor
public class ParamEditorKeybindTab
{
    public ParamEditorKeybindTab() { }

    public void Display()
    {
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

            KeyBindings.Current.PARAM_SortRows = InputTracker.KeybindLine(23,
                KeyBindings.Current.PARAM_SortRows,
                KeyBindings.Default.PARAM_SortRows);

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

            KeyBindings.Current.PARAM_CopyId = InputTracker.KeybindLine(9,
                KeyBindings.Current.PARAM_CopyId,
                KeyBindings.Default.PARAM_CopyId);

            KeyBindings.Current.PARAM_CopyId = InputTracker.KeybindLine(10,
                KeyBindings.Current.PARAM_CopyIdAndName,
                KeyBindings.Default.PARAM_CopyIdAndName);
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

            KeyBindings.Current.PARAM_ExportCSV_Names = InputTracker.KeybindLine(11,
                KeyBindings.Current.PARAM_ExportCSV_Names,
                KeyBindings.Default.PARAM_ExportCSV_Names);

            KeyBindings.Current.PARAM_ExportCSV_Param = InputTracker.KeybindLine(12,
                KeyBindings.Current.PARAM_ExportCSV_Param,
                KeyBindings.Default.PARAM_ExportCSV_Param);

            KeyBindings.Current.PARAM_ExportCSV_AllRows = InputTracker.KeybindLine(13,
                KeyBindings.Current.PARAM_ExportCSV_AllRows,
                KeyBindings.Default.PARAM_ExportCSV_AllRows);


            KeyBindings.Current.PARAM_ExportCSV_ModifiedRows = InputTracker.KeybindLine(14,
                KeyBindings.Current.PARAM_ExportCSV_ModifiedRows,
                KeyBindings.Default.PARAM_ExportCSV_ModifiedRows);

            KeyBindings.Current.PARAM_ExportCSV_SelectedRows = InputTracker.KeybindLine(15,
                KeyBindings.Current.PARAM_ExportCSV_SelectedRows,
                KeyBindings.Default.PARAM_ExportCSV_SelectedRows);
        }

        if (ImGui.CollapsingHeader("Row Namer", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.PARAM_ApplyRowNamer = InputTracker.KeybindLine(60,
                KeyBindings.Current.PARAM_ApplyRowNamer,
                KeyBindings.Default.PARAM_ApplyRowNamer);
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
            KeyBindings.Current.PARAM_CreateParamPinGroup = InputTracker.KeybindLine(13,
                KeyBindings.Current.PARAM_CreateParamPinGroup,
                KeyBindings.Default.PARAM_CreateParamPinGroup);

            KeyBindings.Current.PARAM_CreateRowPinGroup = InputTracker.KeybindLine(14,
                KeyBindings.Current.PARAM_CreateRowPinGroup,
                KeyBindings.Default.PARAM_CreateRowPinGroup);

            KeyBindings.Current.PARAM_CreateFieldPinGroup = InputTracker.KeybindLine(15,
                KeyBindings.Current.PARAM_CreateFieldPinGroup,
                KeyBindings.Default.PARAM_CreateFieldPinGroup);

            KeyBindings.Current.PARAM_ClearCurrentPinnedParams = InputTracker.KeybindLine(16,
                KeyBindings.Current.PARAM_ClearCurrentPinnedParams,
                KeyBindings.Default.PARAM_ClearCurrentPinnedParams);

            KeyBindings.Current.PARAM_ClearCurrentPinnedRows = InputTracker.KeybindLine(17,
                KeyBindings.Current.PARAM_ClearCurrentPinnedRows,
                KeyBindings.Default.PARAM_ClearCurrentPinnedRows);

            KeyBindings.Current.PARAM_ClearCurrentPinnedFields = InputTracker.KeybindLine(18,
                KeyBindings.Current.PARAM_ClearCurrentPinnedFields,
                KeyBindings.Default.PARAM_ClearCurrentPinnedFields);

            KeyBindings.Current.PARAM_OnlyShowPinnedParams = InputTracker.KeybindLine(19,
                KeyBindings.Current.PARAM_OnlyShowPinnedParams,
                KeyBindings.Default.PARAM_OnlyShowPinnedParams);

            KeyBindings.Current.PARAM_OnlyShowPinnedRows = InputTracker.KeybindLine(20,
                KeyBindings.Current.PARAM_OnlyShowPinnedRows,
                KeyBindings.Default.PARAM_OnlyShowPinnedRows);

            KeyBindings.Current.PARAM_OnlyShowPinnedFields = InputTracker.KeybindLine(21,
                KeyBindings.Current.PARAM_OnlyShowPinnedFields,
                KeyBindings.Default.PARAM_OnlyShowPinnedFields);

            KeyBindings.Current.PARAM_InheritReferencedRowName = InputTracker.KeybindLine(22,
                KeyBindings.Current.PARAM_InheritReferencedRowName,
                KeyBindings.Default.PARAM_InheritReferencedRowName);
        }
    }
}
#endregion

//------------------------------------------
// Text Editor
//------------------------------------------
#region Text Editor
public class TextEditorKeybindTab
{
    public TextEditorKeybindTab() { }

    public void Display()
    {
        if (ImGui.CollapsingHeader("Core", ImGuiTreeNodeFlags.DefaultOpen))
        {
            KeyBindings.Current.TEXT_SelectAll = InputTracker.KeybindLine(0,
                KeyBindings.Current.TEXT_SelectAll,
                KeyBindings.Default.TEXT_SelectAll);

            KeyBindings.Current.TEXT_Multiselect = InputTracker.KeybindLine(1,
                KeyBindings.Current.TEXT_Multiselect,
                KeyBindings.Default.TEXT_Multiselect);

            KeyBindings.Current.TEXT_MultiselectRange = InputTracker.KeybindLine(2,
                KeyBindings.Current.TEXT_MultiselectRange,
                KeyBindings.Default.TEXT_MultiselectRange);

            KeyBindings.Current.TEXT_CopyEntrySelection = InputTracker.KeybindLine(3,
                KeyBindings.Current.TEXT_CopyEntrySelection,
                KeyBindings.Default.TEXT_CopyEntrySelection);

            KeyBindings.Current.TEXT_PasteEntrySelection = InputTracker.KeybindLine(4,
                KeyBindings.Current.TEXT_PasteEntrySelection,
                KeyBindings.Default.TEXT_PasteEntrySelection);

            KeyBindings.Current.TEXT_CopyEntryContents = InputTracker.KeybindLine(5,
                KeyBindings.Current.TEXT_CopyEntryContents,
                KeyBindings.Default.TEXT_CopyEntryContents);

            KeyBindings.Current.TEXT_FocusSelectedEntry = InputTracker.KeybindLine(6,
                KeyBindings.Current.TEXT_FocusSelectedEntry,
                KeyBindings.Default.TEXT_FocusSelectedEntry);
        }
    }
}
#endregion

//------------------------------------------
// Graphics Param Editor
//------------------------------------------
#region Graphics Param Editor
public class GparamEditorKeybindTab
{
    public GparamEditorKeybindTab() { }

    public void Display()
    {
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

            KeyBindings.Current.GPARAM_ReloadParam = InputTracker.KeybindLine(3,
                KeyBindings.Current.GPARAM_ReloadParam,
                KeyBindings.Default.GPARAM_ReloadParam);
        }
    }
}
#endregion

//------------------------------------------
// Time Act Editor
//------------------------------------------
#region Time Act Editor
public class TimeActEditorKeybindTab
{
    public TimeActEditorKeybindTab() { }

    public void Display()
    {
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
#endregion

//------------------------------------------
// Texture Viewer
//------------------------------------------
#region Texture Viewer
public class TextureViewerKeybindTab
{
    public TextureViewerKeybindTab() { }

    public void Display()
    {
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
#endregion