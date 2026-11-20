using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Collections.Concurrent;
using System.Numerics;
using System.Text.RegularExpressions;

namespace StudioCore.Editors.LuaEditor;

/// <summary>
/// The Global Search tool of the Tools window: finds a string or value in every Lua script of the project, line by
/// line. The results are listed under the script they are in: click a script to select it, a line to show it.
/// Scripts are searched as the editor has them (unsaved edits included). Bytecode is decompiled for the search, in the
/// background, and kept, so the first search over a game's scripts is slow and the later ones are quick.
/// </summary>
public class LuaGlobalSearch
{
    private readonly LuaEditorScreen Editor;

    // The results stop here, as a short string such as "0" is found on nearly every line
    private const int MaxResults = 10000;

    // How much of a line is kept to show, either side of the match
    private const int PreviewContext = 80;

    private string SearchText = "";
    private string BinderText = "";
    private bool MatchCase = false;
    private bool WholeWord = false;
    private bool UseRegex = false;
    private bool IncludeBattle = true;
    private bool IncludeLogic = true;
    private bool IncludeCommon = true;
    private bool IncludeOther = true;

    private string Error;

    private SearchRun Run;

    // The matched lines, in the order found, and the scripts they are in (in the same order)
    private List<LineMatch> Matches;
    private List<ScriptGroup> Groups;
    private Dictionary<LuaScript, ScriptGroup> GroupOf;

    // Scripts whose lines are hidden in the list
    private readonly HashSet<LuaScript> Collapsed = new();

    private string ResultFilter = "";
    private bool ExactResultFilter = false;

    // The rows the list shows: rebuilt when the results, filter or collapsed scripts change
    private List<Row> Rows;
    private bool RowsDirty = true;
    private string CachedFilter = "";
    private bool CachedExact = false;

    public LuaGlobalSearch(LuaEditorScreen editor)
    {
        Editor = editor;
    }

    private record LineMatch(LuaScript Script, int Line, string Preview, int MatchStart, int MatchLength, int Count);

    private class ScriptGroup
    {
        public LuaScript Script;
        public string Label;
        public List<LineMatch> Lines = new();
        public int Hits;
    }

    // A script's row (Match null) or one of its lines
    private record struct Row(ScriptGroup Group, LineMatch Match);

    private class SearchRun
    {
        public readonly CancellationTokenSource Cancellation = new();
        public readonly ConcurrentQueue<LineMatch> Found = new();
        public volatile int Progress;
        public volatile int Total;
        public volatile string Status = "";
        public volatile bool Done;
        public volatile bool Truncated;
        public int Count;
    }

    public void OnToolWindow()
    {
        if (RevealHeader)
        {
            RevealHeader = false;
            ToolSections.Focus();
        }

        if (!ToolSections.Header($"{LOC.Get("LUA_Tool_Global_Search")}##luaGlobalSearchTool"))
            return;

        ImGui.BeginChild("LuaGlobalSearchToolSection", ImGuiChildFlags.Borders);
        Display();
        ImGui.EndChild();
    }

    // Opens the tool's header on the next frame (after a search from the script view)
    private bool RevealHeader;

    /// <summary>Searches for a token from the script view's context menu: as a whole word, as written</summary>
    public void SearchFor(string text, bool wholeWord)
    {
        SearchText = text ?? "";
        WholeWord = wholeWord;
        MatchCase = true;
        UseRegex = false;
        RevealHeader = true;
        StartSearch();
    }

    private void Display()
    {
        GUI.WrappedText(LOC.Get("LUA_GlobalSearch_Hint"));

        if (DisplayInputs())
            StartSearch();

        if (Error != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, Error);

        DisplayProgress();
        DisplayResults();
    }

    #region Inputs
    private bool DisplayInputs()
    {
        var search = false;

        search |= InputSection("LUA_GlobalSearch_Text", "##luaGlobalSearchText", ref SearchText);
        search |= InputSection("LUA_GlobalSearch_Binder", "##luaGlobalSearchBinder", ref BinderText);

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("LUA_GlobalSearch_Options_Header"), LOC.Get("LUA_GlobalSearch_Options_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("LUA_GlobalSearch_Match_Case")}##luaGlobalSearchCase", ref MatchCase);
        GUI.Tooltip(LOC.Get("LUA_GlobalSearch_Match_Case_TT"));
        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("LUA_GlobalSearch_Whole_Word")}##luaGlobalSearchWord", ref WholeWord);
        GUI.Tooltip(LOC.Get("LUA_GlobalSearch_Whole_Word_TT"));
        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("LUA_GlobalSearch_Regex")}##luaGlobalSearchRegex", ref UseRegex);
        GUI.Tooltip(LOC.Get("LUA_GlobalSearch_Regex_TT"));

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("LUA_GlobalSearch_Kinds_Header"), LOC.Get("LUA_GlobalSearch_Kinds_Header_TT"));

        ImGui.Checkbox($"{LOC.Get("LUA_FileList_Kind_Battle")}##luaGlobalSearchBattle", ref IncludeBattle);
        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("LUA_FileList_Kind_Logic")}##luaGlobalSearchLogic", ref IncludeLogic);
        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("LUA_FileList_Kind_Common")}##luaGlobalSearchCommon", ref IncludeCommon);
        ImGui.SameLine();
        ImGui.Checkbox($"{LOC.Get("LUA_FileList_Kind_Other")}##luaGlobalSearchOther", ref IncludeOther);

        var hasQuery = SearchText.Length > 0;

        GUI.Spacer();
        GUI.ConditionalMultiButtonInput("luaGlobalSearchActions",
            "luaGlobalSearchButton",
            LOC.Get("LUA_GlobalSearch_Search"),
            LOC.Get("LUA_GlobalSearch_Search_TT"),
            () => search = true,
            hasQuery,

            "luaGlobalSearchClear",
            LOC.Get("LUA_GlobalSearch_Clear"),
            LOC.Get("LUA_GlobalSearch_Clear_TT"),
            ClearAll,
            true);

        return search && hasQuery;
    }

    /// <summary>
    /// A titled text input. True when the field is left after an edit (by Enter, Tab or a click, not Escape), so a
    /// search can be run without the button.
    /// </summary>
    private static bool InputSection(string nameKey, string id, ref string text)
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get(nameKey), LOC.Get($"{nameKey}_TT"));

        ImGui.SetNextItemWidth(-1);
        ImGui.InputText(id, ref text, 255);
        GUI.Tooltip(LOC.Get($"{nameKey}_TT"));

        return GUI.InputCommitted();
    }

    private void ClearAll()
    {
        SearchText = "";
        BinderText = "";
        ClearResults();
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
        Error = null;
        Matches = null;
        Groups = null;
        GroupOf = null;
        Collapsed.Clear();
        RowsDirty = true;
    }

    /// <summary>The search as a regex: the text escaped unless it is one, between word boundaries if asked</summary>
    private Regex BuildPattern()
    {
        var pattern = UseRegex ? SearchText : Regex.Escape(SearchText);
        // Not \b: a search that starts or ends with a symbol (e.g. "= 3") would never match at a word boundary
        if (WholeWord)
            pattern = $"(?<![A-Za-z0-9_]){pattern}(?![A-Za-z0-9_])";

        var options = RegexOptions.CultureInvariant;
        if (!MatchCase)
            options |= RegexOptions.IgnoreCase;

        return new Regex(pattern, options, TimeSpan.FromMilliseconds(250));
    }

    /// <summary>Gathers the scripts' binders on the main thread, then searches them in the background</summary>
    private void StartSearch()
    {
        ClearResults();

        var bank = Editor.Bank;
        if (bank == null || SearchText.Length == 0)
            return;

        // Any pending edit in the text box is searched too
        Editor.ScriptView.CommitPending();

        Regex regex;
        try
        {
            regex = BuildPattern();
        }
        catch (ArgumentException e)
        {
            Error = LOC.Get("LUA_GlobalSearch_Bad_Regex", e.Message);
            return;
        }

        var binder = BinderText.Trim();
        var containers = bank.Containers
            .Where(c => IncludeKind(c.Kind))
            .Where(c => binder.Length == 0 || c.Name.Contains(binder, StringComparison.OrdinalIgnoreCase))
            .ToList();

        Matches = new();
        Groups = new();
        GroupOf = new();

        var run = new SearchRun { Total = containers.Count };
        Run = run;
        var token = run.Cancellation.Token;

        // One thread: the decompiler is only known to be safe from one at a time (the definition index uses it too,
        // under each script's own lock)
        Task.Run(() =>
        {
            try
            {
                foreach (var container in containers)
                {
                    if (token.IsCancellationRequested || run.Truncated)
                        break;

                    run.Status = container.Name;

                    if (container.EnsureLoaded())
                    {
                        foreach (var script in container.Scripts)
                        {
                            if (token.IsCancellationRequested || run.Truncated)
                                break;
                            SearchScript(run, script, regex);
                        }
                    }

                    run.Progress++;
                }
            }
            catch (Exception e)
            {
                Smithbox.LogError(this, LOC.Get("LUA_GlobalSearch_Failed"), e);
            }
            finally
            {
                run.Done = true;
            }
        }, token);
    }

    private bool IncludeKind(LuaBinderKind kind) => kind switch
    {
        LuaBinderKind.Battle => IncludeBattle,
        LuaBinderKind.Logic => IncludeLogic,
        LuaBinderKind.Common => IncludeCommon,
        _ => IncludeOther,
    };

    private static void SearchScript(SearchRun run, LuaScript script, Regex regex)
    {
        var text = script.Text;
        if (text == null)
            return;

        var line = 0;
        var start = 0;
        while (start <= text.Length)
        {
            line++;
            var end = text.IndexOf('\n', start);
            if (end < 0)
                end = text.Length;
            var length = end - start;
            if (length > 0 && text[end - 1] == '\r')
                length--;

            var lineText = text.Substring(start, length);
            MatchCollection found;
            try
            {
                // Count runs the matching (the collection is lazy), so a timeout is thrown here
                found = regex.Matches(lineText);
                _ = found.Count;
            }
            catch (RegexMatchTimeoutException)
            {
                found = null;
            }

            if (found is { Count: > 0 })
            {
                var first = found[0];
                var (preview, matchStart) = Preview(lineText, first.Index, first.Length);
                run.Found.Enqueue(new LineMatch(script, line, preview, matchStart, first.Length, found.Count));

                if (Interlocked.Increment(ref run.Count) >= MaxResults)
                {
                    run.Truncated = true;
                    return;
                }
            }

            start = end + 1;
        }
    }

    /// <summary>The line without its indent, cut to some context either side of the match; and where the match is in it</summary>
    private static (string Text, int MatchStart) Preview(string line, int index, int length)
    {
        var from = 0;
        while (from < index && char.IsWhiteSpace(line[from]))
            from++;
        var prefix = "";
        if (index - from > PreviewContext)
        {
            from = index - PreviewContext;
            prefix = "...";
        }

        var to = line.Length;
        var suffix = "";
        if (to - (index + length) > PreviewContext)
        {
            to = index + length + PreviewContext;
            suffix = "...";
        }

        return (prefix + line[from..to] + suffix, index - from + prefix.Length);
    }
    #endregion

    #region Results
    /// <summary>Picks up what the search has found so far, and shows how far it has got</summary>
    private void DisplayProgress()
    {
        var run = Run;
        if (run == null)
            return;

        while (run.Found.TryDequeue(out var match))
            AddMatch(match);

        if (run.Done)
            return;

        GUI.Spacer();
        var total = Math.Max(run.Total, 1);
        var fraction = Math.Clamp((float)run.Progress / total, 0, 1);
        ImGui.ProgressBar(fraction, new Vector2(-1, 0), LOC.Get("LUA_GlobalSearch_Progress", run.Progress, run.Total));
        GUI.WrappedText(LOC.Get("LUA_GlobalSearch_Searching", run.Status));

        GUI.MultiButtonInput("luaGlobalSearchProgressActions",
            "luaGlobalSearchCancel",
            LOC.Get("LUA_GlobalSearch_Cancel"),
            LOC.Get("LUA_GlobalSearch_Cancel_TT"),
            CancelRun);
    }

    private void AddMatch(LineMatch match)
    {
        if (Matches == null)
            return;

        Matches.Add(match);
        if (!GroupOf.TryGetValue(match.Script, out var group))
        {
            var container = match.Script.Container;
            // A binder with one script is named by the binder alone
            var label = container.Scripts.Count == 1 ? container.Name : $"{container.Name} / {match.Script.EntryName}";
            group = new ScriptGroup { Script = match.Script, Label = label };
            GroupOf[match.Script] = group;
            Groups.Add(group);
        }
        group.Lines.Add(match);
        group.Hits += match.Count;
        RowsDirty = true;
    }

    private void DisplayResults()
    {
        if (Matches == null)
            return;

        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("LUA_GlobalSearch_Results_Header"), LOC.Get("LUA_GlobalSearch_Results_Header_TT"));

        var countText = Run is { Truncated: true }
            ? LOC.Get("LUA_GlobalSearch_Results_Truncated", MaxResults)
            : LOC.Get("LUA_GlobalSearch_Results", Matches.Count, Groups.Count);
        GUI.WrappedText(countText);

        if (Matches.Count == 0)
            return;

        GUI.MultiButtonInput("luaGlobalSearchFoldActions",
            "luaGlobalSearchCollapseAll",
            LOC.Get("LUA_GlobalSearch_Collapse_All"),
            LOC.Get("LUA_GlobalSearch_Collapse_All_TT"),
            () => { foreach (var g in Groups) Collapsed.Add(g.Script); RowsDirty = true; },

            "luaGlobalSearchExpandAll",
            LOC.Get("LUA_GlobalSearch_Expand_All"),
            LOC.Get("LUA_GlobalSearch_Expand_All_TT"),
            () => { Collapsed.Clear(); RowsDirty = true; });

        EditorFilters.DisplayFramedListFilter("luaGlobalSearchResultFilter", ref ResultFilter, ref ExactResultFilter);

        var rows = GetRows();

        ImGui.BeginChild("##luaGlobalSearchResults", Vector2.Zero, ImGuiChildFlags.Borders);

        Row? clicked = null;

        var clipper = new ImGuiListClipper();
        clipper.Begin(rows.Count);
        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
            {
                var row = rows[i];
                if (row.Match == null)
                {
                    if (DrawScriptRow(row.Group, i))
                        clicked = row;
                }
                else if (DrawLineRow(row.Match, i))
                    clicked = row;
            }
        }
        clipper.End();

        ImGui.EndChild();

        if (clicked is { } c)
        {
            if (c.Match != null)
                Editor.ShowLine(c.Match.Script, c.Match.Line);
            else
            {
                Editor.SelectScript(c.Group.Script);
                Editor.FileList.ScrollToSelection = true;
            }
        }
    }

    /// <summary>A script's row: its fold arrow, its name and how often it matched. True when its name is clicked.</summary>
    private bool DrawScriptRow(ScriptGroup group, int index)
    {
        var collapsed = Collapsed.Contains(group.Script);
        if (ImGui.ArrowButton($"##luaGlobalSearchFold_{index}", collapsed ? ImGuiDir.Right : ImGuiDir.Down))
        {
            if (!Collapsed.Remove(group.Script))
                Collapsed.Add(group.Script);
            RowsDirty = true;
        }
        ImGui.SameLine();

        var selected = Editor.SelectedScript == group.Script;
        var label = group.Script.IsDirty ? $"{group.Label}*" : group.Label;
        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AliasName_Text);
        var clicked = ImGui.Selectable($"{Icons.File} {label}##luaGlobalSearchScript_{index}", selected);
        ImGui.PopStyleColor();
        GUI.Tooltip(LOC.Get("LUA_GlobalSearch_Script_TT"));

        ImGui.SameLine();
        ImGui.TextDisabled(LOC.Get("LUA_GlobalSearch_Script_Hits", group.Lines.Count, group.Hits));
        return clicked;
    }

    /// <summary>A matched line: its number and the text around the match, the match picked out. True when clicked.</summary>
    private bool DrawLineRow(LineMatch match, int index)
    {
        var indent = ImGui.GetFrameHeight() + ImGui.GetStyle().ItemSpacing.X;
        ImGui.SetCursorPosX(ImGui.GetCursorPosX() + indent);

        var start = ImGui.GetCursorPosX();
        var clicked = ImGui.Selectable($"##luaGlobalSearchLine_{index}", false, ImGuiSelectableFlags.AllowOverlap);
        if (ImGui.IsItemHovered())
            GUI.Tooltip(LOC.Get("LUA_GlobalSearch_Line_TT", match.Line, match.Count));

        ImGui.SameLine();
        ImGui.SetCursorPosX(start);

        ImGui.TextDisabled($"{match.Line,6}:");
        ImGui.SameLine();

        var text = match.Preview;
        var a = Math.Clamp(match.MatchStart, 0, text.Length);
        var b = Math.Clamp(match.MatchStart + match.MatchLength, a, text.Length);

        if (a > 0)
        {
            ImGui.TextUnformatted(text[..a]);
            ImGui.SameLine(0, 0);
        }
        ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, b > a ? text[a..b] : "");
        if (b < text.Length)
        {
            ImGui.SameLine(0, 0);
            ImGui.TextUnformatted(text[b..]);
        }

        return clicked;
    }

    /// <summary>The rows to list: each script that matches the filter (by its name or a line), then its lines unless folded</summary>
    private List<Row> GetRows()
    {
        if (!RowsDirty && Rows != null && CachedFilter == ResultFilter && CachedExact == ExactResultFilter)
            return Rows;

        RowsDirty = false;
        CachedFilter = ResultFilter;
        CachedExact = ExactResultFilter;

        var rows = new List<Row>();
        var filtering = !string.IsNullOrEmpty(ResultFilter);

        foreach (var group in Groups)
        {
            var nameMatches = !filtering || EditorFilters.IsMatch(ResultFilter, group.Label, ExactResultFilter);
            var lines = nameMatches
                ? group.Lines
                : group.Lines.Where(l => EditorFilters.IsMatch(ResultFilter, l.Preview, ExactResultFilter)).ToList();
            if (lines.Count == 0)
                continue;

            rows.Add(new Row(group, null));
            if (!Collapsed.Contains(group.Script))
                foreach (var l in lines)
                    rows.Add(new Row(group, l));
        }

        Rows = rows;
        return rows;
    }
    #endregion
}
