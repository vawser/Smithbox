using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.SoundEditor;

/// <summary>
/// The sounds of the selected bank: FSB samples, or the wems a Wwise bank holds or streams. Double-click or Enter
/// plays; the row being played is marked.
/// </summary>
public class SoundEntryList
{
    private readonly SoundEditorView View;

    private string Filter = "";
    private bool ExactFilter = false;

    private List<SoundEntry> Visible = new();
    private List<SoundEntry> VisibleSource;
    private string VisibleFilter;
    private bool VisibleExact;
    private int VisibleNamesVersion = -1;

    private bool ResetScrollPending;
    public bool ScrollToSelection;

    public SoundEntryList(SoundEditorView view)
    {
        View = view;
    }

    public void ResetScroll() => ResetScrollPending = true;

    public void Draw()
    {
        var c = View.SelectedContainer;
        GUI.TitleHeader(LOC.Get("SOUND_Sounds_Header"), LOC.Get("SOUND_Sounds_Header_TT"));

        if (c == null || c.State != SoundLoadState.Loaded)
        {
            ImGui.BeginChild("soundEntryList", Vector2.Zero, ImGuiChildFlags.Borders);
            if (c == null)
                ImGui.TextDisabled(LOC.Get("SOUND_Sounds_Select_Bank"));
            else if (c.State == SoundLoadState.Failed)
                ImGui.TextColored(UI.Current.ImGui_Warning_Text_Color, LOC.Get("SOUND_Sounds_Failed", c.Name, c.Error));
            else
                ImGui.TextDisabled(LOC.Get("SOUND_Sounds_Loading", c.Name));
            ImGui.EndChild();
            return;
        }

        // Sounds | Events: Wwise banks, and FMOD banks whose .fev project was read
        if ((c.Kind == SoundContainerKind.WwiseBank || c.Events.Count > 0) && ImGui.BeginTabBar("soundListTabs"))
        {
            if (ImGui.BeginTabItem($"{LOC.Get("SOUND_Tab_Sounds")} ({c.Entries.Count})###soundTabSounds",
                    View.RequestedTab == SoundListTab.Sounds ? ImGuiTabItemFlags.SetSelected : ImGuiTabItemFlags.None))
            {
                View.ActiveTab = SoundListTab.Sounds;
                DrawSounds(c);
                ImGui.EndTabItem();
            }
            if (ImGui.BeginTabItem($"{LOC.Get("SOUND_Tab_Events")} ({c.Events.Count})###soundTabEvents",
                    View.RequestedTab == SoundListTab.Events ? ImGuiTabItemFlags.SetSelected : ImGuiTabItemFlags.None))
            {
                View.ActiveTab = SoundListTab.Events;
                View.EventList.Draw(c);
                ImGui.EndTabItem();
            }
            View.RequestedTab = null;
            ImGui.EndTabBar();
            return;
        }

        View.ActiveTab = SoundListTab.Sounds;
        DrawSounds(c);
    }

    private void DrawSounds(SoundContainer c)
    {
        ImGui.BeginChild("soundEntryListHeader", EditorFilters.GetHeaderSize(), ImGuiChildFlags.Borders);
        EditorFilters.DisplaySearchbar("soundEditor_EntryList", ref Filter, ref ExactFilter);
        ImGui.EndChild();

        ImGui.BeginChild("soundEntryList", Vector2.Zero, ImGuiChildFlags.Borders);

        if (c.Entries.Count == 0)
        {
            ImGui.TextDisabled(LOC.Get("SOUND_Sounds_None"));
            ImGui.EndChild();
            return;
        }

        BuildVisible(c.Entries);
        DrawTable();
        HandleKeys();

        ImGui.EndChild();
    }

    private void DrawTable()
    {
        var flags = ImGuiTableFlags.RowBg | ImGuiTableFlags.BordersInnerV | ImGuiTableFlags.ScrollY | ImGuiTableFlags.SizingFixedFit | ImGuiTableFlags.Resizable;
        if (!ImGui.BeginTable("soundEntryTable", 6, flags, Vector2.Zero))
            return;

        ImGui.TableSetupScrollFreeze(0, 1);
        ImGui.TableSetupColumn(LOC.Get("SOUND_Column_Index"));
        ImGui.TableSetupColumn(LOC.Get("SOUND_Column_Name"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn(LOC.Get("SOUND_Column_Alias"), ImGuiTableColumnFlags.WidthStretch);
        ImGui.TableSetupColumn(LOC.Get("SOUND_Column_Length"));
        ImGui.TableSetupColumn(LOC.Get("SOUND_Column_Format"));
        ImGui.TableSetupColumn(LOC.Get("SOUND_Column_Source"));
        ImGui.TableHeadersRow();

        if (ResetScrollPending)
        {
            ImGui.SetScrollY(0);
            ResetScrollPending = false;
        }

        if (ScrollToSelection && View.SelectedEntry != null)
        {
            int row = Visible.IndexOf(View.SelectedEntry);
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

    private void DrawRow(SoundEntry e)
    {
        ImGui.TableNextRow();

        // Index (+ playing marker)
        ImGui.TableNextColumn();
        var current = View.IsCurrent(e);
        var label = current ? $"> {e.Index}" : e.Index.ToString();
        if (ImGui.Selectable($"{label}##soundEntry_{e.Index}", View.SelectedEntry == e, ImGuiSelectableFlags.SpanAllColumns | ImGuiSelectableFlags.AllowDoubleClick))
        {
            View.SelectEntry(e, ImGui.IsMouseDoubleClicked(ImGuiMouseButton.Left));
        }

        if (ImGui.BeginPopupContextItem($"soundEntryContext_{e.Index}"))
        {
            ContextMenu(e);
            ImGui.EndPopup();
        }

        // Name
        ImGui.TableNextColumn();
        if (e.IsEmpty || e.IsNotAudio)
            ImGui.TextDisabled($"{e.DisplayName} {LOC.Get(e.IsNotAudio ? "SOUND_Label_Not_Audio" : "SOUND_Label_Empty")}");
        else
            ImGui.Text(e.DisplayName);
        if (e.SourceFile != null)
            GUI.Tooltip(e.SourceFile);

        // Alias (the project's Sounds alias for the sound ID in the name)
        ImGui.TableNextColumn();
        var alias = e.Alias;
        if (!string.IsNullOrEmpty(alias))
            ImGui.TextColored(UI.Current.ImGui_AliasName_Text, alias);

        // Length
        ImGui.TableNextColumn();
        var secs = e.BestSeconds;
        ImGui.Text(secs > 0 ? (e.Info == null && e.SecondsApprox ? $"~{SoundEditorView.FormatTime(secs)}" : SoundEditorView.FormatTime(secs)) : "");

        // Format
        ImGui.TableNextColumn();
        ImGui.Text(FormatText(e));

        // Source
        ImGui.TableNextColumn();
        ImGui.TextDisabled(e.Container.Kind == SoundContainerKind.WwiseBank
            ? LOC.Get(e.IsStreamed ? "SOUND_Source_Streamed" : "SOUND_Source_InBank")
            : e.InnerFile ?? "");
    }

    public static string FormatText(SoundEntry e)
    {
        var rate = e.Info?.SampleRate > 0 ? e.Info.SampleRate : e.SampleRate;
        var ch = e.Info?.Channels > 0 ? e.Info.Channels : e.Channels;
        var codec = e.Codec;
        if (rate <= 0) return codec;
        return $"{codec} {rate / 1000.0:0.#} kHz {ch} ch";
    }

    private void ContextMenu(SoundEntry e)
    {
        if (ImGui.MenuItem($"{LOC.Get("SOUND_Action_Play")}##play", "", false, e.CanPlay))
            View.SelectEntry(e, true);

        if (ImGui.MenuItem($"{LOC.Get("SOUND_Action_Copy_Name")}##copyName"))
            ImGui.SetClipboardText(e.DisplayName);

        if (e.SoundId != null && ImGui.MenuItem($"{LOC.Get("SOUND_Action_Copy_Sound_Id")}##copySoundId"))
            ImGui.SetClipboardText(e.SoundId);

        if (e.MediaId != 0 && ImGui.MenuItem($"{LOC.Get("SOUND_Action_Copy_Id")}##copyId"))
            ImGui.SetClipboardText(e.MediaId.ToString());

        if (e.StreamPath != null && ImGui.MenuItem($"{LOC.Get("SOUND_Action_Copy_Path")}##copyPath"))
            ImGui.SetClipboardText(e.StreamPath);

        ImGui.Separator();
        var canStart = View.CanStartExport;

        if (ImGui.MenuItem($"{LOC.Get("SOUND_Export_Wav")}##exportWav", "", false, canStart && SoundExportJob.CanExport(e, SoundExportFormat.Wav)))
            View.ExportEntry(e, SoundExportFormat.Wav);
        GUI.Tooltip(LOC.Get("SOUND_Export_Wav_TT"));

        if (e.Container.Kind == SoundContainerKind.WwiseBank)
        {
            if (ImGui.MenuItem($"{LOC.Get("SOUND_Export_Wem")}##exportWem", "", false, canStart && SoundExportJob.CanExport(e, SoundExportFormat.Wem)))
                View.ExportEntry(e, SoundExportFormat.Wem);
            GUI.Tooltip(LOC.Get("SOUND_Export_Wem_TT"));
        }

        // the list as filtered: a whole bank when there is no filter
        var visible = Visible;
        var filtered = !string.IsNullOrWhiteSpace(Filter);
        var label = filtered ? LOC.Get("SOUND_Export_Visible_Wav", visible.Count) : LOC.Get("SOUND_Export_Bank_Wav", visible.Count);
        if (ImGui.MenuItem($"{label}##exportVisible", "", false, canStart && visible.Count > 0))
        {
            var c = e.Container;
            var name = string.IsNullOrEmpty(c.Language) ? c.Name : $"{c.Name}_{c.Language}";
            var list = visible.ToList();
            View.Export(filtered ? $"{name}_filtered" : name, SoundExportFormat.Wav, () => list);
        }
        GUI.Tooltip(LOC.Get("SOUND_Export_Visible_Wav_TT"));
    }

    private void HandleKeys()
    {
        if (!ImGui.IsWindowFocused(ImGuiFocusedFlags.RootAndChildWindows) || ImGui.IsAnyItemActive())
            return;

        var sel = View.SelectedEntry;

        if (sel != null && (ImGui.IsKeyPressed(ImGuiKey.Enter) || ImGui.IsKeyPressed(ImGuiKey.KeypadEnter)))
            View.SelectEntry(sel, true);

        if (ImGui.IsKeyPressed(ImGuiKey.Space))
            View.Player.TogglePause();

        // Up / Down step through the visible rows
        int dir = ImGui.IsKeyPressed(ImGuiKey.DownArrow) ? 1 : ImGui.IsKeyPressed(ImGuiKey.UpArrow) ? -1 : 0;
        if (dir != 0 && Visible.Count > 0)
        {
            int i = sel == null ? -1 : Visible.IndexOf(sel);
            i = Math.Clamp(i + dir, 0, Visible.Count - 1);
            View.SelectEntry(Visible[i]);
            ScrollToSelection = true;
        }
    }

    private void BuildVisible(List<SoundEntry> entries)
    {
        // alias edits change what a filter matches
        var namesVersion = View.Data?.Names?.Version ?? 0;
        if (VisibleSource == entries && VisibleFilter == Filter && VisibleExact == ExactFilter && VisibleNamesVersion == namesVersion)
            return;

        VisibleSource = entries;
        VisibleFilter = Filter;
        VisibleExact = ExactFilter;
        VisibleNamesVersion = namesVersion;

        // name, alias, sound ID and media ID all match
        Visible = string.IsNullOrWhiteSpace(Filter)
            ? entries
            : entries.Where(e => EditorFilters.IsMatch(Filter, e.DisplayName, ExactFilter, e.Alias ?? "") ||
                                 (e.SoundId != null && EditorFilters.IsMatch(Filter, e.SoundId, ExactFilter)) ||
                                 (e.MediaId != 0 && EditorFilters.IsMatch(Filter, e.MediaId.ToString(), ExactFilter))).ToList();
    }
}
