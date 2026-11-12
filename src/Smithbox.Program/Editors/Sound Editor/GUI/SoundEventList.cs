using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.SoundEditor;

/// <summary>
/// The Wwise events of the selected bank: name (table / rule / guessed), alias, actions and how many of the bank's
/// sounds each can play. Double-click or Enter plays the event's first playable sound.
/// </summary>
public class SoundEventList
{
    private readonly SoundEditorView View;

    private string Filter = "";
    private bool ExactFilter = false;
    private bool PlayOnly = true;

    private List<SoundEvent> Visible = new();
    private List<SoundEvent> VisibleSource;
    private string VisibleFilter;
    private bool VisibleExact, VisiblePlayOnly;
    private int VisibleNamesVersion = -1;

    public bool ScrollToSelection;

    public SoundEventList(SoundEditorView view)
    {
        View = view;
    }

    public void Draw(SoundContainer c)
    {
        ImGui.BeginChild("soundEventListHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);
        EditorFilters.DisplaySearchbar("soundEditor_EventList", ref Filter, ref ExactFilter);
        ImGui.EndChild();

        ImGui.Checkbox($"{LOC.Get("SOUND_Events_Play_Only")}##soundEventsPlayOnly", ref PlayOnly);
        GUI.Tooltip(LOC.Get("SOUND_Events_Play_Only_TT"));

        ImGui.BeginChild("soundEventList", Vector2.Zero, ImGuiChildFlags.Borders);

        if (c.Events.Count == 0)
        {
            ImGui.TextDisabled(LOC.Get("SOUND_Events_None"));
            ImGui.EndChild();
            return;
        }

        BuildVisible(c.Events);
        DrawTable();
        HandleKeys();

        ImGui.EndChild();
    }

    private void DrawTable()
    {
        var flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Resizable;
        if (!ImGui.BeginTable("soundEventTable", 5, flags, Vector2.Zero))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn(LOC.Get("SOUND_Column_Index"));
        ImGui.TableSetupColumn(LOC.Get("SOUND_Column_Event"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn(LOC.Get("SOUND_Column_Alias"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn(LOC.Get("SOUND_Column_Actions"));
        ImGui.TableSetupColumn(LOC.Get("SOUND_Column_Sounds"));
        ImGui.TableHeadersRow();

        if (ScrollToSelection && View.SelectedEvent != null)
        {
            int row = Visible.IndexOf(View.SelectedEvent);
            if (row >= 0)
                ImGui.SetScrollY(row * ImGui.GetTextLineHeightWithSpacing());
            ScrollToSelection = false;
        }

        var clipper = new ImGuiListClipper();
        clipper.Begin(Visible.Count);
        while (clipper.Step())
        {
            for (int i = clipper.DisplayStart; i < clipper.DisplayEnd; i++)
                DrawRow(Visible[i]);
        }
        clipper.End();

        ImGui.EndTable();
    }

    private void DrawRow(SoundEvent ev)
    {
        ImGui.TableNextRow();

        // Index (+ playing marker)
        ImGui.TableNextColumn();
        bool current = ev.Entries.Any(View.IsCurrent);
        var label = current ? $"> {ev.Index}" : ev.Index.ToString();
        if (ImGui.Selectable($"{label}##soundEvent_{ev.Id}", View.SelectedEvent == ev, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowDoubleClick))
            View.SelectEvent(ev, ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left));

        if (ImGui.BeginPopupContextItem($"soundEventContext_{ev.Id}"))
        {
            ContextMenu(ev);
            ImGui.EndPopup();
        }

        // Event name: guessed names muted, unnamed events by ID
        ImGui.TableNextColumn();
        switch (ev.NameSource)
        {
            case SoundEventNameSource.Guess:
                ImGui.TextDisabled($"{ev.DisplayName} ?");
                GUI.Tooltip(LOC.Get("SOUND_Event_Name_Guess_TT"));
                break;
            case SoundEventNameSource.None:
                ImGui.TextDisabled(ev.DisplayName);
                break;
            default:
                ImGui.Text(ev.DisplayName);
                break;
        }

        // Alias
        ImGui.TableNextColumn();
        var alias = ev.Alias;
        if (!string.IsNullOrEmpty(alias))
            ImGui.TextColored(UI.Current.ImGui_AliasName_Text, alias);

        // Actions
        ImGui.TableNextColumn();
        ImGui.TextDisabled(ev.ActionSummary);

        // Sounds it can play
        ImGui.TableNextColumn();
        if (ev.Entries.Count > 0)
            ImGui.Text(ev.Entries.Count.ToString());
        else if (ev.LayersUnread)
        {
            ImGui.TextDisabled("?");
            GUI.Tooltip(LOC.Get("SOUND_Events_Layers_Unread_TT"));
        }
        else if (ev.ExternalTargets > 0)
        {
            ImGui.TextDisabled(LOC.Get("SOUND_Events_External"));
            GUI.Tooltip(LOC.Get(ev.FromFev ? "SOUND_Events_External_Fev_TT" : "SOUND_Events_External_TT", ev.ExternalTargets));
        }
    }

    private void ContextMenu(SoundEvent ev)
    {
        var first = ev.Entries.FirstOrDefault(e => e.CanPlay);
        if (ImGui.MenuItem($"{LOC.Get("SOUND_Action_Play")}##play", "", false, first != null))
            View.SelectEvent(ev, true);

        if (ImGui.MenuItem($"{LOC.Get("SOUND_Action_Copy_Event_Name")}##copyName"))
            ImGui.SetClipboardText(ev.DisplayName);

        if (!ev.FromFev && ImGui.MenuItem($"{LOC.Get("SOUND_Action_Copy_Event_Id")}##copyId"))
            ImGui.SetClipboardText(ev.Id.ToString());

        if (ev.SoundId != null && ImGui.MenuItem($"{LOC.Get("SOUND_Action_Copy_Sound_Id")}##copySoundId"))
            ImGui.SetClipboardText(ev.SoundId);
    }

    private void HandleKeys()
    {
        if (!ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows) || ImGui.IsAnyItemActive())
            return;

        var sel = View.SelectedEvent;

        if (sel != null && (ImGui.IsKeyPressed(ImGuiKey.Enter) || ImGui.IsKeyPressed(ImGuiKey.KeypadEnter)))
            View.SelectEvent(sel, true);

        if (ImGui.IsKeyPressed(ImGuiKey.Space))
            View.Player.TogglePause();

        int dir = ImGui.IsKeyPressed(ImGuiKey.DownArrow) ? 1 : ImGui.IsKeyPressed(ImGuiKey.UpArrow) ? -1 : 0;
        if (dir != 0 && Visible.Count > 0)
        {
            int i = sel == null ? -1 : Visible.IndexOf(sel);
            i = Math.Clamp(i + dir, 0, Visible.Count - 1);
            View.SelectEvent(Visible[i]);
            ScrollToSelection = true;
        }
    }

    private void BuildVisible(List<SoundEvent> events)
    {
        var namesVersion = View.Data?.Names?.Version ?? 0;
        if (VisibleSource == events && VisibleFilter == Filter && VisibleExact == ExactFilter && VisiblePlayOnly == PlayOnly && VisibleNamesVersion == namesVersion)
            return;

        VisibleSource = events;
        VisibleFilter = Filter;
        VisibleExact = ExactFilter;
        VisiblePlayOnly = PlayOnly;
        VisibleNamesVersion = namesVersion;

        IEnumerable<SoundEvent> q = events;
        if (PlayOnly)
            q = q.Where(e => e.PlayActions > 0);
        if (!string.IsNullOrWhiteSpace(Filter))
            q = q.Where(e => EditorFilters.IsMatch(Filter, e.DisplayName, ExactFilter, e.Alias ?? "") ||
                             (e.SoundId != null && EditorFilters.IsMatch(Filter, e.SoundId, ExactFilter)) ||
                             EditorFilters.IsMatch(Filter, e.Id.ToString(), ExactFilter));
        Visible = q.ToList();
    }
}
