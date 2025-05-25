using Hexa.NET.ImGui;
using Octokit;
using StudioCore.Editor;
using StudioCore.Editors.MapEditor.Core;
using StudioCore.Editors.ModelEditor.Utils;
using StudioCore.Interface;
using StudioCore.Scene.Interfaces;
using StudioCore.Utilities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static HKLib.hk2018.hkaiUserEdgeUtils;

namespace StudioCore.Editors.MapEditor.Framework;

public class MapContentFilters
{
    private MapEditorScreen Editor;

    public MapContentFilters(MapEditorScreen screen)
    {
        Editor = screen;
    }

    public string SearchInput = "";
    public string StoredSearchInput = "";

    /// <summary>
    /// Display the event filter UI
    /// </summary>
    public void DisplaySearch(MapContentView view)
    {
        var width = ImGui.GetWindowWidth();
        var mapId = view.MapID;
        var mapName = AliasUtils.GetMapNameAlias(Editor.Project, view.MapID);

        ImGui.SetNextItemWidth(width * 0.6f);
        ImGui.InputText($"##contentFilterSearch_{view.ImguiID}", ref SearchInput, 255);
        UIHelper.Tooltip($"Filter the content tree for {mapId}: {mapName}");

        ImGui.SameLine();
        if (ImGui.Button($"{Icons.QuestionCircle}"))
        {
            ImGui.OpenPopup("searchInputHint");
        }
        UIHelper.Tooltip("View documentation on search commands.");

        if (ImGui.BeginPopup("searchInputHint"))
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
    }

    /// <summary>
    /// Search filter for the content tree. 
    /// </summary>
    public bool ContentFilter(MapContentView view, Entity curEnt)
    {
        bool isValid = true;

        var input = SearchInput;

        if (input != "")
        {
            string[] inputParts = input.Split(";");
            bool[] partTruth = new bool[inputParts.Length];
            
            for(int i = 0; i < inputParts.Length; i++)
            {
                var cmd = inputParts[i];

                if (cmd.Contains("prop:"))
                {
                    partTruth[i] = PropertyValueFilter(view, curEnt, cmd);
                }
                // Default to name filter if no explicit command is used
                else
                {
                    partTruth[i] = NameFilter(view, curEnt, cmd);
                }
            }

            foreach (bool entry in partTruth)
            {
                if (!entry)
                {
                    isValid = false;
                }
            }
        }

        return isValid;
    }

    /// <summary>
    /// Filtering based on object name
    /// </summary>
    private bool NameFilter(MapContentView view, Entity curEnt, string cmd)
    {
        bool isValid = false;

        if (curEnt == null)
            return isValid;

        var input = cmd.Replace("name:", "").Trim().ToLower();

        if (curEnt.Name != null)
        {
            var entName = curEnt.Name.Trim().ToLower();

            if (entName != null)
            {
                if (entName != "" && entName.Contains(input))
                    isValid = true;
            }
        }

        if (curEnt.CachedAliasName != null)
        {
            var aliasName = curEnt.CachedAliasName.Trim().ToLower();
            if (aliasName != null)
            {
                if (aliasName != "" && aliasName.Contains(input))
                    isValid = true;
            }
        }

        return isValid;
    }

    /// <summary>
    /// Filtering based on object property value
    /// </summary>
    private bool PropertyValueFilter(MapContentView view, Entity curEnt, string cmd)
    {
        bool isValid = false;

        var input = cmd.Replace("prop:", "");

        var segments = input.Split(" ");
        if(segments.Length >= 3)
        {
            var prop = segments[0];
            var compare = segments[1].Trim().ToLower();
            var targetValue = segments[2].Trim().ToLower();

            var index = -1;

            if(prop.Contains("[") && prop.Contains("]"))
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

                if(targetProp != null)
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
                if(SupportsNumericComparison(valueType) && compare != "=")
                {
                    isValid = PerformNumericComparison(compare, targetValue, targetProp_Value, valueType);
                }
                // Otherwise do string comparison
                else
                {
                    if(targetValue == $"{targetProp_Value}")
                    {
                        isValid = true;
                    }
                }
            }
        }


        return isValid;
    }

    private bool SupportsNumericComparison(Type valueType)
    {
        if( valueType == typeof(byte) || 
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

    private bool PerformNumericComparison(string comparator, string targetVal, object propValue, Type valueType)
    {
        // LONG
        if (valueType == typeof(long))
        {
            var tPropValue = (long)propValue;
            var tTargetValue = (long)propValue;

            var res = long.TryParse(targetVal, out tTargetValue);
            if (res)
            {
                if(comparator == "<")
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
}
