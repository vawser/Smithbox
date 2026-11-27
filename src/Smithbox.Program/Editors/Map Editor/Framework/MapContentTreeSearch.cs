using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.MapEditor;

public class MapContentTreeSearch
{
    private MapEditorView View;

    public MapContentTreeSearch(MapEditorView view)
    {
        View = view;
    }

    public string SearchInput = "";
    public string StoredSearchInput = "";
    public bool ExactMatch = false;

    /// <summary>
    /// Display the event filter UI
    /// </summary>
    public void DisplaySearch(MapContainer map)
    {
        var mapId = "";
        var mapName = "";

        if (map != null)
        {
            mapId = map.Name;
            mapName = AliasHelper.GetMapNameAlias(View.Project, map.Name);
        }

        EditorFilters.DisplaySearchbar("MapContentSearch", ref SearchInput, ref ExactMatch);
        GUI.Tooltip($"Filter the content tree for {mapId}: {mapName}");

        // View: Search Input
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.QuestionCircle}", DPI.IconButtonSize))
        {
            ImGui.OpenPopup("searchInputHint");
        }
        GUI.Tooltip("View documentation on search commands.");

        if (ImGui.BeginPopup("searchInputHint"))
        {
            var tableFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

            ImGui.Text("The search bar accepts multiple search commands.");
            ImGui.Text($"Join commands with {EditorFilters.SplitChr} to require all of them (; also works), " +
                $"or with {EditorFilters.OR_Chr} to accept any of them.");
            ImGui.Text($"Example: c1000 {EditorFilters.SplitChr} prop:EntityID > 0 {EditorFilters.OR_Chr} modified");
            ImGui.Text($"{EditorFilters.ModifiedTerm} / {EditorFilters.AddedTerm}: objects that differ from, or aren't in, the vanilla map.");
            ImGui.Text("Property names and values ignore case, and spaces around the comparator are optional.");

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
                ImGui.Text("<string> or name:<string>");

                // Description
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Description");

                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Filters the map objects by matches against their name or alias.");

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
                ImGui.Text("prop:<property name> [<index>] <comparator> <value>");

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

    /// <summary>Whether a search is entered, so the tree hides what doesn't match</summary>
    public bool IsActive => !string.IsNullOrWhiteSpace(SearchInput);

    /// <summary>
    /// True on the frame the search (its text, the exact toggle or the map searched) changed, so the tree opens
    /// the nodes holding matches once, and the user can still close them afterwards
    /// </summary>
    public bool SearchChanged { get; private set; }

    private string _lastInput = "";
    private bool _lastExact;
    private MapContainer _lastMap;

    // Each object's result this frame: the tree asks for an object more than once (does the node hold a match,
    // then is the row listed), and objects change between frames, so results last one frame only
    private readonly Dictionary<Entity, bool> _frameMatches = new();

    // The parsed search: any group may match (|), and within a group every term must (+, or the older ;)
    private string _parsedInput;
    private List<string[]> _parsedGroups = new();

    /// <summary>
    /// Call once a frame, after the search bar and before the tree
    /// </summary>
    public void BeginFrame(MapContainer map)
    {
        _frameMatches.Clear();

        SearchChanged = SearchInput != _lastInput || ExactMatch != _lastExact || map != _lastMap;

        _lastInput = SearchInput;
        _lastExact = ExactMatch;
        _lastMap = map;
    }

    /// <summary>
    /// Search filter for the content tree.
    /// </summary>
    public bool ContentFilter(MapContainer map, Entity curEnt)
    {
        if (!IsActive)
            return true;

        if (curEnt == null)
            return false;

        if (_frameMatches.TryGetValue(curEnt, out var known))
            return known;

        var result = false;

        foreach (var group in GetParsedGroups())
        {
            if (IsGroupMatch(map, curEnt, group))
            {
                result = true;
                break;
            }
        }

        _frameMatches[curEnt] = result;

        return result;
    }

    private List<string[]> GetParsedGroups()
    {
        if (_parsedInput != SearchInput)
        {
            _parsedInput = SearchInput;
            _parsedGroups = SearchInput
                .Split(EditorFilters.OR_Chr)
                .Select(g => g.Split(new[] { EditorFilters.SplitChr, ";" }, StringSplitOptions.None)
                    .Select(t => t.Trim())
                    .Where(t => t != "")
                    .ToArray())
                .Where(g => g.Length > 0)
                .ToList();
        }

        return _parsedGroups;
    }

    private bool IsGroupMatch(MapContainer map, Entity curEnt, string[] terms)
    {
        foreach (var cmd in terms)
        {
            var term = cmd.ToLower();
            bool isMatch;

            // The shared vanilla-state terms: objects that differ from, or aren't in, the vanilla map
            if (term == EditorFilters.ModifiedTerm || term == EditorFilters.AddedTerm)
            {
                var status = View.Editor.VanillaCompare.GetStatus(curEnt);
                isMatch = term == EditorFilters.ModifiedTerm ? status == VanillaStatus.Modified : status == VanillaStatus.Added;
            }
            else if (term.StartsWith("prop:"))
            {
                isMatch = PropertyValueFilter(map, curEnt, cmd);
            }
            // Default to name filter if no explicit command is used
            else
            {
                isMatch = NameFilter(map, curEnt, cmd);
            }

            if (!isMatch)
                return false;
        }

        return true;
    }

    /// <summary>
    /// Filtering based on object name
    /// </summary>
    private bool NameFilter(MapContainer map, Entity curEnt, string cmd)
    {
        bool isValid = false;

        if (curEnt == null)
            return isValid;

        var input = cmd.Trim().ToLower();

        if (input.StartsWith("name:"))
            input = input.Substring("name:".Length).Trim();

        // The alias is otherwise only cached when a row is drawn, so an object not yet shown (a collapsed node,
        // rows outside a long list's view, or one the search hid) could never be found by its alias
        AliasHelper.UpdateEntityAliasName(View.Project, curEnt);

        // Light atlas entries have no name of their own, and are listed by the part they point to
        var name = curEnt.WrappedObject is SoulsFormats.BTAB.Entry atlasEntry ? atlasEntry.PartName : curEnt.Name;

        if (name != null)
        {
            var entName = name.Trim().ToLower();

            if (entName != null)
            {
                if(ExactMatch)
                {
                    if (entName != "" && entName == input)
                        isValid = true;
                }
                else
                {
                    if (entName != "" && entName.Contains(input))
                        isValid = true;
                }
            }
        }

        if (curEnt.CachedAliasName != null)
        {
            var aliasName = curEnt.CachedAliasName.Trim().ToLower();
            if (aliasName != null)
            {
                if (ExactMatch)
                {
                    if (aliasName != "" && aliasName == input)
                        isValid = true;
                }
                else
                {
                    if (aliasName != "" && aliasName.Contains(input))
                        isValid = true;
                }
            }
        }

        return isValid;
    }

    // prop:<name>[<index>] <op> <value>, with any spacing (or none) around the operator. The value is the rest
    // of the command, so it may hold spaces.
    private static readonly Regex PropCommand = new(
        @"^\s*prop:\s*(?<prop>[^\s=<>]+)\s*(?<op>[=<>])\s*(?<value>.+?)\s*$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex PropIndex = new(@"\[(.*?)\]", RegexOptions.Compiled);

    /// <summary>
    /// Filtering based on object property value
    /// </summary>
    private bool PropertyValueFilter(MapContainer map, Entity curEnt, string cmd)
    {
        bool isValid = false;

        if (curEnt?.WrappedObject == null)
            return false;

        var command = PropCommand.Match(cmd);

        if (command.Success)
        {
            var prop = command.Groups["prop"].Value;
            var compare = command.Groups["op"].Value;
            var targetValue = command.Groups["value"].Value;

            var index = -1;

            if(prop.Contains("[") && prop.Contains("]"))
            {
                var match = PropIndex.Match(prop);

                if (match.Success)
                {
                    var val = match.Value.Replace("[", "").Replace("]", "");

                    int.TryParse(val, out index);
                    prop = prop.Replace($"{match.Value}", "");
                }
            }

            Type targetObj = curEnt.WrappedObject.GetType();

            // Property names are matched ignoring case, like the rest of the search (entityid finds EntityID)
            prop = ResolvePropertyName(targetObj, prop);

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
                // Otherwise an equality check
                else if (compare == "=")
                {
                    isValid = IsValueEqual(targetProp_Value, targetValue);
                }
            }
        }

        return isValid;
    }

    /// <summary>
    /// The property's declared name for a name typed in any case; the typed name if the type has no such
    /// property (or two that differ only by case, which can't be told apart).
    /// </summary>
    private static string ResolvePropertyName(Type type, string prop)
    {
        try
        {
            var info = type.GetProperty(prop, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            return info?.Name ?? prop;
        }
        catch (AmbiguousMatchException)
        {
            return prop;
        }
    }

    /// <summary>
    /// Whether a property value equals the typed value. Bools take 1/0 or true/false, enums their name (any case)
    /// or number, numbers their value (so 1.0 finds 1), and anything else its text ignoring case.
    /// </summary>
    private static bool IsValueEqual(object value, string typed)
    {
        switch (value)
        {
            case bool b:
                if (typed == "1")
                    return b;
                if (typed == "0")
                    return !b;
                return bool.TryParse(typed, out var tb) && tb == b;

            case Enum e:
                if (string.Equals(e.ToString(), typed, StringComparison.OrdinalIgnoreCase))
                    return true;
                return long.TryParse(typed, NumberStyles.Integer, CultureInfo.InvariantCulture, out var tl)
                    && Convert.ToInt64(e, CultureInfo.InvariantCulture) == tl;

            case byte or sbyte or short or ushort or int or uint or long or ulong or float or double:
                if (double.TryParse(typed, NumberStyles.Float, CultureInfo.InvariantCulture, out var td))
                    return Convert.ToDouble(value, CultureInfo.InvariantCulture) == td;
                break;
        }

        return string.Equals($"{value}", typed, StringComparison.OrdinalIgnoreCase);
    }

    private static bool SupportsNumericComparison(Type valueType)
    {
        return valueType == typeof(byte) ||
            valueType == typeof(sbyte) ||
            valueType == typeof(short) ||
            valueType == typeof(ushort) ||
            valueType == typeof(int) ||
            valueType == typeof(uint) ||
            valueType == typeof(long) ||
            valueType == typeof(ulong) ||
            valueType == typeof(float) ||
            valueType == typeof(double) ||
            valueType == typeof(Vector3);
    }

    /// <summary>
    /// A typed number, read with a full stop as the decimal mark whatever the Windows language, and also with a
    /// comma for a single number (1,5) since a user on a comma locale will type that
    /// </summary>
    private static bool TryParseNumber(string text, bool allowCommaDecimal, out double value)
    {
        text = text.Trim();

        if (double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
            return true;

        return allowCommaDecimal &&
            double.TryParse(text.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out value);
    }

    private static bool Compare(string comparator, double propValue, double target)
    {
        return comparator switch
        {
            "<" => propValue < target,
            ">" => propValue > target,
            _ => false
        };
    }

    /// <summary>
    /// &lt; or &gt; against a number, or for a Vector3 against x,y,z (true only if every axis passes)
    /// </summary>
    private static bool PerformNumericComparison(string comparator, string targetVal, object propValue, Type valueType)
    {
        if (propValue is Vector3 vec)
        {
            var parts = targetVal.Split(',');

            if (parts.Length != 3 ||
                !TryParseNumber(parts[0], false, out var x) ||
                !TryParseNumber(parts[1], false, out var y) ||
                !TryParseNumber(parts[2], false, out var z))
            {
                return false;
            }

            return Compare(comparator, vec.X, x) && Compare(comparator, vec.Y, y) && Compare(comparator, vec.Z, z);
        }

        if (!TryParseNumber(targetVal, true, out var target))
            return false;

        return Compare(comparator, Convert.ToDouble(propValue, CultureInfo.InvariantCulture), target);
    }
}
