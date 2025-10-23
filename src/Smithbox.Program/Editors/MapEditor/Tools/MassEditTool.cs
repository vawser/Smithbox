﻿using Hexa.NET.ImGui;
using Octokit;
using StudioCore.Configuration;
using StudioCore.Core;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Core;
using StudioCore.Editors.MapEditor.Enums;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Resource.Locators;
using StudioCore.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Editors.MapEditor.Framework.MassEdit;

public class MassEditTool
{
    private MapEditorScreen Editor;
    public ProjectEntry Project;

    private MapListType MapTarget;

    public List<string> MapInputs = new List<string>() { "" };

    public SelectionConditionLogic MapSelectionLogic;

    public List<string> SelectionInputs = new List<string>() { "" };

    public SelectionConditionLogic MapObjectSelectionLogic;

    public List<string> EditInputs = new List<string>() { "" };

    private List<MapActionGroup> MassEditActions = new List<MapActionGroup>();

    public bool ShowMassEditLog = true;
    public MassEditTool(MapEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;
    }

    /// <summary>
    /// Shortcut
    /// </summary>
    public void OnShortcut()
    {

    }

    /// <summary>
    /// Tool Window
    /// </summary>
    public void OnToolWindow()
    {
        var windowWidth = ImGui.GetWindowWidth();

        if (ImGui.CollapsingHeader("Property Mass Edit"))
        {
            UIHelper.SimpleHeader("Map Target", "Map Target", "Determine which maps will be affected by the mass edit.", UI.Current.ImGui_Default_Text_Color);

            ConfigureMapTarget();

            UIHelper.SimpleHeader("Selection Criteria", "Selection Criteria", "Determine which map objects will be affected by the mass edit.", UI.Current.ImGui_Default_Text_Color);

            ConfigureSelection();

            UIHelper.SimpleHeader("Edit Commands", "Edit Commands", "Determine which property to affect and the value change to apply for this mass edit.", UI.Current.ImGui_Default_Text_Color);

            ConfigureEdit();

            if (MayRunEdit)
            {
                if (ImGui.Button("Apply", DPI.StandardButtonSize))
                {
                    StartMassEdit();
                }
            }
            else
            {
                ImGui.BeginDisabled();
                if (ImGui.Button("Apply", DPI.StandardButtonSize))
                {
                }
                ImGui.EndDisabled();
                UIHelper.Tooltip("Mass Edit in the process of being applied.");
            }

            ImGui.Separator();

            if (MassEditActions != null)
            {
                if (ImGui.Button($"{Icons.Eye}##previousEditLog", DPI.IconButtonSize))
                {
                    ShowMassEditLog = !ShowMassEditLog;
                    ClearLogSource();
                }
                UIHelper.Tooltip("Toggle visibility of the edit log.");
            }

            DisplayHintPopups();
            DisplayLog();
        }
    }

    /// <summary>
    /// Handles the map target section
    /// </summary>
    private void ConfigureMapTarget()
    {
        var windowWidth = ImGui.GetWindowWidth();

        //--------------
        // Actions
        //--------------
        // Documentation
        if (ImGui.Button($"{Icons.QuestionCircle}##mapTargetHintButton", DPI.IconButtonSize))
        {
            ImGui.OpenPopup("mapTargetHint");
        }
        UIHelper.Tooltip("View the documentation on map target commands.");

        ImGui.SameLine();

        // Add
        if (ImGui.Button($"{Icons.Plus}##mapSelectionAdd", DPI.IconButtonSize))
        {
            MapInputs.Add("");
        }
        UIHelper.Tooltip("Add new map selection input row.");

        ImGui.SameLine();

        // Remove
        if (MapInputs.Count < 2)
        {
            ImGui.BeginDisabled();

            if (ImGui.Button($"{Icons.Minus}##mapSelectionRemoveDisabled", DPI.IconButtonSize))
            {
                MapInputs.RemoveAt(MapInputs.Count - 1);
            }
            UIHelper.Tooltip("Remove last added map selection input row.");

            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button($"{Icons.Minus}##mapSelectionRemove", DPI.IconButtonSize))
            {
                MapInputs.RemoveAt(MapInputs.Count - 1);
                UIHelper.Tooltip("Remove last added map selection input row.");
            }
        }

        ImGui.SameLine();

        // Reset
        if (ImGui.Button("Reset##resetMapSelectionInput", DPI.StandardButtonSize))
        {
            MapInputs = new List<string>() { "" };
        }
        UIHelper.Tooltip("Reset map selection input rows.");

        ImGui.SameLine();

        // Conditional Logic
        DPI.ApplyInputWidth(windowWidth * 0.3f);
        if (ImGui.BeginCombo($"##mapSelectionCommandLogic", MapSelectionLogic.GetDisplayName()))
        {
            foreach (var entry in Enum.GetValues(typeof(SelectionConditionLogic)))
            {
                var curEnum = (SelectionConditionLogic)entry;

                if (ImGui.Selectable($"{curEnum.GetDisplayName()}", MapSelectionLogic == curEnum))
                {
                    MapSelectionLogic = curEnum;
                }
            }

            ImGui.EndCombo();
        }
        UIHelper.Tooltip("The logic with which to handle the selection inputs." +
            "\n\nAll must match means all the selection criteria must be true for the map object to be included." +
            "\n\nOne must match means only one of the selection criteria must be true for the map object to be included.");

        ImGui.SameLine();

        // Map List Type
        DPI.ApplyInputWidth(windowWidth * 0.3f);
        if (ImGui.BeginCombo("##mapTargetCombo", MapTarget.GetDisplayName()))
        {
            foreach (var entry in Enum.GetValues(typeof(MapListType)))
            {
                var curEnum = (MapListType)entry;

                if (ImGui.Selectable($"{curEnum.GetDisplayName()}", MapTarget == curEnum))
                {
                    MapTarget = curEnum;
                }
            }

            ImGui.EndCombo();
        }
        UIHelper.Tooltip("Determines how the map list is obtained." +
            "\n\nLocal means only currently loaded maps will be edited (that match the map selection criteria)." +
            "\n\nGlobal means all maps will be edited (that match the map selection criteria).\nWARNING: editing a large amounts of maps will cause Smithbox to hang until it is finished, which may be several minutes.");

        //--------------
        // Map Inputs
        //--------------
        for (int i = 0; i < MapInputs.Count; i++)
        {
            var curCommand = MapInputs[i];
            var curText = curCommand;

            DPI.ApplyInputWidth(windowWidth);
            if (ImGui.InputText($"##mapSelectionInput{i}", ref curText, 255))
            {
                MapInputs[i] = curText;
            }
            UIHelper.Tooltip("The map selection command to process.");
        }
    }

    /// <summary>
    /// Handles the selection criteria section
    /// </summary>
    private void ConfigureSelection()
    {
        var windowWidth = ImGui.GetWindowWidth();

        //--------------
        // Actions
        //--------------
        // Documentation
        if (ImGui.Button($"{Icons.QuestionCircle}##selectionHintButton", DPI.IconButtonSize))
        {
            ImGui.OpenPopup("selectionInputHint");
        }
        UIHelper.Tooltip("View documentation on selection commands.");

        ImGui.SameLine();

        // Add
        if (ImGui.Button($"{Icons.Plus}##selectionAdd", DPI.IconButtonSize))
        {
            SelectionInputs.Add("");
        }
        UIHelper.Tooltip("Add new selection input row.");

        ImGui.SameLine();

        // Remove
        if (SelectionInputs.Count < 2)
        {
            ImGui.BeginDisabled();

            if (ImGui.Button($"{Icons.Minus}##selectionRemoveDisabled", DPI.IconButtonSize))
            {
                SelectionInputs.RemoveAt(SelectionInputs.Count - 1);
            }
            UIHelper.Tooltip("Remove last added selection input row.");

            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button($"{Icons.Minus}##selectionRemove", DPI.IconButtonSize))
            {
                SelectionInputs.RemoveAt(SelectionInputs.Count - 1);
                UIHelper.Tooltip("Remove last added selection input row.");
            }
        }

        ImGui.SameLine();

        // Reset
        if (ImGui.Button("Reset##resetSelectionInput", DPI.StandardButtonSize))
        {
            SelectionInputs = new List<string>() { "" };
        }
        UIHelper.Tooltip("Reset selection input rows.");

        ImGui.SameLine();

        // Conditional Logic
        ImGui.SetNextItemWidth(windowWidth * 0.3f);
        if (ImGui.BeginCombo($"##selectionCommandLogic", MapObjectSelectionLogic.GetDisplayName()))
        {
            foreach (var entry in Enum.GetValues(typeof(SelectionConditionLogic)))
            {
                var curEnum = (SelectionConditionLogic)entry;

                if (ImGui.Selectable($"{curEnum.GetDisplayName()}", MapObjectSelectionLogic == curEnum))
                {
                    MapObjectSelectionLogic = curEnum;
                }
            }

            ImGui.EndCombo();
        }
        UIHelper.Tooltip("The logic with which to handle the selection inputs." +
            "\n\nAll must match means all the selection criteria must be true for the map object to be included." +
            "\n\nOne must match means only one of the selection criteria must be true for the map object to be included.");

        //--------------
        // Selection Inputs
        //--------------
        for (int i = 0; i < SelectionInputs.Count; i++)
        {
            var curCommand = SelectionInputs[i];
            var curText = curCommand;

            ImGui.SetNextItemWidth(windowWidth);
            if (ImGui.InputText($"##selectionInput{i}", ref curText, 255))
            {
                SelectionInputs[i] = curText;
            }
            UIHelper.Tooltip("The selection command to process.");
        }

    }

    /// <summary>
    /// Handles the edit section
    /// </summary>
    private void ConfigureEdit()
    {
        var windowWidth = ImGui.GetWindowWidth();

        //--------------
        // Actions Inputs
        //--------------
        // Documentation
        if (ImGui.Button($"{Icons.QuestionCircle}##editHintButton", DPI.IconButtonSize))
        {
            ImGui.OpenPopup("editInputHint");
        }
        UIHelper.Tooltip("View documentation on edit commands.");

        ImGui.SameLine();

        // Add
        if (ImGui.Button($"{Icons.Plus}##editAdd", DPI.IconButtonSize))
        {
            EditInputs.Add("");
        }
        UIHelper.Tooltip("Add edit input row.");

        ImGui.SameLine();

        // Remove
        if (EditInputs.Count < 2)
        {
            ImGui.BeginDisabled();

            if (ImGui.Button($"{Icons.Minus}##editRemoveDisabled", DPI.IconButtonSize))
            {
                EditInputs.RemoveAt(EditInputs.Count - 1);
            }
            UIHelper.Tooltip("Remove last added edit input row.");

            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button($"{Icons.Minus}##editRemove", DPI.IconButtonSize))
            {
                EditInputs.RemoveAt(EditInputs.Count - 1);
            }
            UIHelper.Tooltip("Remove last added edit input row.");
        }

        ImGui.SameLine();

        // Reset
        if (ImGui.Button("Reset##resetEditInputs", DPI.StandardButtonSize))
        {
            EditInputs = new List<string>() { "" };
        }
        UIHelper.Tooltip("Reset edit input rows.");

        //--------------
        // Edit Inputs
        //--------------
        for (int i = 0; i < EditInputs.Count; i++)
        {
            var curCommand = EditInputs[i];
            var curText = curCommand;

            ImGui.SetNextItemWidth(windowWidth);
            if (ImGui.InputText($"##editInput{i}", ref curText, 255))
            {
                EditInputs[i] = curText;
            }
            UIHelper.Tooltip("The edit command to process.");
        }
    }

    private bool MayRunEdit = true;

    private async void StartMassEdit()
    {
        MayRunEdit = false;

        Task<bool> applyEditTask = ProcessMassEdit();
        bool result = await applyEditTask;

        if(result)
        {
            TaskLogs.AddLog("Applied MSB Mass Edit successfully.");
        }
        else
        {
            TaskLogs.AddLog("Failed to apply MSB Mass Edit.");
        }

        MayRunEdit = true;
    }

    /// <summary>
    /// Handles the map-level part of the Mass Edit
    /// </summary>
    private async Task<bool> ProcessMassEdit()
    {
        await Task.Yield();

        var selection = Editor.ViewportSelection;
        var listView = Editor.MapListView;
        var universe = Editor.Universe;

        List<MapActionGroup> actionGroups = new List<MapActionGroup>();

        // Clear selection before applying edits, to ensure the properties view doesn't interfere.
        selection.ClearSelection(Editor);

        // Get filtered list of maps
        var mapList = MapLocator.GetFullMapList(Editor.Project);
        var availableList = new List<string>();
        foreach (var entry in mapList)
        {
            if (IsValidMap(entry))
            {
                availableList.Add(entry);
            }
        }

        // Local
        if (MapTarget is MapListType.Local)
        {
            foreach (var entry in Editor.Project.MapData.PrimaryBank.Maps)
            {
                if (entry.Value.MapContainer != null)
                {
                    if (availableList.Contains(entry.Key.Filename))
                    {
                        if (listView.ContentViews.ContainsKey(entry.Key.Filename))
                        {
                            var curView = listView.ContentViews[entry.Key.Filename];

                            var actionList = ProcessSelectionCriteria(curView);

                            if (actionList.Count > 0)
                                actionGroups.Add(new MapActionGroup(entry.Key.Filename, actionList));
                        }
                    }
                }
            }

            if (actionGroups.Count > 0)
            {
                UpdateLogSource(actionGroups);
                var compoundAction = new MapActionGroupCompoundAction(Editor, actionGroups);
                Editor.EditorActionManager.ExecuteAction(compoundAction);
            }
            else
            {
                return false;
            }
        }

        // Global
        if (MapTarget is MapListType.Global)
        {
            var restoreRendering = false;

            if(CFG.Current.Viewport_Enable_Rendering)
            {
                CFG.Current.Viewport_Enable_Rendering = false;
                restoreRendering = true;
            }

            CFG.Current.MapEditor_IgnoreSaveExceptions = true;

            // Load all maps
            foreach (var entry in availableList)
            {
                Editor.MapListView.TriggerMapLoad(entry);
            }

            // Process each map
            foreach (var entry in Editor.Project.MapData.PrimaryBank.Maps)
            {
                if (entry.Value.MapContainer != null)
                {
                    if (listView.ContentViews.ContainsKey(entry.Key.Filename))
                    {
                        var curView = listView.ContentViews[entry.Key.Filename];

                        var actionList = ProcessSelectionCriteria(curView);

                        if (actionList.Count > 0)
                            actionGroups.Add(new MapActionGroup(entry.Key.Filename, actionList));
                    }
                }
            }

            if (actionGroups.Count > 0)
            {
                UpdateLogSource(actionGroups);
                var compoundAction = new MapActionGroupCompoundAction(Editor, actionGroups);
                Editor.EditorActionManager.ExecuteAction(compoundAction);
            }
            else
            {
                return false;
            }

            universe.SaveAllMaps();

            //universe.UnloadAllMaps();
            foreach (var entry in availableList)
            {
                Editor.MapListView.TriggerMapUnload(entry);
            }

            if(restoreRendering)
            {
                CFG.Current.Viewport_Enable_Rendering = true;
            }

            CFG.Current.MapEditor_IgnoreSaveExceptions = false;
        }

        return true;
    }
    /// <summary>
    /// Handles the selection filtering for map objects
    /// </summary>
    private bool IsValidMap(string mapID)
    {
        var isValid = true;

        if (MapSelectionLogic is SelectionConditionLogic.OR)
        {
            isValid = false;
        }

        bool[] partTruth = new bool[MapInputs.Count];

        for (int i = 0; i < MapInputs.Count; i++)
        {
            var cmd = MapInputs[i];

            // Blank will match for everything
            if (cmd == "")
                partTruth[i] = true;

            if (cmd.Contains("exclude:"))
            {
                var input = cmd.Replace("exclude:", "").Trim().ToLower();

                if (mapID.Contains(input))
                {
                    partTruth[i] = false;
                }
            }
            // Default to name filter if no explicit command is used
            else
            {
                var input = cmd.Trim().ToLower();

                if (mapID.Contains(input))
                {
                    partTruth[i] = true;
                }
            }
        }

        foreach (bool entry in partTruth)
        {
            if (MapSelectionLogic is SelectionConditionLogic.AND)
            {
                if (!entry)
                    isValid = false;
            }
            else if (MapSelectionLogic is SelectionConditionLogic.OR)
            {
                if (entry)
                    isValid = true;
            }
        }

        return isValid;
    }

    /// <summary>
    /// Handles the selection criteria process
    /// </summary>
    private List<ViewportAction> ProcessSelectionCriteria(MapContentView curView)
    {
        List<ViewportAction> actions = new List<ViewportAction>();

        var container = Editor.GetMapContainerFromMapID(curView.MapID);

        if (container != null)
        {
            foreach (var entry in container.Objects)
            {
                if (entry is MsbEntity mEnt)
                {
                    if (IsValidMapObject(curView, mEnt))
                    {
                        var actionList = ProcessEditCommands(curView, mEnt);
                        foreach (var actionEntry in actionList)
                        {
                            actions.Add(actionEntry);
                        }
                    }
                }
            }
        }

        return actions;
    }

    /// <summary>
    /// Handles the selection filtering for map objects
    /// </summary>
    private bool IsValidMapObject(MapContentView curView, MsbEntity mEnt)
    {
        var invertTruth = false;
        var isValid = true;

        if (MapObjectSelectionLogic is SelectionConditionLogic.OR)
        {
            isValid = false;
        }

        bool[] partTruth = new bool[SelectionInputs.Count];

        for (int i = 0; i < SelectionInputs.Count; i++)
        {
            var cmd = SelectionInputs[i];

            if (cmd.StartsWith("!"))
            {
                cmd = cmd.Replace("!", "");
                invertTruth = true;
            }

            if (cmd.Contains("prop:"))
            {
                partTruth[i] = PropertyValueFilter(curView, mEnt, cmd);
                if (invertTruth)
                    partTruth[i] = !partTruth[i];
            }
            // Default to name filter if no explicit command is used
            else
            {
                partTruth[i] = PropertyNameFilter(curView, mEnt, cmd);
                if (invertTruth)
                    partTruth[i] = !partTruth[i];
            }
        }

        foreach (bool entry in partTruth)
        {
            if (MapObjectSelectionLogic is SelectionConditionLogic.AND)
            {
                if (!entry)
                    isValid = false;
            }
            else if (MapObjectSelectionLogic is SelectionConditionLogic.OR)
            {
                if (entry)
                    isValid = true;
            }
        }

        return isValid;
    }

    /// <summary>
    /// Handles the selection filtering for map objects based on name
    /// </summary>
    public bool PropertyNameFilter(MapContentView view, Entity curEnt, string cmd)
    {
        bool isValid = false;

        if (curEnt == null)
            return isValid;

        if (curEnt.Name == null)
            return isValid;

        if (curEnt.CachedAliasName == null)
            return isValid;

        var entName = curEnt.Name.Trim().ToLower();
        var aliasName = curEnt.CachedAliasName.Trim().ToLower();

        var input = cmd.Replace("name:", "").Trim().ToLower();

        if (entName != null)
        {
            if (entName != "" && entName.Contains(input))
                isValid = true;
        }

        if (aliasName != null)
        {
            if (aliasName != "" && aliasName.Contains(input))
                isValid = true;
        }

        return isValid;
    }

    /// <summary>
    /// Handles the selection filtering for map objects based on property value
    /// </summary>
    public bool PropertyValueFilter(MapContentView view, Entity curEnt, string cmd)
    {
        bool isValid = false;

        var input = cmd.Replace("prop:", "");

        var segments = input.Split(" ");
        if (segments.Length >= 3)
        {
            var prop = segments[0];
            var compare = segments[1].Trim().ToLower();
            var targetValue = segments[2].Trim().ToLower();

            var index = -1;

            if (prop.Contains("[") && prop.Contains("]"))
            {
                var match = new Regex(@"\[(.*?)\]").Match(prop);

                if (match.Success)
                {
                    var val = match.Value.Replace("[", "").Replace("]", "");

                    int.TryParse(val, out index);
                    prop = prop.Replace($"{match.Value}", "");
                }
            }

            Type targetObj = curEnt.WrappedObject.GetType();

            PropertyInfo targetProp = null;
            object targetProp_Value = null;

            // Get the actual property from within the array
            if (index != -1)
            {
                targetProp = targetObj.GetProperty(prop);

                if (targetProp != null)
                {
                    object collection = targetProp.GetValue(curEnt.WrappedObject);

                    if (collection is Array arr && index >= 0 && index < arr.Length)
                    {
                        targetProp_Value = arr.GetValue(index);
                    }
                    else if (collection is IList list && index >= 0 && index < list.Count)
                    {
                        targetProp_Value = list[index];
                    }
                }
            }
            else
            {
                targetProp = curEnt.GetProperty(prop);
                targetProp_Value = curEnt.GetPropertyValue(prop);
            }

            if (targetProp != null && targetProp_Value != null)
            {
                var valueType = targetProp_Value.GetType();

                // Do numeric comparison if compare str is < or >
                if (MassEditUtils.IsNumericType(valueType) && compare != "=")
                {
                    isValid = PerformNumericComparison(compare, targetValue, targetProp_Value, valueType);
                }
                // Otherwise do string comparison
                else
                {
                    if (targetValue == $"{targetProp_Value}")
                    {
                        isValid = true;
                    }
                }
            }
        }


        return isValid;
    }


    /// <summary>
    /// Performs the mathematical condition check
    /// </summary>
    public bool PerformNumericComparison(string comparator, string targetVal, object propValue, Type valueType)
    {
        // LONG
        if (valueType == typeof(long))
        {
            var tPropValue = (long)propValue;
            var tTargetValue = (long)propValue;

            var res = long.TryParse(targetVal, out tTargetValue);
            if (res)
            {
                if (comparator == "<")
                {
                    if (tPropValue < tTargetValue)
                        return true;
                }
                if (comparator == ">")
                {
                    if (tPropValue > tTargetValue)
                        return true;
                }
            }
        }
        // UINT
        if (valueType == typeof(uint))
        {
            var tPropValue = (uint)propValue;
            var tTargetValue = (uint)propValue;

            var res = uint.TryParse(targetVal, out tTargetValue);
            if (res)
            {
                if (comparator == "<")
                {
                    if (tPropValue < tTargetValue)
                        return true;
                }
                if (comparator == ">")
                {
                    if (tPropValue > tTargetValue)
                        return true;
                }
            }
        }
        // INT
        if (valueType == typeof(int))
        {
            var tPropValue = (int)propValue;
            var tTargetValue = (int)propValue;

            var res = int.TryParse(targetVal, out tTargetValue);
            if (res)
            {
                if (comparator == "<")
                {
                    if (tPropValue < tTargetValue)
                        return true;
                }
                if (comparator == ">")
                {
                    if (tPropValue > tTargetValue)
                        return true;
                }
            }
        }
        // USHORT
        if (valueType == typeof(ushort))
        {
            var tPropValue = (ushort)propValue;
            var tTargetValue = (ushort)propValue;

            var res = ushort.TryParse(targetVal, out tTargetValue);
            if (res)
            {
                if (comparator == "<")
                {
                    if (tPropValue < tTargetValue)
                        return true;
                }
                if (comparator == ">")
                {
                    if (tPropValue > tTargetValue)
                        return true;
                }
            }
        }
        // SHORT
        if (valueType == typeof(short))
        {
            var tPropValue = (short)propValue;
            var tTargetValue = (short)propValue;

            var res = short.TryParse(targetVal, out tTargetValue);
            if (res)
            {
                if (comparator == "<")
                {
                    if (tPropValue < tTargetValue)
                        return true;
                }
                if (comparator == ">")
                {
                    if (tPropValue > tTargetValue)
                        return true;
                }
            }
        }
        // SBYTE
        if (valueType == typeof(sbyte))
        {
            var tPropValue = (sbyte)propValue;
            var tTargetValue = (sbyte)propValue;

            var res = sbyte.TryParse(targetVal, out tTargetValue);
            if (res)
            {
                if (comparator == "<")
                {
                    if (tPropValue < tTargetValue)
                        return true;
                }
                if (comparator == ">")
                {
                    if (tPropValue > tTargetValue)
                        return true;
                }
            }
        }
        // BYTE
        if (valueType == typeof(byte))
        {
            var tPropValue = (byte)propValue;
            var tTargetValue = (byte)propValue;

            var res = byte.TryParse(targetVal, out tTargetValue);
            if (res)
            {
                if (comparator == "<")
                {
                    if (tPropValue < tTargetValue)
                        return true;
                }
                if (comparator == ">")
                {
                    if (tPropValue > tTargetValue)
                        return true;
                }
            }
        }
        // FLOAT
        if (valueType == typeof(float))
        {
            var tPropValue = (float)propValue;
            var tTargetValue = (float)propValue;

            var res = float.TryParse(targetVal, out tTargetValue);
            if (res)
            {
                if (comparator == "<")
                {
                    if (tPropValue < tTargetValue)
                        return true;
                }
                if (comparator == ">")
                {
                    if (tPropValue > tTargetValue)
                        return true;
                }
            }
        }
        // VECTOR 3
        if (valueType == typeof(Vector3))
        {
            var tPropValue = (Vector3)propValue;
            var tTargetValue = (Vector3)propValue;

            var parts = targetVal.Split(",");
            if (parts.Length >= 2)
            {
                var x = parts[0];
                var y = parts[1];
                var z = parts[2];

                float tX = 0.0f;
                float tY = 0.0f;
                float tZ = 0.0f;

                var resX = float.TryParse(x, out tX);
                var resY = float.TryParse(y, out tY);
                var resZ = float.TryParse(z, out tZ);

                if (resX && resY && resZ)
                {
                    if (comparator == "<")
                    {
                        if (tPropValue.X < tX && tPropValue.Y < tY && tPropValue.Z < tZ)
                            return true;
                    }
                    if (comparator == ">")
                    {
                        if (tPropValue.X > tX && tPropValue.Y > tY && tPropValue.Z > tZ)
                            return true;
                    }
                }
            }
        }


        return false;
    }

    /// <summary>
    /// Handles the edit command process
    /// </summary>
    private List<ViewportAction> ProcessEditCommands(MapContentView curView, MsbEntity mEnt)
    {
        var editCommands = EditInputs;

        List<ViewportAction> actions = new();

        for (int i = 0; i < editCommands.Count; i++)
        {
            var cmd = editCommands[i];

            if (cmd.Contains("random:"))
            {

            }
            // Default to <prop> <operation> <value>
            else
            {
                var action = PropertyValueOperation(curView, mEnt, cmd);
                if (action != null)
                    actions.Add(action);
            }
        }

        return actions;
    }

    /// <summary>
    /// Handles the property value operation edits
    /// TODO: adjust how this is done so we don't need to duplicate the operation logic so much
    /// </summary>
    private ViewportAction PropertyValueOperation(MapContentView curView, MsbEntity curEnt, string cmd)
    {
        var input = cmd.Replace("prop:", "");

        var segments = input.Split(" ");
        if (segments.Length >= 3)
        {
            var prop = segments[0];
            var compare = segments[1].Trim().ToLower();
            var newValue = segments[2].Trim().ToLower();

            var index = -1;

            if (prop.Contains("[") && prop.Contains("]"))
            {
                var match = new Regex(@"\[(.*?)\]").Match(prop);

                if (match.Success)
                {
                    var val = match.Value.Replace("[", "").Replace("]", "");

                    int.TryParse(val, out index);
                    prop = prop.Replace($"{match.Value}", "");
                }
            }

            Type targetObj = curEnt.WrappedObject.GetType();

            PropertyInfo targetProp = null;
            object targetProp_Value = null;

            // Get the actual property from within the array
            if (index != -1)
            {
                targetProp = targetObj.GetProperty(prop);

                if (targetProp != null)
                {
                    object collection = targetProp.GetValue(curEnt.WrappedObject);

                    if (collection is Array arr && index >= 0 && index < arr.Length)
                    {
                        targetProp_Value = arr.GetValue(index);

                        if (targetProp_Value == null)
                            return null;

                        var valueType = targetProp_Value.GetType();

                        // If numeric operation is not supported, force set operation
                        if (!MassEditUtils.IsNumericType(valueType))
                        {
                            compare = "=";
                        }

                        // LONG
                        if (valueType == typeof(long))
                        {
                            long tNewValue = 0;
                            long tExistingValue = (long)targetProp_Value;

                            var res = long.TryParse(newValue, out tNewValue);

                            if (res)
                            {
                                var result = tExistingValue;
                                if (compare == "=")
                                {
                                    try
                                    {
                                        result = tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "+")
                                {
                                    try
                                    {
                                        result += tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "-")
                                {
                                    try
                                    {
                                        result -= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "*")
                                {
                                    try
                                    {
                                        result *= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "/")
                                {
                                    try
                                    {
                                        result /= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }

                                return new PropertiesChangedAction(targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                            }
                        }
                        // UINT
                        if (valueType == typeof(uint))
                        {
                            uint tNewValue = 0;
                            uint tExistingValue = (uint)targetProp_Value;

                            var res = uint.TryParse(newValue, out tNewValue);

                            if (res)
                            {
                                var result = tExistingValue;

                                if (compare == "=")
                                {
                                    try
                                    {
                                        result = tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "+")
                                {
                                    try
                                    {
                                        result += tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "-")
                                {
                                    try
                                    {
                                        result -= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "*")
                                {
                                    try
                                    {
                                        result *= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "/")
                                {
                                    try
                                    {
                                        result /= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }

                                return new PropertiesChangedAction(targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                            }
                        }
                        // INT
                        if (valueType == typeof(int))
                        {
                            int tNewValue = 0;
                            int tExistingValue = (int)targetProp_Value;

                            var res = int.TryParse(newValue, out tNewValue);

                            if (res)
                            {
                                var result = tExistingValue;

                                if (compare == "=")
                                {
                                    try
                                    {
                                        result = tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "+")
                                {
                                    try
                                    {
                                        result += tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "-")
                                {
                                    try
                                    {
                                        result -= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "*")
                                {
                                    try
                                    {
                                        result *= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "/")
                                {
                                    try
                                    {
                                        result /= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }

                                return new PropertiesChangedAction(targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                            }
                        }
                        // USHORT
                        if (valueType == typeof(ushort))
                        {
                            ushort tNewValue = 0;
                            ushort tExistingValue = (ushort)targetProp_Value;

                            var res = ushort.TryParse(newValue, out tNewValue);

                            if (res)
                            {
                                var result = tExistingValue;

                                if (compare == "=")
                                {
                                    try
                                    {
                                        result = tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "+")
                                {
                                    try
                                    {
                                        result += tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "-")
                                {
                                    try
                                    {
                                        result -= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "*")
                                {
                                    try
                                    {
                                        result *= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "/")
                                {
                                    try
                                    {
                                        result /= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }

                                return new PropertiesChangedAction(targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                            }
                        }
                        // SHORT
                        if (valueType == typeof(short))
                        {
                            short tNewValue = 0;
                            short tExistingValue = (short)targetProp_Value;

                            var res = short.TryParse(newValue, out tNewValue);

                            if (res)
                            {
                                var result = tExistingValue;

                                if (compare == "=")
                                {
                                    try
                                    {
                                        result = tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "+")
                                {
                                    try
                                    {
                                        result += tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "-")
                                {
                                    try
                                    {
                                        result -= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "*")
                                {
                                    try
                                    {
                                        result *= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "/")
                                {
                                    try
                                    {
                                        result /= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }

                                return new PropertiesChangedAction(targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                            }
                        }
                        // SBYTE
                        if (valueType == typeof(sbyte))
                        {
                            sbyte tNewValue = 0;
                            sbyte tExistingValue = (sbyte)targetProp_Value;

                            var res = sbyte.TryParse(newValue, out tNewValue);

                            if (res)
                            {
                                var result = tExistingValue;

                                if (compare == "=")
                                {
                                    try
                                    {
                                        result = tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "+")
                                {
                                    try
                                    {
                                        result += tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "-")
                                {
                                    try
                                    {
                                        result -= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "*")
                                {
                                    try
                                    {
                                        result *= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "/")
                                {
                                    try
                                    {
                                        result /= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }

                                return new PropertiesChangedAction(targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                            }
                        }
                        // BYTE
                        if (valueType == typeof(byte))
                        {
                            byte tNewValue = 0;
                            byte tExistingValue = (byte)targetProp_Value;

                            var res = byte.TryParse(newValue, out tNewValue);

                            if (res)
                            {
                                var result = tExistingValue;

                                if (compare == "=")
                                {
                                    try
                                    {
                                        result = tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "+")
                                {
                                    try
                                    {
                                        result += tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "-")
                                {
                                    try
                                    {
                                        result -= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "*")
                                {
                                    try
                                    {
                                        result *= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "/")
                                {
                                    try
                                    {
                                        result /= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }

                                return new PropertiesChangedAction(targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                            }
                        }
                        // FLOAT
                        if (valueType == typeof(float))
                        {
                            float tNewValue = 0;
                            float tExistingValue = (float)targetProp_Value;

                            var res = float.TryParse(newValue, out tNewValue);

                            if (res)
                            {
                                var result = tExistingValue;

                                if (compare == "=")
                                {
                                    try
                                    {
                                        result = tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "+")
                                {
                                    try
                                    {
                                        result += tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "-")
                                {
                                    try
                                    {
                                        result -= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "*")
                                {
                                    try
                                    {
                                        result *= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }
                                if (compare == "/")
                                {
                                    try
                                    {
                                        result /= tNewValue;
                                    }
                                    catch (Exception e)
                                    {
                                        TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                                    }
                                }

                                return new PropertiesChangedAction(targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                            }
                        }
                        // STRING
                        if (valueType == typeof(string))
                        {
                            string result = newValue;

                            return new PropertiesChangedAction(targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                        }
                    }
                }
            }
            else
            {
                targetProp = curEnt.GetProperty(prop);
                targetProp_Value = curEnt.GetPropertyValue(prop);

                if (targetProp_Value == null)
                    return null;

                var valueType = targetProp_Value.GetType();

                // If numeric operation is not supported, force set operation
                if (!MassEditUtils.IsNumericType(valueType))
                {
                    compare = "=";
                }

                // LONG
                if (valueType == typeof(long))
                {
                    long tNewValue = 0;
                    long tExistingValue = (long)targetProp_Value;

                    var res = long.TryParse(newValue, out tNewValue);

                    if (res)
                    {
                        var result = tExistingValue;
                        if (compare == "=")
                        {
                            try
                            {
                                result = tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "+")
                        {
                            try
                            {
                                result += tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "-")
                        {
                            try
                            {
                                result -= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "*")
                        {
                            try
                            {
                                result *= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "/")
                        {
                            try
                            {
                                result /= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }

                        return new PropertiesChangedAction(targetProp, curEnt.WrappedObject, result, curEnt.Name);
                    }
                }
                // UINT
                if (valueType == typeof(uint))
                {
                    uint tNewValue = 0;
                    uint tExistingValue = (uint)targetProp_Value;

                    var res = uint.TryParse(newValue, out tNewValue);

                    if (res)
                    {
                        var result = tExistingValue;

                        if (compare == "=")
                        {
                            try
                            {
                                result = tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "+")
                        {
                            try
                            {
                                result += tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "-")
                        {
                            try
                            {
                                result -= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "*")
                        {
                            try
                            {
                                result *= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "/")
                        {
                            try
                            {
                                result /= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }

                        return new PropertiesChangedAction(targetProp, curEnt.WrappedObject, result, curEnt.Name);
                    }
                }
                // INT
                if (valueType == typeof(int))
                {
                    int tNewValue = 0;
                    int tExistingValue = (int)targetProp_Value;

                    var res = int.TryParse(newValue, out tNewValue);

                    if (res)
                    {
                        var result = tExistingValue;

                        if (compare == "=")
                        {
                            try
                            {
                                result = tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "+")
                        {
                            try
                            {
                                result += tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "-")
                        {
                            try
                            {
                                result -= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "*")
                        {
                            try
                            {
                                result *= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "/")
                        {
                            try
                            {
                                result /= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }

                        return new PropertiesChangedAction(targetProp, curEnt.WrappedObject, result, curEnt.Name);
                    }
                }
                // USHORT
                if (valueType == typeof(ushort))
                {
                    ushort tNewValue = 0;
                    ushort tExistingValue = (ushort)targetProp_Value;

                    var res = ushort.TryParse(newValue, out tNewValue);

                    if (res)
                    {
                        var result = tExistingValue;

                        if (compare == "=")
                        {
                            try
                            {
                                result = tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "+")
                        {
                            try
                            {
                                result += tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "-")
                        {
                            try
                            {
                                result -= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "*")
                        {
                            try
                            {
                                result *= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "/")
                        {
                            try
                            {
                                result /= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }

                        return new PropertiesChangedAction(targetProp, curEnt.WrappedObject, result, curEnt.Name);
                    }
                }
                // SHORT
                if (valueType == typeof(short))
                {
                    short tNewValue = 0;
                    short tExistingValue = (short)targetProp_Value;

                    var res = short.TryParse(newValue, out tNewValue);

                    if (res)
                    {
                        var result = tExistingValue;

                        if (compare == "=")
                        {
                            try
                            {
                                result = tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "+")
                        {
                            try
                            {
                                result += tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "-")
                        {
                            try
                            {
                                result -= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "*")
                        {
                            try
                            {
                                result *= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "/")
                        {
                            try
                            {
                                result /= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }

                        return new PropertiesChangedAction(targetProp, curEnt.WrappedObject, result, curEnt.Name);
                    }
                }
                // SBYTE
                if (valueType == typeof(sbyte))
                {
                    sbyte tNewValue = 0;
                    sbyte tExistingValue = (sbyte)targetProp_Value;

                    var res = sbyte.TryParse(newValue, out tNewValue);

                    if (res)
                    {
                        var result = tExistingValue;

                        if (compare == "=")
                        {
                            try
                            {
                                result = tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "+")
                        {
                            try
                            {
                                result += tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "-")
                        {
                            try
                            {
                                result -= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "*")
                        {
                            try
                            {
                                result *= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "/")
                        {
                            try
                            {
                                result /= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }

                        return new PropertiesChangedAction(targetProp, curEnt.WrappedObject, result, curEnt.Name);
                    }
                }
                // BYTE
                if (valueType == typeof(byte))
                {
                    byte tNewValue = 0;
                    byte tExistingValue = (byte)targetProp_Value;

                    var res = byte.TryParse(newValue, out tNewValue);

                    if (res)
                    {
                        var result = tExistingValue;

                        if (compare == "=")
                        {
                            try
                            {
                                result = tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "+")
                        {
                            try
                            {
                                result += tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "-")
                        {
                            try
                            {
                                result -= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "*")
                        {
                            try
                            {
                                result *= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "/")
                        {
                            try
                            {
                                result /= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }

                        return new PropertiesChangedAction(targetProp, curEnt.WrappedObject, result, curEnt.Name);
                    }
                }
                // FLOAT
                if (valueType == typeof(float))
                {
                    float tNewValue = 0;
                    float tExistingValue = (float)targetProp_Value;

                    var res = float.TryParse(newValue, out tNewValue);

                    if (res)
                    {
                        var result = tExistingValue;

                        if (compare == "=")
                        {
                            try
                            {
                                result = tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "+")
                        {
                            try
                            {
                                result += tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "-")
                        {
                            try
                            {
                                result -= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "*")
                        {
                            try
                            {
                                result *= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }
                        if (compare == "/")
                        {
                            try
                            {
                                result /= tNewValue;
                            }
                            catch (Exception e)
                            {
                                TaskLogs.AddLog($"{e.Message} {e.StackTrace}");
                            }
                        }

                        return new PropertiesChangedAction(targetProp, curEnt.WrappedObject, result, curEnt.Name);
                    }
                }
                // STRING
                if (valueType == typeof(string))
                {
                    string result = newValue;

                    return new PropertiesChangedAction(targetProp, index, curEnt.WrappedObject, result, curEnt.Name);
                }
            }
        }

        return null;
    }

    public void DisplayLog()
    {
        if (ShowMassEditLog)
        {
            var windowSize = DPI.GetWindowSize(Editor.BaseEditor._context);
            var sectionWidth = ImGui.GetWindowWidth() * 0.95f;
            var sectionHeight = windowSize.Y * 0.3f;
            var sectionSize = new Vector2(sectionWidth * DPI.UIScale(), sectionHeight * DPI.UIScale());

            ImGui.BeginChild("massEditLogSection", sectionSize, ImGuiChildFlags.Borders);

            if (MassEditActions != null)
            {
                if (ShowMassEditLog)
                {
                    foreach (var entry in MassEditActions)
                    {
                        var displayName = entry.MapID;
                        var alias = AliasUtils.GetMapNameAlias(Editor.Project, entry.MapID);
                        if (alias != null)
                            displayName = $"{displayName} {alias}";

                        if (ImGui.CollapsingHeader($"{displayName}##mapTab_{entry.MapID}"))
                        {
                            var changes = entry.Actions;

                            foreach (var change in changes)
                            {
                                if (change is PropertiesChangedAction propChange)
                                {
                                    UIHelper.WrappedText($"{propChange.GetEditMessage()}");
                                }
                            }
                        }
                    }
                }
            }

            ImGui.EndChild();
        }
    }
    public void UpdateLogSource(List<MapActionGroup> actionGroups)
    {
        MassEditActions = actionGroups;
    }

    public void ClearLogSource()
    {
        MassEditActions = new List<MapActionGroup>();
    }

    #region Hints
    /// <summary>
    /// Handles the documentation popups
    /// </summary>
    public void DisplayHintPopups()
    {
        // MAP TARGET
        if (ImGui.BeginPopup("mapTargetHint"))
        {
            var tableFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

            if (ImGui.CollapsingHeader("Name", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.BeginTable($"nameSelectionTable", 2, tableFlags))
                {
                    ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthFixed);

                    // Name
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Command");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("<string>");

                    // Description
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Description");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Select map whose name matches, or partially matches the specified string." +
                        "\nLeave blank to target all maps.");

                    ImGui.EndTable();
                }
            }

            if (ImGui.CollapsingHeader("Exclude", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.BeginTable($"nameSelectionTable", 2, tableFlags))
                {
                    ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthFixed);

                    // Name
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Command");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("exclude: <string>");

                    // Description
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Description");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Exclude map whose name matches, or partially matches the specified string.");

                    ImGui.EndTable();
                }
            }

            ImGui.EndPopup();
        }

        // SELECTION
        if (ImGui.BeginPopup("selectionInputHint"))
        {
            var tableFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

            ImGui.Text("Precede the command with ! to select the invert.");

            if (ImGui.CollapsingHeader("Name", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.BeginTable($"nameSelectionTable", 2, tableFlags))
                {
                    ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthFixed);

                    // Name
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Command");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("<string>");

                    // Description
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Description");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Select map objects whose name matches, or partially matches the specified string.");

                    ImGui.EndTable();
                }
            }

            if (ImGui.CollapsingHeader("Property Value", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.BeginTable($"propValueSelectionTable", 2, tableFlags))
                {
                    ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthFixed);

                    // Name
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Command");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("prop: <property name> [<index>] <comparator> <value>");

                    // Description
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Description");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Select map objects who possess the specified property, and where the " +
                        "\nproperty's value is equal, less than or greater than the specified value.");

                    // Parameter 1
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Parameters");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("<property name>: the name of the property to target." +
                        "\nTarget a slot in an array property with the [] syntax.");

                    // Parameter 2
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("<comparator>: the comparator to use." +
                        "\nAccepted symbols: =, <, >");

                    // Parameter 3
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("<value>: the value to check for.");

                    // Example 1
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Examples");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("prop:EntityID = 1" +
                        "\nSelect all map objects with an Entity ID equal to 1.");

                    // Example 2
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("prop:EntityID > 1000" +
                        "\nSelect all map objects with an Entity ID equal or greater than 1000.");

                    // Example 3
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("prop:EntityGroupIDs[1] < 999" +
                        "\nSelect all map objects with an EntityGroupID (at index 1) equal or less than 999");

                    ImGui.EndTable();
                }
            }

            ImGui.EndPopup();
        }

        // EDIT
        if (ImGui.BeginPopup("editInputHint"))
        {
            var tableFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

            if (ImGui.CollapsingHeader("Basic Operations", ImGuiTreeNodeFlags.DefaultOpen))
            {
                if (ImGui.BeginTable($"basicOperationEditTable", 2, tableFlags))
                {
                    ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
                    ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthFixed);

                    // Name
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Command");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("<property name> [<index>] <operation> <value>");

                    // Description
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Description");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Edit the specified property using the operation and value specified " +
                        "\nfor the map objects that meet the selection criteria.");

                    // Parameter 1
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Parameters");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("<property name>: the name of the property to edit." +
                        "\nEdit a slot in an array property with the [] syntax.");

                    // Parameter 2
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("<operation>: the operation to apply with the value." +
                        "\nAccepted operations: =, +, -, *, /");

                    // Parameter 3
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("<value>: the value to use with the operation. " +
                        "\nIf the operation is +, -, * or /, it will conduct the operation with the " +
                        "\nexisting property value one the left-hand side.");

                    // Example 1
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("Examples");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("EntityID = 1000" +
                        "\nSet the Entity ID field in all the map objects that meet the selection " +
                        "\ncriteria to 1000.");

                    // Example 2
                    ImGui.TableNextRow();
                    ImGui.TableSetColumnIndex(0);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("");

                    ImGui.TableSetColumnIndex(1);

                    ImGui.AlignTextToFramePadding();
                    ImGui.Text("EntityID + 999" +
                        "\nAdds 999 to the existing Entity ID field value of all the map objects that " +
                        "\nmeet the selection criteria.");

                    ImGui.EndTable();
                }
            }

            ImGui.EndPopup();
        }
    }
    #endregion
}

