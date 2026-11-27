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

        EditorFilters.DisplaySearchbar("MapContentSearch", ref SearchInput, ref ExactMatch, "MAP_Contents_Filter_Exact_Match_TT");
        GUI.Tooltip($"Filter the content tree for {mapId}: {mapName}");

        // View: Search Input
        ImGui.SameLine();

        if (ImGui.Button($"{Icons.QuestionCircle}", DPI.IconButtonSize))
        {
            ImGui.OpenPopup("searchInputHint");
        }
        GUI.Tooltip("View documentation on search commands.");

        // A prop: command that can't match anything shows an empty tree, which looks like no matches; say why
        UpdateProblems(map);

        if (_problems.Count > 0)
        {
            ImGui.SameLine();
            ImGui.AlignTextToFramePadding();
            ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, Icons.ExclamationTriangle);
            GUI.Tooltip(string.Join("\n\n", _problems));
        }

        if (ImGui.BeginPopup("searchInputHint"))
        {
            var tableFlags = ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Borders;

            ImGui.Text("The search bar accepts multiple search commands.");
            ImGui.Text($"Join commands with {EditorFilters.SplitChr} to require all of them (; also works), " +
                $"or with {EditorFilters.OR_Chr} to accept any of them.");
            ImGui.Text($"Example: c1000 {EditorFilters.SplitChr} prop:EntityID > 0 {EditorFilters.OR_Chr} modified");
            ImGui.Text($"{EditorFilters.ModifiedTerm} / {EditorFilters.AddedTerm}: objects that differ from, or aren't in, the vanilla map.");
            ImGui.Text($"Start a command with {EditorFilters.NotChr} to show the objects it doesn't match (!c1000, !type:Enemy).");
            ImGui.Text("type:<name>: objects of a category (Part, Region, Event) or type (Enemy, MapPiece, Collision).");
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
                    "\nTarget a slot in an array property with the [] syntax; without it, any slot may match.");

                // Parameter 2
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Parameter 2");

                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("<comparator>: the comparator to use." +
                    "\nAccepted symbols: =, !=, <, >, <=, >=");

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

                // Example 4
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Example");

                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("prop:EntityGroupIDs = 1234" +
                    "\nThe map contents will only show map objects with 1234 in any EntityGroupID slot.");

                // Example 5
                ImGui.TableNextRow();
                ImGui.TableSetColumnIndex(0);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("Example");

                ImGui.TableSetColumnIndex(1);

                ImGui.AlignTextToFramePadding();
                ImGui.Text("prop:EntityID >= 1000 + prop:EntityID < 2000" +
                    "\nThe map contents will only show map objects with an Entity ID from 1000 to 1999.");

                ImGui.EndTable();
            }

            ImGui.EndPopup();
        }
    }

    // Why prop: commands in the search can't match, worked out when the search or the map changes
    private readonly List<string> _problems = new();
    private string _problemsInput;
    private MapContainer _problemsMap;

    /// <summary>
    /// Checks each prop: command: that it reads as prop:name op value, that some object in the map has the
    /// property, and that an ordering comparison has a number (or x,y,z) to compare with
    /// </summary>
    private void UpdateProblems(MapContainer map)
    {
        if (_problemsInput == SearchInput && _problemsMap == map)
            return;

        _problemsInput = SearchInput;
        _problemsMap = map;
        _problems.Clear();

        if (!IsActive || map == null)
            return;

        // Every object type in the map, objects listed under parent files included
        var types = new HashSet<Type>();

        void AddTypes(IEnumerable<Entity> objects)
        {
            foreach (var e in objects)
            {
                if (e?.WrappedObject != null)
                    types.Add(e.WrappedObject.GetType());
            }
        }

        AddTypes(map.Objects);

        foreach (var parents in new[] { map.BTLParents, map.LightAtlasParents, map.LightProbeParents })
        {
            if (parents != null)
            {
                foreach (var p in parents)
                    AddTypes(p.Children);
            }
        }

        foreach (var group in GetParsedGroups())
        {
            foreach (var rawTerm in group)
            {
                var term = EditorFilters.StripNot(rawTerm, out _);

                if (!term.StartsWith("prop:", StringComparison.OrdinalIgnoreCase))
                    continue;

                var command = PropCommand.Match(term);

                if (!command.Success)
                {
                    _problems.Add($"\"{term}\" isn't a complete property command.\n" +
                        "Use prop:<name> <comparator> <value>, e.g. prop:EntityID > 0.");
                    continue;
                }

                var name = PropIndex.Replace(command.Groups["prop"].Value, "");
                var op = command.Groups["op"].Value;
                var value = command.Groups["value"].Value;

                if (!types.Any(t => HasProperty(t, name)))
                {
                    _problems.Add($"No object in this map has a property named \"{name}\".");
                }

                if (op is "<" or ">" or "<=" or ">=")
                {
                    var parts = value.Split(',');
                    var isNumber = TryParseNumber(value, true, out _);
                    var isVector = parts.Length == 3 && parts.All(p => TryParseNumber(p, false, out _));

                    if (!isNumber && !isVector)
                    {
                        _problems.Add($"\"{value}\" isn't a number, so {op} can't compare with it.\n" +
                            "Use a full stop for decimals (1.5), or x,y,z for a position.");
                    }
                }
            }
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
        foreach (var rawCmd in terms)
        {
            // !<command> matches the objects the command doesn't
            var cmd = EditorFilters.StripNot(rawCmd, out var negated);
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
            else if (term.StartsWith("type:"))
            {
                isMatch = TypeFilter(curEnt, term.Substring("type:".Length).Trim());
            }
            // Default to name filter if no explicit command is used
            else
            {
                isMatch = NameFilter(map, curEnt, cmd);
            }

            if (isMatch == negated)
                return false;
        }

        return true;
    }

    /// <summary>
    /// type:<name>: the object's category (Part, Region, Event, Light...) or its own type (Enemy, MapPiece,
    /// Collision...), ignoring case; contained in either unless Exact is on
    /// </summary>
    private bool TypeFilter(Entity curEnt, string typed)
    {
        if (typed == "")
            return true;

        bool Matches(string name)
        {
            if (string.IsNullOrEmpty(name))
                return false;

            name = name.ToLower();
            return ExactMatch ? name == typed : name.Contains(typed);
        }

        if (curEnt is MsbEntity me && Matches(me.Type.ToString()))
            return true;

        return Matches(curEnt.WrappedObject?.GetType().Name);
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
    // of the command, so it may hold spaces. Two-character operators come first so >= isn't read as > then =.
    private static readonly Regex PropCommand = new(
        @"^\s*prop:\s*(?<prop>[^\s=<>!]+)\s*(?<op>>=|<=|!=|=|<|>)\s*(?<value>.+?)\s*$",
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

            // The property, ignoring case, on the object or inside one of its structs (NR and ER keep a part's
            // EntityID in EntityData, which the property view shows inline)
            var chain = ResolvePropertyChain(curEnt.WrappedObject.GetType(), prop);

            if (chain == null)
                return false;

            object targetProp_Value = curEnt.WrappedObject;

            foreach (var link in chain)
            {
                targetProp_Value = link.GetValue(targetProp_Value);

                if (targetProp_Value == null)
                    return false;
            }

            var targetProp = chain[^1];

            // A slot of an array property
            if (index != -1)
            {
                var collection = targetProp_Value;
                targetProp_Value = null;

                if (collection is Array arr && index >= 0 && index < arr.Length)
                {
                    targetProp_Value = arr.GetValue(index);
                }
                else if (collection is IList list && index >= 0 && index < list.Count)
                {
                    targetProp_Value = list[index];
                }
            }

            if (targetProp != null && targetProp_Value != null)
            {
                // A list or array with no slot given: any slot may match (EntityGroupIDs = 1234), and != means no
                // slot holds the value
                if (index == -1 && targetProp_Value is IList slots && targetProp_Value is not string)
                {
                    var values = slots.Cast<object>().Where(v => v != null);

                    isValid = compare == "!="
                        ? values.All(v => IsValueMatch("!=", v, targetValue))
                        : values.Any(v => IsValueMatch(compare, v, targetValue));
                }
                else
                {
                    isValid = IsValueMatch(compare, targetProp_Value, targetValue);
                }
            }
        }

        return isValid;
    }

    /// <summary>
    /// One value against the typed value: = and != by <see cref="IsValueEqual" />, the ordering operators for
    /// numbers and Vector3s only
    /// </summary>
    private static bool IsValueMatch(string compare, object value, string typed)
    {
        switch (compare)
        {
            case "=":
                return IsValueEqual(value, typed);

            case "!=":
                return !IsValueEqual(value, typed);

            default:
                var valueType = value.GetType();
                return SupportsNumericComparison(valueType) &&
                    PerformNumericComparison(compare, typed, value, valueType);
        }
    }

    /// <summary>Whether the type has a property of this name, in any case, on itself or inside a struct</summary>
    private static bool HasProperty(Type type, string prop)
    {
        return ResolvePropertyChain(type, prop) != null;
    }

    // How deep into nested structs a property name is looked for (Part → EntityData → EntityID is 2)
    private const int MaxPropertyDepth = 3;

    // Resolved property paths per object type and typed name; null where the type has no such property
    private static readonly Dictionary<(Type, string), PropertyInfo[]> _chainCache = new();

    /// <summary>
    /// The properties to follow from an object of this type to the named one, ignoring case. A dotted name
    /// (EntityData.EntityID) is followed as written. A plain name is looked for on the type first, then inside
    /// its structs, shallowest first, as the property view shows them inline. Null if none has it.
    /// </summary>
    private static PropertyInfo[] ResolvePropertyChain(Type type, string prop)
    {
        var key = (type, prop.ToLowerInvariant());

        if (_chainCache.TryGetValue(key, out var cached))
            return cached;

        PropertyInfo[] chain = null;

        if (prop.Contains('.'))
        {
            var links = new List<PropertyInfo>();
            var current = type;

            foreach (var segment in prop.Split('.'))
            {
                var link = FindProperty(current, segment);

                if (link == null)
                {
                    links = null;
                    break;
                }

                links.Add(link);
                current = link.PropertyType;
            }

            chain = links?.ToArray();
        }
        else
        {
            // Breadth first, so a property on the object wins over one of the same name in a struct
            var queue = new Queue<(Type Type, PropertyInfo[] Path)>();
            queue.Enqueue((type, Array.Empty<PropertyInfo>()));

            while (queue.Count > 0 && chain == null)
            {
                var (current, path) = queue.Dequeue();

                var found = FindProperty(current, prop);

                if (found != null)
                {
                    chain = path.Append(found).ToArray();
                    break;
                }

                if (path.Length >= MaxPropertyDepth - 1)
                    continue;

                foreach (var nested in current.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    if (IsStructProperty(nested))
                        queue.Enqueue((nested.PropertyType, path.Append(nested).ToArray()));
                }
            }
        }

        _chainCache[key] = chain;

        return chain;
    }

    /// <summary>
    /// A property holding one of the format's own data classes (EntityData, DisplayData...), which is looked
    /// into; not values, strings, arrays, lists or references to other objects' types from outside the format
    /// </summary>
    private static bool IsStructProperty(PropertyInfo property)
    {
        var t = property.PropertyType;

        if (t.IsPrimitive || t.IsEnum || t == typeof(string) || t.IsArray || typeof(IEnumerable).IsAssignableFrom(t))
            return false;

        if (property.GetIndexParameters().Length > 0)
            return false;

        return t.Namespace != null && t.Namespace.StartsWith("SoulsFormats") && t.IsClass;
    }

    /// <summary>A public property of the type by name, ignoring case; null if there's none</summary>
    private static PropertyInfo FindProperty(Type type, string name)
    {
        try
        {
            return type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
        }
        catch (AmbiguousMatchException)
        {
            // Two properties differing only by case: take the exact one
            return type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance);
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
            "<=" => propValue <= target,
            ">=" => propValue >= target,
            _ => false
        };
    }

    /// <summary>
    /// &lt;, &gt;, &lt;= or &gt;= against a number, or for a Vector3 against x,y,z (true only if every axis passes)
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
