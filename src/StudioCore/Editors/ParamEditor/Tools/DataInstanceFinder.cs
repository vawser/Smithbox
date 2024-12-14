using Andre.Formats;
using ImGuiNET;
using SoulsFormats;
using StudioCore.Editor;
using StudioCore.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace StudioCore.Editors.ParamEditor.Tools;

public static class DataInstanceFinder
{
    public static FieldNameFinder FieldNameFinder = new();
    public static FieldValueFinder FieldValueFinder = new();
    public static RowNameFinder RowNameFinder = new();
    public static RowIDFinder RowIDFinder = new();
}

public class FieldNameFinder
{
    public string imguiID = "FieldNameFinder";

    public string SearchText = "";
    public string CachedSearchText = "";
    public bool IncludeCommunityNameInSearch = true;
    public bool IncludeDescriptionInSearch = true;
    public bool MatchTextExactly = false;
    public bool DisplayCommunityNameInResult = false;

    public List<DataSearchResult> Results = new();

    public FieldNameFinder() { }

    public void Display()
    {
        if (ParamBank.PrimaryBank.Params == null)
        {
            return;
        }

        var windowWidth = ImGui.GetWindowWidth();
        var defaultButtonSize = new Vector2(windowWidth * 0.975f, 32);
        var Size = ImGui.GetWindowSize();
        float EditX = (Size.X / 100) * 95;
        float EditY = (Size.Y / 100) * 25;

        UIHelper.WrappedText("Display all fields and the respective params they appear in based on the search text.");
        UIHelper.WrappedText("");

        // Search Text
        UIHelper.WrappedText("Search Text:");
        ImGui.SetNextItemWidth(defaultButtonSize.X);
        ImGui.InputText("##searchString", ref SearchText, 255);

        UIHelper.ShowHoverTooltip("The text to search for. Matches loosely by default.");

        // Checkbox: Include Community Name in Search
        ImGui.Checkbox($"Include Community Name in Search##includeCommunityName_{imguiID}",
            ref IncludeCommunityNameInSearch);

        UIHelper.ShowHoverTooltip("Include the community name text for a field in the search.");

        // Checkbox: Include Descriptions in Search
        ImGui.Checkbox($"Include Descriptions in Search##includeDescriptions_{imguiID}", 
            ref IncludeDescriptionInSearch);

        UIHelper.ShowHoverTooltip("Include the description text for a field in the search.");

        // Checkbox: Match Exactly
        ImGui.Checkbox($"Complete Word Match##matchExact_{imguiID}",
            ref MatchTextExactly);

        UIHelper.ShowHoverTooltip("When matching, ensure the search term is an exact match for a word, not a partial element of the word." +
            "\nFor internal names, this will split the string based on capitalization before checking.");

        // Checkbox: Display Community Name in Results
        ImGui.Checkbox($"Display Community Name in Results##useCommunityNames_{imguiID}", 
            ref DisplayCommunityNameInResult);

        UIHelper.ShowHoverTooltip("Display the community name for the field instead of the internal name.");

        UIHelper.WrappedText("");

        // Results List
        if (Results.Count > 0)
        {
            UIHelper.WrappedText($"Search Term:");
            UIHelper.DisplayAlias(CachedSearchText);

            UIHelper.WrappedText($"Result Count:");
            UIHelper.DisplayAlias($"{Results.Count}");

            UIHelper.WrappedText($"");
            UIHelper.WrappedText($"Param: Row Name");

            ImGui.BeginChild($"##resultSection_{imguiID}",
                new Vector2(EditX * DPI.GetUIScale(), EditY * DPI.GetUIScale()));

            foreach (var result in Results)
            {
                var name = result.FieldInternalName;

                if (DisplayCommunityNameInResult)
                {
                    name = result.FieldDisplayName;
                }

                if (ImGui.Selectable($"{result.ParamName}: {name}##resultEntry_{imguiID}"))
                {
                    EditorCommandQueue.AddCommand($@"param/select/-1/{result.ParamName}");
                }
            }

            ImGui.EndChild();
        }
        else
        {
            ImGui.Text("No results to display.");
        }

        UIHelper.WrappedText("");

        // Search Button
        if (ImGui.Button($"Search##searchButton_{imguiID}", defaultButtonSize))
        {
            if (ParamBank.PrimaryBank.Params != null)
            {
                CachedSearchText = SearchText;

                Results = ConstructResults();
                Results.Sort();
            }
        }
    }

    /// <summary>
    /// Construct the results list when the search button is used.
    /// </summary>
    private List<DataSearchResult> ConstructResults()
    {
        List<DataSearchResult> output = new();

        var searchComponents = SearchText.ToLower().Split(" ");

        foreach (var p in ParamBank.PrimaryBank.Params)
        {
            var def = p.Value.AppliedParamdef;
            var meta = ParamMetaData.Get(def);

            foreach (var field in def.Fields)
            {
                bool addResult = false;
                var fieldMeta = FieldMetaData.Get(field);

                foreach (var entry in searchComponents)
                {
                    // Internal Name
                    if (field.InternalName != null)
                    {
                        if (MatchTextExactly)
                        {
                            var adjustedName = field.InternalName;

                            // Place _ in front of inter-word capitalization
                            adjustedName = Regex.Replace(adjustedName, "(?<!^)([A-Z])", "_$1");

                            var adjustedComponents = adjustedName.Split("_");

                            foreach (var component in adjustedComponents)
                            {
                                if (component.ToLower() == entry)
                                {
                                    addResult = true;
                                }
                            }
                        }
                        else
                        {
                            if (field.InternalName.ToLower().Contains(entry))
                            {
                                addResult = true;
                            }
                        }
                    }

                    // Display Name
                    if (fieldMeta.AltName != null && IncludeCommunityNameInSearch)
                    {
                        var displayNameComponents = fieldMeta.AltName.Split(" ");

                        foreach (var displayComponent in displayNameComponents)
                        {
                            if(MatchTextExactly)
                            {
                                if (displayComponent.ToLower() == entry)
                                {
                                    addResult = true;
                                }
                            }
                            else
                            {
                                if (displayComponent.ToLower().Contains(entry))
                                {
                                    addResult = true;
                                }
                            }
                        }
                    }

                    // Wiki
                    if (fieldMeta.Wiki != null && IncludeDescriptionInSearch)
                    {
                        var descriptionComponents = fieldMeta.Wiki.Split(" ");

                        foreach (var descriptionComponent in descriptionComponents)
                        {
                            if (MatchTextExactly)
                            {
                                if (descriptionComponent.ToLower() == entry)
                                {
                                    addResult = true;
                                }
                            }
                            else
                            {
                                if (descriptionComponent.ToLower().Contains(entry))
                                {
                                    addResult = true;
                                }
                            }
                        }
                    }
                }

                if (addResult)
                {
                    var result = new DataSearchResult();
                    result.FieldInternalName = field.InternalName;
                    result.FieldDisplayName = fieldMeta.AltName;
                    result.ParamName = p.Key;

                    output.Add(result);
                }
            }
        }

        return output;
    }
}

public class FieldValueFinder
{
    public string imguiID = "FieldValueFinder";

    public string SearchText = "";
    public string CachedSearchText = "";
    public string RangeSearchText_Start = "";
    public string RangeSearchText_End = "";

    public bool UseRangeMatchMode = false;
    public bool DisplayFirstMatchOnlyInResult = false;
    public bool DisplayCommunityNameInResult = false;

    public List<DataSearchResult> Results = new();

    public FieldValueFinder() { }

    public void Display()
    {
        if (ParamBank.PrimaryBank.Params == null)
        {
            return;
        }

        var windowWidth = ImGui.GetWindowWidth();
        var defaultButtonSize = new Vector2(windowWidth * 0.975f, 32);
        var Size = ImGui.GetWindowSize();
        float EditX = (Size.X / 100) * 95;
        float EditY = (Size.Y / 100) * 25;

        UIHelper.WrappedText("Display all instances of a specified field value.");
        UIHelper.WrappedText("");

        if (UseRangeMatchMode)
        {
            // Start Value
            UIHelper.WrappedText("Start Value:");
            ImGui.SetNextItemWidth(defaultButtonSize.X);
            ImGui.InputText($"##startSearchValue_{imguiID}", ref RangeSearchText_Start, 255);

            UIHelper.ShowHoverTooltip("The start value in the search range.");

            // End Value
            UIHelper.WrappedText("End Value:");
            ImGui.SetNextItemWidth(defaultButtonSize.X);
            ImGui.InputText($"##endSearchValue_{imguiID}", ref RangeSearchText_End, 255);

            UIHelper.ShowHoverTooltip("The end value in the search range.");
        }

        if (!UseRangeMatchMode)
        {
            UIHelper.WrappedText("Search Value:");
            ImGui.SetNextItemWidth(defaultButtonSize.X);
            ImGui.InputText($"##searchValue_{imguiID}", ref SearchText, 255);

            UIHelper.ShowHoverTooltip("The value to search for.");
        }

        // Checkbox: Enable Range Search
        ImGui.Checkbox($"Enable Range Search##rangeMode_{imguiID}", ref UseRangeMatchMode);

        UIHelper.ShowHoverTooltip("If enabled, the search will search for matches between a start and end value.");

        // Checkbox: Display First Match Only
        ImGui.Checkbox($"Display First Match Only##firstMatchOnly_{imguiID}", ref DisplayFirstMatchOnlyInResult);

        UIHelper.ShowHoverTooltip("Only display the first match within a param, instead of all matches.");

        // Checkbox: Display Community Name in Result
        ImGui.Checkbox($"Display Community Names in Result##displayCommunityNames_{imguiID}", 
            ref DisplayCommunityNameInResult);

        UIHelper.ShowHoverTooltip("Display the community name for the field instead of the internal name.");

        UIHelper.WrappedText("");

        // Result List
        if (Results.Count > 0)
        {
            UIHelper.WrappedText($"Search Term:");
            UIHelper.DisplayAlias(CachedSearchText);

            UIHelper.WrappedText($"Result Count:");
            UIHelper.DisplayAlias($"{Results.Count}");

            UIHelper.WrappedText($"");
            UIHelper.WrappedText($"Param: Row ID: Field Name: Field Value");

            ImGui.BeginChild($"##resultSection_{imguiID}", 
                new Vector2(EditX * DPI.GetUIScale(), EditY * DPI.GetUIScale()));
            
            foreach (var result in Results)
            {
                var name = result.FieldInternalName;

                if (DisplayCommunityNameInResult)
                {
                    name = result.FieldDisplayName;
                }
                if (ImGui.Selectable($"{result.ParamName}: {result.RowID}: {name}: {result.FieldValue}##resultEntry_{imguiID}"))
                {
                    EditorCommandQueue.AddCommand($@"param/select/-1/{result.ParamName}/{result.RowID}/{result.FieldInternalName}");
                }
            }
            ImGui.EndChild();
        }
        else
        {
            ImGui.Text("No results to display.");
        }

        UIHelper.WrappedText("");

        if (ImGui.Button($"Search##searchButton_{imguiID}", defaultButtonSize))
        {
            CachedSearchText = SearchText;

            if(UseRangeMatchMode)
            {
                CachedSearchText = $"{RangeSearchText_Start} -> {RangeSearchText_End}";
            }

            Results = ConstructResults();
            Results.Sort();
        }
    }

    /// <summary>
    /// Construct the results list when the search button is used.
    /// </summary>
    private List<DataSearchResult> ConstructResults()
    {
        List<DataSearchResult> output = new();

        var searchValue = SearchText;
        var startValue = RangeSearchText_Start;
        var endValue = RangeSearchText_End;

        foreach (var p in ParamBank.PrimaryBank.Params)
        {
            for (var i = 0; i < p.Value.Rows.Count; i++)
            {
                var r = p.Value.Rows[i];
                var id = r.ID;

                string fieldName = "";
                string fieldDisplayName = "";

                foreach (var field in r.Cells)
                {
                    PARAMDEF.DefType type = field.Def.DisplayType;

                    var fieldMeta = FieldMetaData.Get(field.Def);
                    fieldName = field.Def.InternalName;
                    fieldDisplayName = fieldMeta.AltName;

                    var isMatch = false;
                    var fieldValue = "";
                    (isMatch, fieldValue) = DataFinderUtil.IsValueMatch(type, field, searchValue, UseRangeMatchMode, startValue, endValue);

                    if (isMatch)
                    {
                        var result = new DataSearchResult();
                        result.RowID = id;
                        result.ParamName = p.Key;
                        result.FieldInternalName = fieldName;
                        result.FieldDisplayName = fieldDisplayName;
                        result.FieldValue = fieldValue;

                        output.Add(result);

                        // Skip matching more if this is enabled
                        if (DisplayFirstMatchOnlyInResult)
                        {
                            break;
                        }
                    }
                }
            }
        }

        return output;
    }
}

public class RowIDFinder
{
    public string imguiID = "RowIDFinder";

    public int SearchID = -1;
    public int SearchIndex = -1;
    public int CachedSearchID = -1;
    public bool IncludeDescriptionInSearch = true;
    public bool DisplayCommunityNameInResult = false;

    public List<DataSearchResult> Results = new();

    public RowIDFinder() { }

    public void Display()
    {
        if (ParamBank.PrimaryBank.Params == null)
        {
            return;
        }

        var windowWidth = ImGui.GetWindowWidth();
        var defaultButtonSize = new Vector2(windowWidth * 0.975f, 32);
        var Size = ImGui.GetWindowSize();
        float EditX = (Size.X / 100) * 95;
        float EditY = (Size.Y / 100) * 25;

        UIHelper.WrappedText("Display all instances of a specificed row ID.");
        UIHelper.WrappedText("");

        // Row ID
        UIHelper.WrappedText("Row ID:");
        ImGui.SetNextItemWidth(defaultButtonSize.X);
        ImGui.InputInt($"##searchId_{imguiID}", ref SearchID);

        UIHelper.ShowHoverTooltip("The row ID to search for.");

        // Row Index
        UIHelper.WrappedText("Row Index:");
        ImGui.SetNextItemWidth(defaultButtonSize.X);
        ImGui.InputInt($"##searchIndex_{imguiID}", ref SearchIndex);

        UIHelper.ShowHoverTooltip("The row index to search for. -1 for any");

        UIHelper.WrappedText("");

        // Result List
        if (Results.Count > 0)
        {
            UIHelper.WrappedText($"Search Term:");
            UIHelper.DisplayAlias($"{CachedSearchID}");

            UIHelper.WrappedText($"Result Count:");
            UIHelper.DisplayAlias($"{Results.Count}");

            UIHelper.WrappedText($"");
            UIHelper.WrappedText($"Param:");

            ImGui.BeginChild($"##resultSection_{imguiID}", 
                new Vector2(EditX * DPI.GetUIScale(), EditY * DPI.GetUIScale()));

            foreach (var result in Results)
            {
                if (ImGui.Selectable($"{result.ParamName}##resultEntry_{imguiID}"))
                {
                    EditorCommandQueue.AddCommand($@"param/select/-1/{result.ParamName}/{result.RowID}");
                }
            }

            ImGui.EndChild();
        }
        else
        {
            ImGui.Text("No results to display.");
        }

        UIHelper.WrappedText("");

        // Search Button
        if (ImGui.Button($"Search##searchButton_{imguiID}", defaultButtonSize))
        {
            CachedSearchID = SearchID;

            Results = ConstructResults();
            Results.Sort();
        }
    }

    /// <summary>
    /// Construct the results list when the search button is used.
    /// </summary>
    private List<DataSearchResult> ConstructResults()
    {
        List<DataSearchResult> output = new();

        foreach (var p in ParamBank.PrimaryBank.Params)
        {
            for (var i = 0; i < p.Value.Rows.Count; i++)
            {
                var r = p.Value.Rows[i];
                var id = r.ID;

                if (r.ID == SearchID && (SearchIndex == -1 || SearchIndex == i))
                {
                    var result = new DataSearchResult();
                    result.ParamName = p.Key;
                    result.RowID = r.ID;

                    output.Add(result);

                    break;
                }
            }
        }

        return output;
    }
}

public class RowNameFinder
{
    public string imguiID = "RowNameFinder";

    public string SearchText = "";
    public string CachedSearchText = "";
    public int SearchIndex = -1;

    public List<DataSearchResult> Results = new();
    public RowNameFinder() { }

    public void Display()
    {
        if (ParamBank.PrimaryBank.Params == null)
        {
            return;
        }

        var windowWidth = ImGui.GetWindowWidth();
        var defaultButtonSize = new Vector2(windowWidth * 0.975f, 32);
        var Size = ImGui.GetWindowSize();
        float EditX = (Size.X / 100) * 95;
        float EditY = (Size.Y / 100) * 25;

        UIHelper.WrappedText("Display all instances of a specificed row name.");
        UIHelper.WrappedText("");

        // Search Text
        UIHelper.WrappedText("Search Text:");
        ImGui.SetNextItemWidth(defaultButtonSize.X);
        ImGui.InputText($"##searchText_{imguiID}", ref SearchText, 255);

        UIHelper.ShowHoverTooltip("The row name to search for. Matches loosely.");

        // Row Index
        UIHelper.WrappedText("Row Index:");
        ImGui.SetNextItemWidth(defaultButtonSize.X);
        ImGui.InputInt($"##rowIndex_{imguiID}", ref SearchIndex);

        UIHelper.ShowHoverTooltip("The row index to search for. -1 for any");

        UIHelper.WrappedText("");

        // Result List
        if (Results.Count > 0)
        {
            UIHelper.WrappedText($"Search Term:");
            UIHelper.DisplayAlias(CachedSearchText);

            UIHelper.WrappedText($"Result Count:");
            UIHelper.DisplayAlias($"{Results.Count}");

            UIHelper.WrappedText($"");
            UIHelper.WrappedText($"Param: Row Name");

            ImGui.BeginChild($"##resultSection_{imguiID}",
                new Vector2(EditX * DPI.GetUIScale(), EditY * DPI.GetUIScale()));

            foreach (var result in Results)
            {
                if (ImGui.Selectable($"{result.ParamName}: {result.RowName}##resultEntry_{imguiID}"))
                {
                    EditorCommandQueue.AddCommand($@"param/select/-1/{result.ParamName}/{result.RowID}");
                }
            }

            ImGui.EndChild();
        }
        else
        {
            ImGui.Text("No results to display.");
        }

        UIHelper.WrappedText("");

        // Search Button
        if (ImGui.Button("Search##action_SearchForRowNames", defaultButtonSize))
        {
            CachedSearchText = SearchText;

            Results = ConstructResults();
            Results.Sort();
        }
    }

    /// <summary>
    /// Construct the results list when the search button is used.
    /// </summary>
    private List<DataSearchResult> ConstructResults()
    {
        List<DataSearchResult> output = new();

        var searchElements = SearchText.Split(" ");

        foreach (var p in ParamBank.PrimaryBank.Params)
        {
            for (var i = 0; i < p.Value.Rows.Count; i++)
            {
                var r = p.Value.Rows[i];

                bool addResult = false;

                var rowName = "";

                foreach (var element in searchElements)
                {
                    if (r.Name != "" || r.Name != null)
                    {
                        var nameElements = r.Name.Split(" ");

                        rowName = r.Name;

                        foreach (var rowElement in nameElements)
                        {
                            if (rowElement.Contains(element) && (SearchIndex == -1 || SearchIndex == i))
                            {
                                addResult = true;
                            }
                        }
                    }
                }

                if (addResult)
                {
                    var result = new DataSearchResult();
                    result.ParamName = p.Key;
                    result.RowID = r.ID;
                    result.RowIndex = i;
                    result.RowName = rowName;

                    output.Add(result);
                }
            }
        }

        return output;
    }
}

/// <summary>
/// Multi-purpose object that holds the result entries of the above classes
/// </summary>
public class DataSearchResult : IComparable<DataSearchResult>
{
    public string ParamName;
    public string RowName;
    public string FieldInternalName;
    public string FieldDisplayName;
    public string FieldValue;

    public int RowID;
    public int RowIndex;

    public string AliasName;
    public string AliasID;
    public string AliasDisplayName;

    public DataSearchResult() { }

    public int CompareTo(DataSearchResult other)
    {
        return this.ParamName.CompareTo(other.ParamName);
    }
}

public static class DataFinderUtil
{
    public static (bool, string) IsValueMatch(PARAMDEF.DefType type, Param.Cell field, string searchValue, bool isRangeSearch, string startValue, string endValue)
    {
        var success = false;
        var startSuccess = false;
        var endSuccess = false;

        switch (type)
        {
            case PARAMDEF.DefType.s8:
                if (isRangeSearch)
                {
                    sbyte sbyteStartVal;
                    sbyte sbyteEndVal;

                    startSuccess = sbyte.TryParse(startValue, out sbyteStartVal);
                    endSuccess = sbyte.TryParse(endValue, out sbyteEndVal);

                    if (startSuccess && endSuccess)
                    {
                        if (((sbyte)field.Value >= sbyteStartVal) &&
                            ((sbyte)field.Value <= sbyteEndVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                else
                {
                    sbyte sbyteVal;

                    success = sbyte.TryParse(searchValue, out sbyteVal);

                    if (success)
                    {
                        if (((sbyte)field.Value == sbyteVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                break;
            case PARAMDEF.DefType.u8:
                if (isRangeSearch)
                {
                    byte byteStartVal;
                    byte byteEndVal;

                    startSuccess = byte.TryParse(startValue, out byteStartVal);
                    endSuccess = byte.TryParse(endValue, out byteEndVal);

                    if (startSuccess && endSuccess)
                    {
                        if (((byte)field.Value >= byteStartVal) &&
                            ((byte)field.Value <= byteEndVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                else
                {
                    byte byteVal;

                    success = byte.TryParse(searchValue, out byteVal);

                    if (success)
                    {
                        if (((byte)field.Value == byteVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                break;
            case PARAMDEF.DefType.s16:
                if (isRangeSearch)
                {
                    short shortStartVal;
                    short shortEndVal;

                    startSuccess = short.TryParse(startValue, out shortStartVal);
                    endSuccess = short.TryParse(endValue, out shortEndVal);

                    if (startSuccess && endSuccess)
                    {
                        if (((short)field.Value >= shortStartVal) &&
                            ((short)field.Value <= shortEndVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                else
                {
                    short shortVal;

                    success = short.TryParse(searchValue, out shortVal);

                    if (success)
                    {
                        if (((short)field.Value == shortVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                break;
            case PARAMDEF.DefType.u16:
                if (isRangeSearch)
                {
                    ushort ushortStartVal;
                    ushort ushortEndVal;

                    startSuccess = ushort.TryParse(startValue, out ushortStartVal);
                    endSuccess = ushort.TryParse(endValue, out ushortEndVal);

                    if (startSuccess && endSuccess)
                    {
                        if (((ushort)field.Value >= ushortStartVal) &&
                            ((ushort)field.Value <= ushortEndVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                else
                {
                    ushort ushortVal;

                    success = ushort.TryParse(searchValue, out ushortVal);

                    if (success)
                    {
                        if (((ushort)field.Value == ushortVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                break;
            case PARAMDEF.DefType.s32:
                if (isRangeSearch)
                {
                    int intStartVal;
                    int intEndVal;

                    startSuccess = int.TryParse(startValue, out intStartVal);
                    endSuccess = int.TryParse(endValue, out intEndVal);

                    if (startSuccess && endSuccess)
                    {
                        if (((int)field.Value >= intStartVal) &&
                            ((int)field.Value <= intEndVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                else
                {
                    int intVal;

                    success = int.TryParse(searchValue, out intVal);

                    if (success)
                    {
                        if (((int)field.Value == intVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                break;
            case PARAMDEF.DefType.u32:
                if (isRangeSearch)
                {
                    uint uintStartVal;
                    uint uintEndVal;

                    startSuccess = uint.TryParse(startValue, out uintStartVal);
                    endSuccess = uint.TryParse(endValue, out uintEndVal);

                    if (startSuccess && endSuccess)
                    {
                        if (((uint)field.Value >= uintStartVal) &&
                            ((uint)field.Value <= uintEndVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                else
                {
                    uint uintVal;

                    success = uint.TryParse(searchValue, out uintVal);

                    if (success)
                    {
                        if (((uint)field.Value == uintVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                break;
            case PARAMDEF.DefType.f32:
                if (isRangeSearch)
                {
                    double floatStartVal;
                    double floatEndVal;

                    startSuccess = double.TryParse(startValue, out floatStartVal);
                    endSuccess = double.TryParse(endValue, out floatEndVal);

                    if (startSuccess && endSuccess)
                    {
                        if (((double)field.Value >= floatStartVal) &&
                            ((double)field.Value <= floatEndVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                else
                {
                    float floatVal;

                    success = float.TryParse(searchValue, out floatVal);

                    if (success)
                    {
                        if (((float)field.Value == floatVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                break;
            case PARAMDEF.DefType.b32:
                if (isRangeSearch)
                {
                    bool boolStartVal;
                    bool boolEndVal;

                    startSuccess = bool.TryParse(startValue, out boolStartVal);
                    endSuccess = bool.TryParse(endValue, out boolEndVal);

                    if (startSuccess && endSuccess)
                    {
                        if (((bool)field.Value == boolStartVal) ||
                            ((bool)field.Value == boolEndVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                else
                {
                    bool boolVal;

                    success = bool.TryParse(searchValue, out boolVal);

                    if (success)
                    {
                        if (((bool)field.Value == boolVal))
                        {
                            return (true, $"{field.Value}");
                        }
                    }
                }
                break;
            case PARAMDEF.DefType.fixstr:
            case PARAMDEF.DefType.fixstrW:
                if (isRangeSearch)
                {
                    if (((string)field.Value == startValue) ||
                    ((string)field.Value == endValue))
                    {
                        return (true, $"{field.Value}");
                    }
                }
                else
                {
                    if ((string)field.Value == searchValue)
                    {
                        return (true, $"{field.Value}");
                    }
                }
                break;
            default: break;
        }

        return (false, "");
    }
}