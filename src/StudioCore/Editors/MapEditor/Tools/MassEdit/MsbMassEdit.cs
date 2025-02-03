using ImGuiNET;
using StudioCore.Editors.MapEditor.Actions.Viewport;
using StudioCore.Editors.MapEditor.Core;
using StudioCore.Editors.MapEditor.Framework;
using StudioCore.Interface;
using StudioCore.MsbEditor;
using StudioCore.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static HKLib.hk2018.hkSerialize.CompatTypeParentInfo;

namespace StudioCore.Editors.MapEditor.Tools;
public enum CommandLogic
{
    [Display(Name = "AND")]
    AND,
    [Display(Name = "OR")]
    OR
}

public enum MapTargetType
{
    [Display(Name = "Loaded")]
    Loaded,
    [Display(Name = "All")]
    All
}


public static class MsbMassEdit
{
    private static MapTargetType MapTarget;

    private static CommandLogic CommandLogic;

    private static List<string> SelectionInputs = new List<string>() { "" };

    private static List<string> EditInputs = new List<string>() { "" };

    private static List<MapActionGroup> PreviousMassEdit = new List<MapActionGroup>();

    private static bool ShowPreviousMassEditLog = true;

    public static void Display()
    {
        var width = ImGui.GetWindowWidth();
        var buttonSize = new Vector2(width, 24);

        UIHelper.WrappedText("Map Target");
        UIHelper.ShowHoverTooltip("Determine which maps will be affected by the mass edit.");
        ImGui.Separator();

        ConfigureMapTarget();

        ImGui.Separator();
        UIHelper.WrappedText("Selection Criteria");
        UIHelper.ShowHoverTooltip("Determine which map objects will be affected by the mass edit.");
        ImGui.Separator();

        ConfigureSelection();

        ImGui.Separator();
        UIHelper.WrappedText("Edit Commands");
        UIHelper.ShowHoverTooltip("Determine which property to affect and the value change to apply for this mass edit.");
        ImGui.Separator();

        ConfigureEdit();

        if(ImGui.Button("Apply", buttonSize))
        {
            ApplyMassEdit();
        }

        ImGui.Separator();

        DisplayPreviousEdits();

        //PopupHints();
    }

    private static void DisplayPreviousEdits()
    {
        if (PreviousMassEdit != null)
        {
            if (ImGui.Button($"{ForkAwesome.Eye}##previousEditLog"))
            {
                ShowPreviousMassEditLog = !ShowPreviousMassEditLog;
            }
            UIHelper.ShowHoverTooltip("Toggle visibility of the previous edit log.");

            if (ShowPreviousMassEditLog)
            {
                ImGui.BeginChild("previousEditLogSection");

                foreach (var entry in PreviousMassEdit)
                {
                    var displayName = entry.MapID;
                    var alias = AliasUtils.GetMapNameAlias(entry.MapID);
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

                ImGui.EndChild();
            }
        }
    }

    private static void ConfigureMapTarget()
    {
        if (ImGui.BeginCombo("##mapTargetCombo", MapTarget.GetDisplayName()))
        {
            foreach (var entry in Enum.GetValues(typeof(MapTargetType)))
            {
                var curEnum = (MapTargetType)entry;

                if (ImGui.Selectable($"{curEnum.GetDisplayName()}", MapTarget == curEnum))
                {
                    MapTarget = curEnum;
                }
            }

            ImGui.EndCombo();
        }

        ImGui.SameLine();
        if (ImGui.Button($"{ForkAwesome.QuestionCircle}##mapTargetHintButton"))
        {
            ImGui.OpenPopup("mapTargetHint");
        }
        UIHelper.ShowHoverTooltip("View the effect the map target types have.");
    }

    private static void ConfigureSelection()
    {
        var width = ImGui.GetWindowWidth();
        var buttonSize = new Vector2(width * 0.32f, 24);

        if (ImGui.Button($"{ForkAwesome.Plus}##selectionAdd"))
        {
            SelectionInputs.Add( "" );
        }
        UIHelper.ShowHoverTooltip("Add new selection input row.");

        ImGui.SameLine();

        if (SelectionInputs.Count < 2)
        {
            ImGui.BeginDisabled();

            if (ImGui.Button($"{ForkAwesome.Minus}##selectionRemoveDisabled"))
            {
                SelectionInputs.RemoveAt(SelectionInputs.Count - 1);
            }
            UIHelper.ShowHoverTooltip("Remove last added selection input row.");

            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button($"{ForkAwesome.Minus}##selectionRemove"))
            {
                SelectionInputs.RemoveAt(SelectionInputs.Count - 1);
                UIHelper.ShowHoverTooltip("Remove last added selection input row.");
            }
        }

        ImGui.SameLine();

        if (ImGui.Button("Reset##resetSelectionInput"))
        {
            SelectionInputs = new List<string>() { "" };
        }
        UIHelper.ShowHoverTooltip("Reset selection input rows.");

        ImGui.SameLine();

        ImGui.SetNextItemWidth(100f);
        if (ImGui.BeginCombo($"##selectionCommandLogic", CommandLogic.GetDisplayName()))
        {
            foreach (var entry in Enum.GetValues(typeof(CommandLogic)))
            {
                var curEnum = (CommandLogic)entry;

                if (ImGui.Selectable($"{curEnum.GetDisplayName()}", CommandLogic == curEnum))
                {
                    CommandLogic = curEnum;
                }
            }

            ImGui.EndCombo();
        }

        ImGui.SameLine();
        if (ImGui.Button($"{ForkAwesome.QuestionCircle}##selectionHintButton"))
        {
            ImGui.OpenPopup("selectionInputHint");
        }
        UIHelper.ShowHoverTooltip("View documentation on selection commands.");

        for (int i = 0; i < SelectionInputs.Count; i++)
        {
            var curCommand = SelectionInputs[i];
            var curText = curCommand;

            if (ImGui.InputText($"##selectionInput{i}", ref curText, 255))
            {
                SelectionInputs[i] = curText;
            }
            UIHelper.ShowHoverTooltip("The selection command to process.");
        }
    }

    private static void ConfigureEdit()
    {
        var width = ImGui.GetWindowWidth();
        var buttonSize = new Vector2(width * 0.32f, 24);

        if (ImGui.Button($"{ForkAwesome.Plus}##editAdd"))
        {
            EditInputs.Add( "" );
        }
        UIHelper.ShowHoverTooltip("Add edit input row.");

        ImGui.SameLine();

        if (EditInputs.Count < 2)
        {
            ImGui.BeginDisabled();

            if (ImGui.Button($"{ForkAwesome.Minus}##editRemoveDisabled"))
            {
                EditInputs.RemoveAt(EditInputs.Count - 1);
            }
            UIHelper.ShowHoverTooltip("Remove last added edit input row.");

            ImGui.EndDisabled();
        }
        else
        {
            if (ImGui.Button($"{ForkAwesome.Minus}##editRemove"))
            {
                EditInputs.RemoveAt(EditInputs.Count - 1);
            }
            UIHelper.ShowHoverTooltip("Remove last added edit input row.");
        }

        ImGui.SameLine();

        if (ImGui.Button("Reset##resetEditInputs"))
        {
            EditInputs = new List<string>() { "" };
        }
        UIHelper.ShowHoverTooltip("Reset edit input rows.");

        ImGui.SameLine();
        if (ImGui.Button($"{ForkAwesome.QuestionCircle}##editHintButton"))
        {
            ImGui.OpenPopup("editInputHint");
        }
        UIHelper.ShowHoverTooltip("View documentation on edit commands.");

        for (int i = 0; i < EditInputs.Count; i++)
        {
            var curCommand = EditInputs[i];
            var curText = curCommand;

            if (ImGui.InputText($"##editInput{i}", ref curText, 255))
            {
                EditInputs[i] = curText;
            }
        }
    }

    private static void PopupHints()
    {
        // MAP TARGET
        if (ImGui.BeginPopup("mapTargetHint"))
        {
            var tableFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

            if (ImGui.BeginTable($"mapTargetHintTable", 2, tableFlags))
            {
                ImGui.TableSetupColumn("Title", ImGuiTableColumnFlags.WidthFixed);
                ImGui.TableSetupColumn("Contents", ImGuiTableColumnFlags.WidthFixed);

                // Title Bar
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Title");

                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Effect");

                // Loaded
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Loaded");

                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("All currently loaded maps will be affected by this mass edit.");

                // Loaded
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("All");

                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("All maps, whether they are loaded or not, will be affected by this mass edit.");

                ImGui.EndTable();
            }
        }

        // SELECTION
        if (ImGui.BeginPopup("selectionInputHint"))
        {
            var tableFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

            ImGui.Text("The search bar accepts multiple search commands.");
            ImGui.Text("Place ; at the end of a command to denote the start of a new command.");

            ImGui.Text("");
            ImGui.Text("Text");

            // Default
            if (ImGui.BeginTable($"defaultParameterTable", 2, tableFlags))
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
                ImGui.Text("Filters the map objects by matches against their name.");

                ImGui.EndTable();
            }

            ImGui.Text("");
            ImGui.Text("Property Value");

            // Property Value
            if (ImGui.BeginTable($"propValueParameterTable", 2, tableFlags))
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
                ImGui.Text("Filters the map objects by value comparisons for the specified property.");

                // Parameter 1
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Parameter 1");

                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("<property name>: the name of the property to target." +
                    "\nTarget a slot in an array property with the [] syntax.");

                // Parameter 2
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Parameter 2");

                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("<comparator>: the comparator to use." +
                    "\nAccepted symbols: =, <, >");

                // Parameter 3
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Parameter 3");

                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("<value>: the value to check for.");

                // Example 1
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Example");

                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("prop:IsShadowOnly = 1" +
                    "\nThe map contents will only show map objects with the Is Shadow only boolean set to true.");

                // Example 2
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Example");

                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("prop:EntityID > 0" +
                    "\nThe map contents will only show map objects with an Entity ID above 0.");

                // Example 3
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Example");

                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("prop:EntityGroupIDs[1] > 0" +
                    "\nThe map contents will only show map objects with an EntityGroupID in slot 1 that is above 0.");

                ImGui.EndTable();
            }

            ImGui.EndPopup();
        }

        // EDIT
        if (ImGui.BeginPopup("editInputHint"))
        {

        }
    }

    /// <summary>
    /// Mass Edit for all maps
    /// </summary>
    private static void ProcessGlobalMassEdit()
    {
        // TODO: this should fill the universe container with all maps,
        // need to implement special 'fast' LoadMap flow so it ignores all rendering aspects when doing this
    }

    private static void ApplyMassEdit()
    {
        var selection = Smithbox.EditorHandler.MapEditor.Selection;
        var listView = Smithbox.EditorHandler.MapEditor.MapListView;
        var universe = Smithbox.EditorHandler.MapEditor.Universe;

        // Clear selection before applying edits, to ensure the properties view doesn't interfere.
        selection.ClearSelection();

        List<MapActionGroup> actionGroups = new List<MapActionGroup>();

        if (MapTarget is MapTargetType.Loaded)
        {
            if (universe.LoadedObjectContainers.Count > 0)
            {
                var maps = universe.LoadedObjectContainers
                        .Where(k => k.Key is not null)
                        .OrderBy(k => k.Key);

                foreach (var entry in maps)
                {
                    if(listView.ContentViews.ContainsKey(entry.Key))
                    {
                        var curView = listView.ContentViews[entry.Key];

                        var actionList = ProcessLocalMassEdit(curView);

                        if(actionList.Count > 0)
                            actionGroups.Add(new MapActionGroup(entry.Key, actionList));
                    }
                }
            }
        }
        else if (MapTarget is MapTargetType.All)
        {
            ProcessGlobalMassEdit();
        }

        if (actionGroups.Count > 0)
        {
            PreviousMassEdit = actionGroups;
            var compoundAction = new MapActionGroupCompoundAction(actionGroups);
            Smithbox.EditorHandler.MapEditor.EditorActionManager.ExecuteAction(compoundAction);
        }
        else
        {
            TaskLogs.AddLog("MSB mass edit could not be applied.");
        }
    }

    /// <summary>
    /// Mass Edit for loaded maps
    /// </summary>
    private static List<ViewportAction> ProcessLocalMassEdit(MapContentView curView)
    {
        List<ViewportAction> actions = new List<ViewportAction>();

        if (curView.Container != null)
        {
            foreach (var entry in curView.Container.Objects)
            {
                if (entry is MsbEntity mEnt)
                {
                    if (IsSelected(curView, mEnt))
                    {
                        var actionList = ApplyEdit(curView, mEnt);
                        foreach(var actionEntry in actionList)
                        {
                            actions.Add(actionEntry);
                        }
                    }
                }
            }
        }

        return actions;
    }

    private static bool IsSelected(MapContentView curView, MsbEntity mEnt)
    {
        var isValid = true;

        var selectionCommands = SelectionInputs;
        var commandLogic = CommandLogic;

        if(commandLogic is CommandLogic.OR)
        {
            isValid = false;
        }

        bool[] partTruth = new bool[selectionCommands.Count];

        for (int i = 0; i < selectionCommands.Count; i++)
        {
            var cmd = selectionCommands[i];

            if (cmd.Contains("prop:"))
            {
                partTruth[i] = PropertyValueFilter(curView, mEnt, cmd);
            }
            // Default to name filter if no explicit command is used
            else
            {
                partTruth[i] = NameFilter(curView, mEnt, cmd);
            }
        }

        foreach (bool entry in partTruth)
        {
            if (commandLogic is CommandLogic.AND)
            {
                if (!entry)
                    isValid = false;
            }
            else if (commandLogic is CommandLogic.OR)
            {
                if (entry)
                    isValid = true;
            }
        }

        return isValid;
    }

    /// <summary>
    /// Filtering based on object name
    /// </summary>
    private static bool NameFilter(MapContentView view, Entity curEnt, string cmd)
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
    /// Filtering based on object property value
    /// </summary>
    private static bool PropertyValueFilter(MapContentView view, Entity curEnt, string cmd)
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
                if (IsNumericType(valueType) && compare != "=")
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

    private static bool IsNumericType(Type valueType)
    {
        if (valueType == typeof(byte) ||
            valueType == typeof(sbyte) ||
            valueType == typeof(short) ||
            valueType == typeof(ushort) ||
            valueType == typeof(int) ||
            valueType == typeof(uint) ||
            valueType == typeof(long) ||
            valueType == typeof(float) ||
            valueType == typeof(double))
        {
            return true;
        }

        return false;
    }

    private static bool PerformNumericComparison(string comparator, string targetVal, object propValue, Type valueType)
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

    private static List<ViewportAction> ApplyEdit(MapContentView curView, MsbEntity mEnt)
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
                if(action != null)
                    actions.Add(action);
            }
        }

        return actions;
    }

    private static ViewportAction PropertyValueOperation(MapContentView curView, MsbEntity curEnt, string cmd)
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
                        if (!IsNumericType(valueType))
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
                if (!IsNumericType(valueType))
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
                        if(compare == "=")
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
                        if(compare == "+")
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
            }
        }

        return null;
    }

    public static void ClearPreviousMassEditLog()
    {
        PreviousMassEdit = new List<MapActionGroup>();
    }
}

public class MapActionGroup
{
    public string MapID { get; set; }

    public List<ViewportAction> Actions { get; set; }

    public MapActionGroup(string mapID, List<ViewportAction> actions)
    {
        MapID = mapID;
        Actions = actions;
    }
}

