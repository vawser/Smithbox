using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.ModelEditor;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;

namespace StudioCore.Editors.TimeActEditor;

public enum TimeActSearchMode
{
    /// <summary>
    /// Searches the selected source
    /// </summary>
    Local,

    /// <summary>
    /// Searches every source of the project
    /// </summary>
    Global
}

/// <summary>
/// Finds animations and events by their names, properties, fields and values. Each result jumps to what it found,
/// selecting the source, time act, animation and event. The search runs in the background, as a global search
/// reads the time acts of every source.
/// </summary>
public class TimeActSearchTool
{
    public TimeActEditorView View;
    public ProjectEntry Project;
    public TimeActSearchMode Mode;

    private enum SearchScope
    {
        CurrentTimeAct,
        AllTimeActs
    }

    /// <summary>
    /// What was found. The references are to the objects searched, which for a source that wasn't open are copies
    /// read for the search, so the IDs and positions are kept as well to find them again once the source is opened.
    /// </summary>
    private record SearchResult(
        TimeActContainerWrapper Source, ModelListType ListType,
        TimeActEntry Entry, string EntryName, int BindIndex,
        TAE.Animation Animation, long AnimationID,
        TAE.Event Event, int EventIndex, int EventType,
        string Label);

    /// <summary>
    /// A source to search, with what is needed of the editor taken beforehand, as the search runs off the main thread
    /// </summary>
    private record SearchSource(TimeActContainerWrapper Source, ModelListType ListType,
        List<TimeActEntry> Entries, TAE.Template.BankTemplate Template);

    /// <summary>
    /// The inputs as they were when the search started, as they can be edited while it runs
    /// </summary>
    private record SearchQuery(string Animation, string Event, string Field, string Value, bool Exact)
    {
        public bool HasAnimation => !string.IsNullOrWhiteSpace(Animation);
        public bool HasEvent => !string.IsNullOrWhiteSpace(Event);
        public bool HasField => !string.IsNullOrWhiteSpace(Field);
        public bool HasValue => !string.IsNullOrWhiteSpace(Value);
    }

    /// <summary>
    /// A search in progress. The results are passed over through the queue and picked up as the tool is drawn.
    /// </summary>
    private class SearchRun
    {
        public readonly CancellationTokenSource Cancellation = new();
        public readonly ConcurrentQueue<SearchResult> Found = new();

        public volatile int Progress;
        public volatile int Total;
        public volatile string Status = "";
        public volatile bool Done;
        public volatile bool Truncated;

        public int Count;
    }

    // The results stop here, as a value such as 0 is found in nearly every event
    private const int MaxResults = 10000;

    private readonly string Id;

    private SearchScope Scope = SearchScope.CurrentTimeAct;
    private string AnimationText = "";
    private string EventText = "";
    private string FieldText = "";
    private string ValueText = "";
    private bool Exact = false;

    private SearchRun Run;
    // Each result with the label it is listed under, which the filter matches
    private record ResultRow(SearchResult Result, string Label);

    private List<ResultRow> Results;
    private TimeActContainerWrapper ResultSource;
    private bool ShowSourceInLabel;
    private bool ShowTimeActInLabel;

    private string ResultFilter = "";
    private bool ExactResultFilter = false;

    // Rebuilt only when the filter changes, and added to as results arrive
    private List<int> FilteredResults;
    private List<ResultRow> FilteredResultsFor;
    private string CachedResultFilter = "";
    private bool CachedExactResultFilter = false;
    private int CheckedResultCount;

    public TimeActSearchTool(TimeActEditorView view, ProjectEntry project, TimeActSearchMode mode)
    {
        View = view;
        Project = project;
        Mode = mode;

        Id = mode is TimeActSearchMode.Local ? "local" : "global";
    }

    public void OnToolWindow()
    {
        var headerKey = Mode is TimeActSearchMode.Local ? "TAE_SearchTool_Local_Header" : "TAE_SearchTool_Global_Header";

        if (!ToolSections.Header($"{LOC.Get(headerKey)}##timeActSearchToolHeader_{Id}"))
            return;

        ImGui.BeginChild($"TimeActSearchToolSection_{Id}", ImGuiChildFlags.Borders);

        Display();

        ImGui.EndChild();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get(Mode is TimeActSearchMode.Local ? "TAE_SearchTool_Local_Hint" : "TAE_SearchTool_Global_Hint"));

        var source = View.Selection.SelectedTimeActContainerWrapper;

        if (Mode is TimeActSearchMode.Local)
        {
            // The results point into the source they were found in
            if (Results != null && ResultSource != source)
            {
                ClearResults();
            }

            if (source == null)
            {
                GUI.WrappedText(LOC.Get("TAE_SearchTool_No_Source"));
                return;
            }
        }
        else if (Project.Handler.TimeActData.PrimaryBank is { IsScanning: true } bank)
        {
            GUI.WrappedText(LOC.Get("TAE_SearchTool_Global_Scanning", bank.ScanProgress, bank.ScanTotal));
        }

        if (DisplayInputs())
        {
            StartSearch(source);
        }

        DisplayProgress();
        DisplayResults();
    }

    #region Inputs
    private bool DisplayInputs()
    {
        var search = false;

        // Scope: a global search covers every time act of every source
        if (Mode is TimeActSearchMode.Local)
        {
            GUI.Spacer();
            GUI.SimpleHeader(
                LOC.Get("TAE_SearchTool_Scope"),
                LOC.Get("TAE_SearchTool_Scope_TT"));

            ImGui.SetNextItemWidth(-1);

            if (ImGui.BeginCombo($"##timeActSearchScope_{Id}", GetScopeName(Scope)))
            {
                foreach (var scope in Enum.GetValues<SearchScope>())
                {
                    if (ImGui.Selectable(GetScopeName(scope), Scope == scope))
                    {
                        Scope = scope;
                    }
                }

                ImGui.EndCombo();
            }
        }

        search |= InputSection("TAE_SearchTool_Animation", $"##timeActSearchAnimation_{Id}", ref AnimationText);
        search |= InputSection("TAE_SearchTool_Event", $"##timeActSearchEvent_{Id}", ref EventText);
        search |= InputSection("TAE_SearchTool_Field", $"##timeActSearchField_{Id}", ref FieldText);
        search |= InputSection("TAE_SearchTool_Value", $"##timeActSearchValue_{Id}", ref ValueText);

        // Options
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TAE_SearchTool_Options_Header"),
            LOC.Get("TAE_SearchTool_Options_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("TAE_SearchTool_Exact")}##timeActSearchExact_{Id}", ref Exact);
        GUI.Tooltip(LOC.Get("TAE_SearchTool_Exact_TT"));

        var query = GetQuery();
        var hasQuery = query.HasAnimation || query.HasEvent || query.HasField || query.HasValue;

        // Actions
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TAE_SearchTool_Actions_Header"),
            LOC.Get("TAE_SearchTool_Actions_Header_TT"));

        GUI.ConditionalMultiButtonInput($"timeActSearchActions_{Id}",
            $"timeActSearchButton_{Id}",
            LOC.Get("TAE_SearchTool_Search"),
            LOC.Get("TAE_SearchTool_Search_TT"),
            () => search = true,
            hasQuery,

            $"timeActSearchClear_{Id}",
            LOC.Get("TAE_SearchTool_Clear"),
            LOC.Get("TAE_SearchTool_Clear_TT"),
            ClearInputs,
            true);

        return search && hasQuery;
    }

    private void ClearInputs()
    {
        AnimationText = "";
        EventText = "";
        FieldText = "";
        ValueText = "";

        ClearResults();
    }

    /// <summary>
    /// A titled text input. Returns true when the field is left after its text was changed, which Enter does as well,
    /// so a search can be run without the button.
    /// </summary>
    private static bool InputSection(string nameKey, string id, ref string text)
    {
        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get(nameKey),
            LOC.Get($"{nameKey}_TT"));

        ImGui.SetNextItemWidth(-1);

        // The text is only kept on the frames the input returns true, so it can't wait for Enter to return:
        // the text would be lost when the field is left by clicking elsewhere
        ImGui.InputText(id, ref text, 255);
        GUI.Tooltip(LOC.Get($"{nameKey}_TT"));

        return ImGui.IsItemDeactivatedAfterEdit();
    }

    private static string GetScopeName(SearchScope scope) => scope switch
    {
        SearchScope.CurrentTimeAct => LOC.Get("TAE_SearchTool_Scope_Current"),
        SearchScope.AllTimeActs => LOC.Get("TAE_SearchTool_Scope_All"),
        _ => $"{scope}"
    };

    private SearchQuery GetQuery()
    {
        return new SearchQuery(AnimationText.Trim(), EventText.Trim(), FieldText.Trim(), ValueText.Trim(), Exact);
    }
    #endregion

    #region Search
    private void CancelRun()
    {
        Run?.Cancellation.Cancel();
        Run = null;
    }

    private void ClearResults()
    {
        CancelRun();

        Results = null;
        ResultSource = null;
    }

    /// <summary>
    /// Gathers the sources to search on the main thread, where the editor's lists can be read safely,
    /// then hands them to a background task
    /// </summary>
    private void StartSearch(TimeActContainerWrapper selectedSource)
    {
        ClearResults();

        var data = Project.Handler.TimeActData;
        var sources = new List<SearchSource>();

        if (Mode is TimeActSearchMode.Local)
        {
            var entries = Scope is SearchScope.AllTimeActs
                ? selectedSource.TimeActs.ToList()
                : View.Selection.SelectedTimeAct != null ? [View.Selection.SelectedTimeAct] : [];

            sources.Add(new SearchSource(selectedSource, GetListType(selectedSource), entries, data.GetTemplate(selectedSource)));

            ShowSourceInLabel = false;
            ShowTimeActInLabel = Scope is SearchScope.AllTimeActs;
        }
        else
        {
            var bank = data.PrimaryBank;

            foreach (var (listType, list) in new[] { (ModelListType.Character, bank.GetCharacters()), (ModelListType.Asset, bank.GetAssets()) })
            {
                foreach (var source in list.OrderBy(e => e.Name))
                {
                    // An open source, or one with unsaved changes, is searched as it is in the editor.
                    // The rest are read again for the search, without being opened.
                    var entries = source.TimeActs.Count > 0 ? source.TimeActs.ToList() : null;

                    sources.Add(new SearchSource(source, listType, entries, data.GetTemplate(source)));
                }
            }

            ShowSourceInLabel = true;
            ShowTimeActInLabel = true;
        }

        Results = new();
        ResultSource = selectedSource;

        var run = new SearchRun();
        var query = GetQuery();

        // A local search counts its time acts, as there is only the one source
        run.Total = Mode is TimeActSearchMode.Local ? sources[0].Entries.Count : sources.Count;

        Run = run;

        var token = run.Cancellation.Token;

        Task.Run(() =>
        {
            try
            {
                Search(run, query, sources, token);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("TAE_SearchTool_Failed"), e);
            }
            finally
            {
                run.Done = true;
            }
        }, token);
    }

    private ModelListType GetListType(TimeActContainerWrapper source)
    {
        return Project.Handler.TimeActData.PrimaryBank.GetAssets().Contains(source)
            ? ModelListType.Asset
            : ModelListType.Character;
    }

    private void Search(SearchRun run, SearchQuery query, List<SearchSource> sources, CancellationToken token)
    {
        var countEntries = Mode is TimeActSearchMode.Local;

        foreach (var item in sources)
        {
            token.ThrowIfCancellationRequested();

            run.Status = item.Source.Name;

            List<TimeActEntry> entries;

            try
            {
                entries = item.Entries ?? item.Source.ReadTimeActs();
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("TAE_Data_Failed_To_Read_Source", item.Source.SourcePath), e);

                run.Progress++;
                continue;
            }

            foreach (var entry in entries)
            {
                token.ThrowIfCancellationRequested();

                if (countEntries)
                {
                    run.Status = $"{item.Source.Name} {entry.Name}";
                }

                try
                {
                    if (!SearchTimeAct(run, query, item, entry, token))
                        return;
                }
                catch (OperationCanceledException)
                {
                    throw;
                }
                catch (Exception e)
                {
                    // The open time act can be edited while it is searched, which can upset the lists being read
                    Smithbox.LogError(this, LOC.Get("TAE_SearchTool_TimeAct_Failed", item.Source.Name, entry.Name), e);
                }

                if (countEntries)
                {
                    run.Progress++;
                }
            }

            if (!countEntries)
            {
                run.Progress++;
            }
        }
    }

    /// <summary>
    /// Returns false once there are as many results as are kept
    /// </summary>
    private bool SearchTimeAct(SearchRun run, SearchQuery query, SearchSource item, TimeActEntry entry, CancellationToken token)
    {
        entry.Load();

        if (entry.TimeAct == null)
            return true;

        var source = item.Source;

        // Only the animations are listed when nothing is asked of the events
        var searchEvents = query.HasEvent || query.HasField || query.HasValue;

        foreach (var animation in entry.TimeAct.Animations.ToArray())
        {
            token.ThrowIfCancellationRequested();

            var animationName = source.GetAnimationName(entry, animation);

            if (!MatchAnimation(query, source, animation, animationName, out var animationMatch))
                continue;

            if (!searchEvents)
            {
                var label = animationMatch != null ? $"{animationName}  ({animationMatch})" : animationName;

                if (!AddResult(run, CreateResult(item, entry, animation, null, -1, label)))
                    return false;

                continue;
            }

            // A field or value on its own can be found in the properties of the animation, as well as in its events
            if (!query.HasEvent &&
                MatchFields(query, GetAnimationProperties(animation), out var propertyMatch))
            {
                if (!AddResult(run, CreateResult(item, entry, animation, null, -1, $"{animationName}  {propertyMatch}")))
                    return false;
            }

            var events = animation.Events.ToArray();

            for (int i = 0; i < events.Length; i++)
            {
                var ev = events[i];
                var eventName = GetEventName(ev, item.Template);

                if (!MatchText(query, query.Event, eventName, $"{ev.Type}"))
                    continue;

                // Events whose template hasn't been applied yet are read through a copy, so the time act is left as it is
                var readable = entry.GetReadableEvent(ev, item.Template);

                if (!MatchFields(query, GetEventFields(readable), out var fieldMatch))
                    continue;

                var label = fieldMatch != null
                    ? $"{animationName}  {eventName}  {fieldMatch}"
                    : $"{animationName}  {eventName}";

                if (!AddResult(run, CreateResult(item, entry, animation, ev, i, label)))
                    return false;
            }
        }

        return true;
    }

    private static SearchResult CreateResult(SearchSource item, TimeActEntry entry, TAE.Animation animation,
        TAE.Event ev, int eventIndex, string label)
    {
        return new SearchResult(item.Source, item.ListType,
            entry, entry.Name, entry.BindIndex,
            animation, animation.ID,
            ev, eventIndex, ev?.Type ?? -1,
            label);
    }

    private static bool AddResult(SearchRun run, SearchResult result)
    {
        if (run.Count >= MaxResults)
        {
            run.Truncated = true;
            return false;
        }

        run.Count++;
        run.Found.Enqueue(result);

        return true;
    }

    /// <summary>
    /// Matches the name and alias of an animation, or failing those the values of its properties.
    /// Gives the property that matched, or null if it was the name.
    /// </summary>
    private bool MatchAnimation(SearchQuery query, TimeActContainerWrapper source, TAE.Animation animation, string name, out string match)
    {
        match = null;

        if (!query.HasAnimation)
            return true;

        var alias = AliasHelper.GetAnimationAlias(Project, source.Name, name);

        if (MatchText(query, query.Animation, name, alias))
            return true;

        foreach (var field in GetAnimationProperties(animation))
        {
            if (MatchText(query, query.Animation, field.Value))
            {
                match = $"{field.DisplayName}: {field.Value}";
                return true;
            }
        }

        return false;
    }

    private readonly record struct SearchField(string Name, string DisplayName, string Value, string EnumName);

    private static IEnumerable<SearchField> GetAnimationProperties(TAE.Animation animation)
    {
        yield return new("ID", LOC.Get("TAE_PropertiesWindow_Animation_ID"), $"{animation.ID}", null);

        if (!string.IsNullOrEmpty(animation.AnimFileName))
            yield return new("AnimFileName", LOC.Get("TAE_PropertiesWindow_Animation_File_Name"), animation.AnimFileName, null);

        switch (animation.MiniHeader)
        {
            case TAE.Animation.AnimMiniHeader.Standard standard:
                if (standard.ImportsHKX)
                {
                    yield return new("ImportHKXSourceAnimID", LOC.Get("TAE_PropertiesWindow_Animation_Import_HKX_Source"),
                        $"{standard.ImportHKXSourceAnimID}", null);
                }
                break;

            case TAE.Animation.AnimMiniHeader.ImportOtherAnim import:
                yield return new("ImportFromAnimID", LOC.Get("TAE_PropertiesWindow_Animation_Import_From"),
                    $"{import.ImportFromAnimID}", null);
                break;
        }
    }

    private IEnumerable<SearchField> GetEventFields(TAE.Event ev)
    {
        yield return new("StartTime", LOC.Get("TAE_PropertiesWindow_Start_Time"), FormatValue(ev.StartTime), null);
        yield return new("EndTime", LOC.Get("TAE_PropertiesWindow_End_Time"), FormatValue(ev.EndTime), null);

        if (ev.Parameters == null)
            yield break;

        var data = Project.Handler.TimeActData;

        foreach (var (key, parameter) in ev.Parameters.Template)
        {
            // Fixed values aren't parameters as such, so they aren't shown in the properties either
            if (parameter.ValueToAssert != null)
                continue;

            var value = ev.Parameters[key];
            var annotation = data.GetFieldAnnotation(ev, parameter.Name)?.Name;
            var displayName = data.GetFieldDisplayName(ev, parameter.Name);

            string enumName = null;

            if (parameter.EnumEntries != null)
            {
                enumName = parameter.EnumEntries.FirstOrDefault(e => Equals(e.Value, value)).Key;
            }

            // The annotation is matched as well as the internal name, whichever is on show
            var name = string.IsNullOrEmpty(annotation) || annotation == parameter.Name
                ? parameter.Name
                : $"{parameter.Name}\n{annotation}";

            yield return new(name, displayName, FormatValue(value), enumName);
        }
    }

    /// <summary>
    /// Matches the field and value inputs against a set of fields. A field and a value must match on the same one.
    /// Gives the field that matched.
    /// </summary>
    private static bool MatchFields(SearchQuery query, IEnumerable<SearchField> fields, out string match)
    {
        match = null;

        if (!query.HasField && !query.HasValue)
            return true;

        foreach (var field in fields)
        {
            if (query.HasField && !MatchFieldName(query, field.Name))
                continue;

            if (query.HasValue && !MatchValue(query, field.Value, field.EnumName))
                continue;

            var shown = field.EnumName != null ? $"{field.EnumName} ({field.Value})" : field.Value;

            match = $"{field.DisplayName} = {shown}";
            return true;
        }

        return false;
    }

    /// <summary>
    /// The internal name and the annotation of a field are held together, split by a line break
    /// </summary>
    private static bool MatchFieldName(SearchQuery query, string name)
    {
        return MatchText(query, query.Field, name.Split('\n'));
    }

    private static bool MatchValue(SearchQuery query, string value, string enumName)
    {
        // Numbers are compared as numbers when exact, so 1 finds 1.0
        if (query.Exact &&
            double.TryParse(query.Value, NumberStyles.Float, CultureInfo.InvariantCulture, out var inputNumber) &&
            double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var valueNumber))
        {
            return inputNumber == valueNumber;
        }

        return MatchText(query, query.Value, value, enumName);
    }

    /// <summary>
    /// Whether any of the texts contains the input, or equals it when the search is exact
    /// </summary>
    private static bool MatchText(SearchQuery query, string input, params string[] texts)
    {
        if (string.IsNullOrWhiteSpace(input))
            return true;

        foreach (var text in texts)
        {
            if (string.IsNullOrEmpty(text))
                continue;

            if (query.Exact ? text.Trim().Equals(input, StringComparison.OrdinalIgnoreCase)
                            : text.Contains(input, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static string FormatValue(object value) => value switch
    {
        null => "",
        IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
        _ => value.ToString()
    };

    private static string GetEventName(TAE.Event ev, TAE.Template.BankTemplate template)
    {
        var name = ev.TypeName;

        if (name == null && template != null && template.TryGetValue(ev.Type, out var eventTemplate))
        {
            name = eventTemplate.Name;
        }

        return name != null ? $"[{ev.Type}] {name}" : $"[{ev.Type}]";
    }
    #endregion

    #region Results
    /// <summary>
    /// Picks up what the search has found so far, and shows how far it has got
    /// </summary>
    private void DisplayProgress()
    {
        var run = Run;

        if (run == null)
            return;

        while (run.Found.TryDequeue(out var result))
        {
            Results?.Add(new ResultRow(result, GetResultLabel(result)));
        }

        if (run.Done)
            return;

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TAE_SearchTool_Progress_Header"),
            LOC.Get("TAE_SearchTool_Progress_Header_TT"));

        var total = Math.Max(run.Total, 1);
        var fraction = Math.Clamp((float)run.Progress / total, 0, 1);

        ImGui.ProgressBar(fraction, new Vector2(-1, 0), LOC.Get("TAE_SearchTool_Progress", run.Progress, run.Total));

        GUI.WrappedText(LOC.Get("TAE_SearchTool_Searching", run.Status));

        GUI.MultiButtonInput($"timeActSearchProgressActions_{Id}",
            $"timeActSearchCancel_{Id}",
            LOC.Get("TAE_SearchTool_Cancel"),
            LOC.Get("TAE_SearchTool_Cancel_TT"),
            CancelRun);
    }

    private string GetResultLabel(SearchResult result)
    {
        var label = result.Label;

        if (ShowTimeActInLabel)
            label = $"{result.EntryName}  {label}";

        if (ShowSourceInLabel)
            label = $"{result.Source.Name}  {label}";

        return label;
    }

    private void DisplayResults()
    {
        if (Results == null)
            return;

        GUI.Spacer();
        GUI.SimpleHeader(
            LOC.Get("TAE_SearchTool_Results_Header"),
            LOC.Get("TAE_SearchTool_Results_Header_TT"));

        var filtered = GetFilteredResults();

        var truncated = Run is { Truncated: true };

        var countText = truncated
            ? LOC.Get("TAE_SearchTool_Results_Truncated", MaxResults)
            : LOC.Get("TAE_SearchTool_Results", Results.Count);

        if (filtered.Count != Results.Count)
        {
            countText = $"{countText} {LOC.Get("TAE_SearchTool_Results_Filtered", filtered.Count)}";
        }

        GUI.WrappedText(countText);

        if (Results.Count == 0)
            return;

        EditorFilters.DisplayFramedListFilter($"timeActSearchResultFilter_{Id}", ref ResultFilter, ref ExactResultFilter);

        ImGui.BeginChild($"##timeActSearchResults_{Id}", new Vector2(0, 0), ImGuiChildFlags.Borders);

        var clipper = new ImGuiListClipper();
        clipper.Begin(filtered.Count);

        SearchResult jumpTarget = null;

        var selection = View.Selection;

        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var index = filtered[i];
                var (result, label) = Results[index];

                if (ImGui.Button($"{Icons.LocationArrow}##timeActSearchJump_{Id}{index}"))
                {
                    jumpTarget = result;
                }
                GUI.Tooltip(LOC.Get("TAE_SearchTool_Jump_TT"));

                ImGui.SameLine();

                var selected = selection.SelectedTimeActContainerWrapper == result.Source &&
                    selection.SelectedAnimation == result.Animation &&
                    (result.Event == null || selection.IsEventSelected(result.Event));

                if (ImGui.Selectable($"{label}##timeActSearchResult_{Id}{index}", selected))
                {
                    jumpTarget = result;
                }
            }
        }

        clipper.End();

        ImGui.EndChild();

        if (jumpTarget != null)
        {
            Jump(jumpTarget);
        }
    }

    /// <summary>
    /// The positions of the results that match the filter. The results arrive while the search runs,
    /// so only the new ones are checked, unless the filter has changed.
    /// </summary>
    private List<int> GetFilteredResults()
    {
        if (FilteredResults == null || FilteredResultsFor != Results ||
            CachedResultFilter != ResultFilter || CachedExactResultFilter != ExactResultFilter ||
            CheckedResultCount > Results.Count)
        {
            FilteredResults = new();
            FilteredResultsFor = Results;
            CachedResultFilter = ResultFilter;
            CachedExactResultFilter = ExactResultFilter;
            CheckedResultCount = 0;
        }

        for (int i = CheckedResultCount; i < Results.Count; i++)
        {
            if (EditorFilters.IsMatch(ResultFilter, Results[i].Label, ExactResultFilter))
            {
                FilteredResults.Add(i);
            }
        }

        CheckedResultCount = Results.Count;

        return FilteredResults;
    }

    /// <summary>
    /// Opens the source of a result if it isn't open, then selects its time act, animation and event,
    /// and moves the playback to the start of the event
    /// </summary>
    private void Jump(SearchResult result)
    {
        var selection = View.Selection;
        var source = result.Source;

        if (selection.SelectedTimeActContainerWrapper != source)
        {
            View.SourceWindow.SelectSource(source);
            View.SourceWindow.ScrollTo(source, result.ListType);
        }

        // An unopened source was searched through copies, so what was found is looked up again by its IDs
        var entry = source.TimeActs.Contains(result.Entry)
            ? result.Entry
            : source.TimeActs.FirstOrDefault(e => e.BindIndex == result.BindIndex && e.Name == result.EntryName);

        entry?.Load();

        var animation = entry?.TimeAct == null ? null
            : entry.TimeAct.Animations.Contains(result.Animation) ? result.Animation
            : entry.TimeAct.Animations.FirstOrDefault(e => e.ID == result.AnimationID);

        TAE.Event ev = null;

        if (animation != null && result.EventIndex >= 0)
        {
            if (animation.Events.Contains(result.Event))
            {
                ev = result.Event;
            }
            else if (result.EventIndex < animation.Events.Count && animation.Events[result.EventIndex].Type == result.EventType)
            {
                ev = animation.Events[result.EventIndex];
            }
        }

        // The result may have been removed since, or moved by an edit
        if (animation == null || (result.EventIndex >= 0 && ev == null))
        {
            Smithbox.LogError(this, LOC.Get("TAE_SearchTool_Stale_Result"));
            return;
        }

        if (selection.SelectedTimeAct != entry)
        {
            selection.SelectedTimeAct = entry;
            View.SelectionWindow.ScrollTo(entry);
        }

        if (selection.SelectedAnimation != animation)
        {
            View.AnimationSelectionWindow.SelectAnimation(animation);
        }

        View.AnimationSelectionWindow.ScrollTo(animation);

        if (ev != null)
        {
            selection.SelectedEvent = ev;

            View.Playback.Pause();
            View.Playback.Seek(ev.StartTime);
        }
    }
    #endregion
}
