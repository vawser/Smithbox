using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Memory;
using StudioCore.Utilities;
using System.Globalization;
using System.Numerics;

namespace StudioCore.Editors.Common;

/// <summary>A search for a flag in the host editor's data, offered from a row (the first one also on double-click)</summary>
public record EventFlagSearch(string LabelKey, Action<long> Go);

/// <summary>
/// Event Flags (ER): the running game's event flags, ON or OFF, newest change first. Reads the game while the tool is
/// shown. Hosted in the Game Memory category of the EMEVD, ESD and Param editors' Tools windows; every instance shares
/// the project's EventFlagMonitor (one connection, one history). A flag can be set ON or OFF in the game (confirmed
/// once per session). See Claude/Documentation/Event Flag Viewer.md.
/// </summary>
public class EventFlagTool
{
    private readonly ProjectEntry Project;
    private readonly string Id;
    private readonly EventFlagSearch[] Searches;

    /// <summary>What clicking a row does: double-click runs the first search, or a click toggles the flag</summary>
    public enum RowClick { Search, Toggle }

    /// <summary>Set by the host; Search by default</summary>
    public RowClick Click = RowClick.Search;

    private enum FlagView { Changed, Log, On, Named, Range }

    private static readonly double[] PollSeconds = { 0.25, 0.5, 1, 2 };
    private static int PollIndex = 0;

    /// <summary>Most rows a view lists (Range and On can be large)</summary>
    private const int RowLimit = 200000;

    private FlagView CurrentView = FlagView.Changed;
    private string Filter = "";

    private record struct Row(EventFlagMonitor.ChangeKind Kind, long Flag, bool? OnAtChange, DateTime? At, int Count, bool Written = false);

    // Change a flag
    private string WriteIdText = "";

    // Writing changes the loaded character: confirmed once per session (shared by every view)
    private static bool WriteConfirmed;
    private bool OpenConfirm;
    private (long Flag, bool On) ConfirmRequest;

    private List<Row> Rows = new();
    private bool RowsTruncated;
    private int BuiltVersion = -1;
    private FlagView BuiltView;
    private string BuiltFilter;
    private DateTime BuiltAt;
    private int BuiltAliasCount = -1;

    private Dictionary<long, string> Names = new();

    /// <param name="id">a prefix for the ImGui IDs, unique per host ("emevd", "esd", "param")</param>
    public EventFlagTool(ProjectEntry project, string id, params EventFlagSearch[] searches)
    {
        Project = project;
        Id = id;
        Searches = searches;
    }

    private EventFlagMonitor Monitor => Project.Handler.EventFlagMonitor;

    public void OnToolWindow()
    {
        if (!ToolSections.Header($"{LOC.Get("EVENTFLAG_Tool_Header")}##{Id}EventFlagToolHeader"))
            return;

        ImGui.BeginChild($"{Id}EventFlagToolSection", ImGuiChildFlags.Borders);

        Display();
        ConfirmModal();

        ImGui.EndChild();
    }

    private void Display()
    {
        if (!Monitor.IsSupported)
        {
            GUI.WrappedText(LOC.Get("EVENTFLAG_Tool_Not_Supported"));
            return;
        }

        GUI.WrappedText(LOC.Get(Click == RowClick.Toggle ? "EVENTFLAG_Tool_Hint_Toggle" : "EVENTFLAG_Tool_Hint"));

        // Reads the game only while the tool is shown
        if (!Monitor.Paused && (Monitor.LastPoll == null ||
            (DateTime.Now - Monitor.LastPoll.Value).TotalSeconds >= PollSeconds[PollIndex]))
        {
            Monitor.RequestPoll();
        }

        DisplayGame();
        DisplayWrite();
        DisplayFlags();
    }

    #region Game

    private void DisplayGame()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("EVENTFLAG_Tool_Game_Header"), LOC.Get("EVENTFLAG_Tool_Game_Header_TT"));

        var message = Monitor.Paused ? LOC.Get("EVENTFLAG_Tool_Paused") : Monitor.StateMessage;
        if (string.IsNullOrEmpty(message))
            message = LOC.Get("GAMELINK_State_Waiting");

        if (Monitor.State is EventFlagMonitor.MonitorState.Failed or EventFlagMonitor.MonitorState.NotFound)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, message);
        else
            GUI.WrappedText(message);

        if (Monitor.WatchingSince is DateTime since)
            GUI.WrappedText(LOC.Get("EVENTFLAG_Tool_Watching_Since", since.ToString("HH:mm:ss", CultureInfo.InvariantCulture)));

        if (Monitor.ChunkCount > 0)
            GUI.WrappedText(LOC.Get("EVENTFLAG_Tool_Layout", Monitor.ChunkCount * 1000L));

        if (Monitor.SkippedChunks > 0)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("EVENTFLAG_Tool_Skipped_Chunks", Monitor.SkippedChunks));

        if (Monitor.UnknownSlotChanges > 0)
            GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, LOC.Get("EVENTFLAG_Tool_Unknown_Slots", Monitor.UnknownSlotChanges));

        // Poll interval
        ImGui.SetNextItemWidth(120f * DPI.UIScale());
        if (ImGui.BeginCombo($"{LOC.Get("EVENTFLAG_Tool_Interval")}##eventFlagInterval", LOC.Get("EVENTFLAG_Tool_Interval_Value", PollSeconds[PollIndex])))
        {
            for (var i = 0; i < PollSeconds.Length; i++)
            {
                if (ImGui.Selectable(LOC.Get("EVENTFLAG_Tool_Interval_Value", PollSeconds[i]), i == PollIndex))
                    PollIndex = i;
            }
            ImGui.EndCombo();
        }
        GUI.Tooltip(LOC.Get("EVENTFLAG_Tool_Interval_TT"));

        GUI.MultiButtonInput("eventFlagActions",
            "eventFlagPause", LOC.Get(Monitor.Paused ? "EVENTFLAG_Tool_Resume" : "EVENTFLAG_Tool_Pause"),
            LOC.Get(Monitor.Paused ? "EVENTFLAG_Tool_Resume_TT" : "EVENTFLAG_Tool_Pause_TT"),
            () => Monitor.Paused = !Monitor.Paused,
            "eventFlagBaseline", LOC.Get("EVENTFLAG_Tool_Reset_Baseline"), LOC.Get("EVENTFLAG_Tool_Reset_Baseline_TT"),
            Monitor.ResetBaseline,
            "eventFlagClear", LOC.Get("EVENTFLAG_Tool_Clear"), LOC.Get("EVENTFLAG_Tool_Clear_TT"),
            Monitor.ClearHistory,
            "eventFlagDetach", LOC.Get("EVENTFLAG_Tool_Detach"), LOC.Get("EVENTFLAG_Tool_Detach_TT"),
            Monitor.Detach);
    }

    #endregion

    #region Change a flag

    private void DisplayWrite()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("EVENTFLAG_Tool_Write_Header"), LOC.Get("EVENTFLAG_Tool_Write_Header_TT"));

        // Applied by the buttons only: leaving the field does nothing
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##eventFlagWriteId", LOC.Get("EVENTFLAG_Tool_Write_Id_Hint"), ref WriteIdText, 12, ImGuiInputTextFlags.CharsDecimal);
        GUI.Tooltip(LOC.Get("EVENTFLAG_Tool_Write_Id_TT"));

        var hasId = long.TryParse(WriteIdText.Trim(), NumberStyles.None, CultureInfo.InvariantCulture, out var id) && id > 0;
        string problem = null;
        if (hasId)
        {
            // The game's own tables when read; else Smithbox's model (ER)
            var stored = Monitor.IsStored(id) ?? EventFlagDisplay.Info(Project, id)?.IsValid;
            if (stored == false)
                problem = LOC.Get("EVENTFLAG_Write_Not_Stored", id);
            else
            {
                var on = Monitor.IsOn(id);
                GUI.WrappedText(LOC.Get("EVENTFLAG_Tool_Write_Current", on == true ? LOC.Get("EVENTFLAG_Tool_On") : on == false ? LOC.Get("EVENTFLAG_Tool_Off") : "-"));
            }
        }

        if (problem != null)
            GUI.WrappedTextColored(UI.Current.ImGui_Warning_Text_Color, problem);

        var canWrite = hasId && problem == null && Monitor.IsWatching && !Monitor.IsWriting;

        GUI.ConditionalMultiButtonInput("eventFlagWriteActions",
            "eventFlagWriteOn", LOC.Get("EVENTFLAG_Tool_Set_On"), LOC.Get("EVENTFLAG_Tool_Set_On_TT"),
            () => RequestWrite(id, true), canWrite,
            "eventFlagWriteOff", LOC.Get("EVENTFLAG_Tool_Set_Off"), LOC.Get("EVENTFLAG_Tool_Set_Off_TT"),
            () => RequestWrite(id, false), canWrite);

        if (!string.IsNullOrEmpty(Monitor.LastWrite))
        {
            if (Monitor.LastWriteFailed)
                GUI.WrappedTextColored(UI.Current.ImGui_Invalid_Text_Color, Monitor.LastWrite);
            else
                GUI.WrappedText(Monitor.LastWrite);
        }
    }

    private void RequestWrite(long flag, bool on)
    {
        if (WriteConfirmed)
        {
            Monitor.RequestWrite(flag, on);
            return;
        }

        ConfirmRequest = (flag, on);
        OpenConfirm = true;
    }

    private void ConfirmModal()
    {
        const string title = "###eventFlagWriteConfirm";
        if (OpenConfirm)
        {
            ImGui.OpenPopup($"{LOC.Get("EVENTFLAG_Tool_Confirm_Title")}{title}");
            OpenConfirm = false;
        }

        ImGui.SetNextWindowSize(new Vector2(420, 0) * DPI.UIScale(), ImGuiCond.Always);
        var open = true;
        if (!ImGui.BeginPopupModal($"{LOC.Get("EVENTFLAG_Tool_Confirm_Title")}{title}", ref open, ImGuiWindowFlags.AlwaysAutoResize))
            return;

        var (flag, on) = ConfirmRequest;
        GUI.WrappedText(LOC.Get("EVENTFLAG_Tool_Confirm_Text", flag, LOC.Get(on ? "EVENTFLAG_Tool_On" : "EVENTFLAG_Tool_Off")));

        GUI.Spacer();
        GUI.MultiButtonInput("eventFlagConfirmActions",
            "eventFlagConfirmChange", LOC.Get("EVENTFLAG_Tool_Confirm_Change"), LOC.Get("EVENTFLAG_Tool_Confirm_Change_TT"),
            () =>
            {
                WriteConfirmed = true;
                Monitor.RequestWrite(flag, on);
                ImGui.CloseCurrentPopup();
            },
            "eventFlagConfirmCancel", LOC.Get("EVENTFLAG_Tool_Confirm_Cancel"), LOC.Get("EVENTFLAG_Tool_Confirm_Cancel_TT"),
            ImGui.CloseCurrentPopup);

        ImGui.EndPopup();
    }

    #endregion

    #region Flags

    private void DisplayFlags()
    {
        GUI.Spacer();
        GUI.SimpleHeader(LOC.Get("EVENTFLAG_Tool_Flags_Header"), LOC.Get("EVENTFLAG_Tool_Flags_Header_TT"));

        ViewButton(FlagView.Changed, "EVENTFLAG_Tool_View_Changed");
        ImGui.SameLine();
        ViewButton(FlagView.Log, "EVENTFLAG_Tool_View_Log");
        ImGui.SameLine();
        ViewButton(FlagView.On, "EVENTFLAG_Tool_View_On");
        ImGui.SameLine();
        ViewButton(FlagView.Named, "EVENTFLAG_Tool_View_Named");
        ImGui.SameLine();
        ViewButton(FlagView.Range, "EVENTFLAG_Tool_View_Range");

        // A filter narrows the list as it is typed (nothing is applied or saved)
        ImGui.SetNextItemWidth(-1);
        ImGui.InputTextWithHint("##eventFlagFilter", LOC.Get("EVENTFLAG_Tool_Filter_Hint"), ref Filter, 128);
        GUI.Tooltip(LOC.Get("EVENTFLAG_Tool_Filter_TT"));

        RebuildRows();

        if (CurrentView == FlagView.Range && ParseFilter(Filter).Range == null)
        {
            GUI.WrappedText(LOC.Get("EVENTFLAG_Tool_Range_Needed"));
            return;
        }

        if (!Monitor.IsWatching && CurrentView is FlagView.On)
        {
            GUI.WrappedText(LOC.Get("EVENTFLAG_Tool_Not_Watching"));
            return;
        }

        GUI.WrappedText(LOC.Get(RowsTruncated ? "EVENTFLAG_Tool_Count_Truncated" : "EVENTFLAG_Tool_Count", Rows.Count));

        DisplayTable();
    }

    private void ViewButton(FlagView view, string key)
    {
        if (ImGui.RadioButton($"{LOC.Get(key)}##eventFlagView{view}", CurrentView == view))
            CurrentView = view;
        GUI.Tooltip(LOC.Get(key + "_TT"));
    }

    private void DisplayTable()
    {
        var height = Math.Max(250f * DPI.UIScale(), ImGui.GetContentRegionAvail().Y);
        var flags = ImGuiTableFlags.ScrollY | ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.Resizable;

        if (!ImGui.BeginTable("eventFlagTable", 5, flags, new Vector2(0, height)))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn(LOC.Get("EVENTFLAG_Tool_Column_Changed"), ImGuiTableColumnFlags.WidthFixed, 80f * DPI.UIScale());
        ImGui.TableSetupColumn(LOC.Get("EVENTFLAG_Tool_Column_Flag"), ImGuiTableColumnFlags.WidthFixed, 90f * DPI.UIScale());
        ImGui.TableSetupColumn(LOC.Get("EVENTFLAG_Tool_Column_State"), ImGuiTableColumnFlags.WidthFixed, 50f * DPI.UIScale());
        ImGui.TableSetupColumn(LOC.Get("EVENTFLAG_Tool_Column_Name"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn(LOC.Get("EVENTFLAG_Tool_Column_Range"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableHeadersRow();

        var now = DateTime.Now;
        var clipper = new ImGuiListClipper();
        clipper.Begin(Rows.Count);

        while (clipper.Step())
        {
            for (var i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                DisplayRow(i, Rows[i], now);
        }

        clipper.End();
        ImGui.EndTable();
    }

    private void DisplayRow(int index, Row row, DateTime now)
    {
        ImGui.TableNextRow();

        if (row.Kind != EventFlagMonitor.ChangeKind.Flag)
        {
            // Baseline / save loaded marker
            ImGui.TableSetColumnIndex(0);
            ImGui.TextDisabled(row.At is DateTime markerAt ? Ago(markerAt, now) : "");
            ImGui.TableSetColumnIndex(3);
            ImGui.TextDisabled(LOC.Get(row.Kind == EventFlagMonitor.ChangeKind.Load ? "EVENTFLAG_Tool_Marker_Load" : "EVENTFLAG_Tool_Marker_Baseline"));
            return;
        }

        var flag = row.Flag;

        // Changed
        ImGui.TableSetColumnIndex(0);
        var changed = row.At is DateTime at ? Ago(at, now) : "";
        if (row.Written)
            changed = LOC.Get("EVENTFLAG_Tool_Written", changed);
        var clicked = ImGui.Selectable($"{changed}##eventFlagRow{index}", false,
            ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowOverlap);
        if (Click == RowClick.Toggle)
        {
            // One click, one toggle: the second click of a double-click is ignored
            var value = Monitor.IsOn(flag);
            if (clicked && !ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left) && value != null && !Monitor.IsWriting)
                RequestWrite(flag, value != true);
        }
        else if (Searches.Length > 0 && ImGui.IsItemHovered() && ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left))
        {
            Searches[0].Go(flag);
        }
        if (row.At is DateTime atTip)
            GUI.Tooltip(LOC.Get(row.Count > 1 ? "EVENTFLAG_Tool_Changed_TT_Count" : "EVENTFLAG_Tool_Changed_TT",
                atTip.ToString("HH:mm:ss.f", CultureInfo.InvariantCulture), row.Count));

        if (ImGui.BeginPopupContextItem($"eventFlagRowContext{index}"))
        {
            if (ImGui.Selectable(LOC.Get("EVENTFLAG_Tool_Copy_Id")))
                PlatformUtils.Instance.SetClipboardText(flag.ToString(CultureInfo.InvariantCulture));
            foreach (var search in Searches)
            {
                if (ImGui.Selectable(LOC.Get(search.LabelKey)))
                    search.Go(flag);
            }

            ImGui.Separator();
            var current = Monitor.IsOn(flag);
            var canWrite = current != null && !Monitor.IsWriting;
            if (ImGui.Selectable(LOC.Get("EVENTFLAG_Tool_Set_On"), false, canWrite && current == false ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
                RequestWrite(flag, true);
            if (ImGui.Selectable(LOC.Get("EVENTFLAG_Tool_Set_Off"), false, canWrite && current == true ? ImGuiSelectableFlags.None : ImGuiSelectableFlags.Disabled))
                RequestWrite(flag, false);
            ImGui.EndPopup();
        }

        // Flag
        ImGui.TableSetColumnIndex(1);
        ImGui.TextUnformatted(flag.ToString(CultureInfo.InvariantCulture));

        // State: the value at the change in the log, else the latest read
        ImGui.TableSetColumnIndex(2);
        var on = CurrentView == FlagView.Log ? row.OnAtChange : Monitor.IsOn(flag);
        if (on == true)
            ImGui.TextColored(UI.Current.ImGui_Benefit_Text_Color, LOC.Get("EVENTFLAG_Tool_On"));
        else if (on == false)
            ImGui.TextDisabled(LOC.Get("EVENTFLAG_Tool_Off"));
        else
            ImGui.TextDisabled("-");

        // Name
        ImGui.TableSetColumnIndex(3);
        if (Names.TryGetValue(flag, out var name))
            ImGui.TextUnformatted(name);

        // Range · usage
        ImGui.TableSetColumnIndex(4);
        var summary = EventFlagDisplay.Summary(Project, flag);
        if (summary != null)
        {
            ImGui.TextDisabled(summary);
            var info = EventFlagDisplay.Info(Project, flag);
            if (info != null)
                GUI.Tooltip(EventFlagDisplay.Tooltip(info));
        }
    }

    private static string Ago(DateTime at, DateTime now)
    {
        var s = (now - at).TotalSeconds;
        if (s < 60)
            return LOC.Get("EVENTFLAG_Tool_Ago_Seconds", (int)s);
        if (s < 3600)
            return LOC.Get("EVENTFLAG_Tool_Ago_Minutes", (int)(s / 60));
        return at.ToString("HH:mm:ss", CultureInfo.InvariantCulture);
    }

    #endregion

    #region Rows

    private record struct ParsedFilter((long First, long Last)? Range, string Digits, string Text);

    /// <summary>"1000-1999" = a range; a number = its block of 1000 in Range, else IDs containing it; other text = name / range text</summary>
    private static ParsedFilter ParseFilter(string filter)
    {
        var f = filter.Trim();
        if (f.Length == 0)
            return new(null, null, null);

        var dash = f.IndexOf('-', 1);
        if (dash > 0 && long.TryParse(f[..dash].Trim(), out var a) && long.TryParse(f[(dash + 1)..].Trim(), out var b))
            return new((Math.Min(a, b), Math.Max(a, b)), null, null);

        if (long.TryParse(f, NumberStyles.None, CultureInfo.InvariantCulture, out var n))
            return new((n / 1000 * 1000, n / 1000 * 1000 + 999), f, null);

        return new(null, null, f);
    }

    private void RebuildRows()
    {
        UpdateNames();

        var version = Monitor.Version;
        var viewChanged = BuiltView != CurrentView || BuiltFilter != Filter || BuiltAliasCount < 0;
        if (!viewChanged && version == BuiltVersion)
            return;

        // Reading every block (On / Range) is throttled while flags keep changing
        if (!viewChanged && CurrentView is FlagView.On or FlagView.Range && (DateTime.Now - BuiltAt).TotalSeconds < 1)
            return;

        BuiltVersion = version;
        BuiltView = CurrentView;
        BuiltFilter = Filter;
        BuiltAt = DateTime.Now;

        var parsed = ParseFilter(Filter);
        var rows = new List<Row>();

        switch (CurrentView)
        {
            case FlagView.Changed:
                foreach (var c in Monitor.RecentChanges())
                    rows.Add(new Row(c.Kind, c.Flag, c.On, c.At, c.Count, c.Written));
                break;

            case FlagView.Log:
                foreach (var c in Monitor.ChangeLog())
                    rows.Add(new Row(c.Kind, c.Flag, c.On, c.At, c.Count, c.Written));
                break;

            case FlagView.On:
                // An explicit range limits the read; a plain number narrows by digits below
                var onRange = parsed.Digits == null ? parsed.Range : null;
                foreach (var flag in Monitor.OnFlags(onRange?.First ?? 0, onRange?.Last ?? long.MaxValue))
                    rows.Add(WithChange(flag));
                parsed = parsed with { Range = null };
                break;

            case FlagView.Named:
                foreach (var flag in Names.Keys.OrderBy(k => k))
                    rows.Add(WithChange(flag));
                break;

            case FlagView.Range:
                if (parsed.Range is (long first, long last))
                {
                    if (Monitor.IsStored(first) != null)
                    {
                        // Only flags the game stores, from its own tables
                        foreach (var flag in Monitor.StoredFlags(first, last, RowLimit))
                            rows.Add(WithChange(flag));
                    }
                    else
                    {
                        // Not read yet: Smithbox's model (ER), else every ID
                        var model = Project.Handler.ProjectData?.EventFlags;
                        for (var flag = Math.Max(first, 1); flag <= last && rows.Count <= RowLimit; flag++)
                        {
                            if (model != null && !model.IsValid(flag))
                            {
                                flag = flag / 1000 * 1000 + 999;
                                continue;
                            }
                            rows.Add(WithChange(flag));
                        }
                    }
                }
                parsed = new(null, null, null);
                break;
        }

        rows = rows.Where(r => Matches(r, parsed)).ToList();

        RowsTruncated = rows.Count > RowLimit;
        if (RowsTruncated)
            rows.RemoveRange(RowLimit, rows.Count - RowLimit);

        Rows = rows;
    }

    private Row WithChange(long flag)
    {
        var c = Monitor.LastChange(flag);
        return c is EventFlagMonitor.FlagChange change
            ? new Row(EventFlagMonitor.ChangeKind.Flag, flag, change.On, change.At, change.Count, change.Written)
            : new Row(EventFlagMonitor.ChangeKind.Flag, flag, null, null, 0);
    }

    private bool Matches(Row row, ParsedFilter f)
    {
        if (row.Kind != EventFlagMonitor.ChangeKind.Flag)
            return f.Range == null && f.Digits == null && f.Text == null;

        if (f.Digits != null)
            return row.Flag.ToString(CultureInfo.InvariantCulture).Contains(f.Digits);

        if (f.Range is (long first, long last))
            return row.Flag >= first && row.Flag <= last;

        if (f.Text != null)
        {
            if (Names.TryGetValue(row.Flag, out var name) && name.Contains(f.Text, StringComparison.OrdinalIgnoreCase))
                return true;
            var summary = EventFlagDisplay.Summary(Project, row.Flag);
            return summary != null && summary.Contains(f.Text, StringComparison.OrdinalIgnoreCase);
        }

        return true;
    }

    private void UpdateNames()
    {
        var aliases = Project.Handler.ProjectData?.Aliases;
        if (aliases == null || !aliases.TryGetValue(ProjectAliasType.EventFlags, out var list))
            list = new List<AliasEntry>();

        if (list.Count == BuiltAliasCount)
            return;

        BuiltAliasCount = list.Count;
        var names = new Dictionary<long, string>();
        foreach (var entry in list)
        {
            if (long.TryParse(entry.ID, NumberStyles.Integer, CultureInfo.InvariantCulture, out var id) && !string.IsNullOrEmpty(entry.Name))
                names.TryAdd(id, entry.Name);
        }
        Names = names;
        BuiltVersion = -1;
    }

    #endregion
}
