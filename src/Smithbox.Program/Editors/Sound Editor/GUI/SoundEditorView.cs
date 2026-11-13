using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using StudioCore.Utilities;

namespace StudioCore.Editors.SoundEditor;

public enum SoundListTab
{
    Sounds,
    Events,
}

/// <summary>
/// The editor's windows: Banks, Sounds, Properties and Player. Read-only in S2: list and play.
/// </summary>
public class SoundEditorView : IDisposable
{
    public SoundEditorScreen Editor;
    public ProjectEntry Project;

    public SoundContainerList ContainerList;
    public SoundEntryList EntryList;
    public SoundEventList EventList;
    public SoundProperties Properties;
    public SoundPlayerBar PlayerBar;
    public SoundToolWindow ToolWindow;

    // Tools
    public SoundSearchTool SearchTool;

    public SoundPlayer Player = new();

    public SoundContainer SelectedContainer { get; private set; }
    public SoundEntry SelectedEntry { get; private set; }
    public SoundEvent SelectedEvent { get; private set; }

    /// <summary>The tab the Sounds window shows (Properties follows it).</summary>
    public SoundListTab ActiveTab = SoundListTab.Sounds;

    /// <summary>Set to switch tabs on the next frame (e.g. going from an event to one of its sounds).</summary>
    public SoundListTab? RequestedTab;

    public SoundEditorView(SoundEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        ContainerList = new(this);
        EntryList = new(this);
        EventList = new(this);
        Properties = new(this);
        PlayerBar = new(this);
        SearchTool = new(this);
        ToolWindow = new(this);

        Player.Volume = CFG.Current.SoundEditor_Volume;
        Player.Loop = CFG.Current.SoundEditor_Loop;
    }

    public SoundData Data => Project.Handler.SoundData;

    /// <summary>Undo/redo of replacements (write plan W2).</summary>
    public ActionManager ActionManager = new();

    /// <summary>
    /// Sounds can be replaced: Wwise on PC (ER, NR, AC6). FMOD games stay read-only (write plan D3); DB_NT is a
    /// console build (Opus), read-only until a write path for it is proven.
    /// </summary>
    public bool CanEdit => Data?.Middleware == SoundMiddleware.Wwise &&
                           Project.Descriptor.ProjectType is ProjectType.ER or ProjectType.NR or ProjectType.AC6;

    // after a save reloads the selected bank: the sound to select again once it has loaded
    private uint? _reselectMedia;

    /// <summary>Asks for a file and replaces the sound with it (unsaved until Save).</summary>
    public void ReplaceEntry(SoundEntry e)
    {
        if (!CanEdit || e == null || e.IsNotAudio)
            return;

        if (!PlatformUtils.Instance.OpenFileDialog(LOC.Get("SOUND_Replace_Dialog"), SoundImport.Filters,
                CFG.Current.SoundEditor_Import_Folder, out var file) || string.IsNullOrEmpty(file))
            return;
        CFG.Current.SoundEditor_Import_Folder = Path.GetDirectoryName(file);

        var edit = SoundImport.Load(file, e.StreamPath, out var error);
        if (edit == null)
        {
            Smithbox.LogError(this, error);
            return;
        }

        StopIfCurrent(e);
        ActionManager.ExecuteAction(new ReplaceSoundAction(Data.Edits, SoundEdits.KeyOf(e), edit));
        Smithbox.Log(this, LOC.Get("SOUND_Replace_Log", e.DisplayName, edit.Label));
    }

    /// <summary>Drops an unsaved replacement.</summary>
    public void DiscardReplacement(SoundEntry e)
    {
        if (Data.Edits.Get(e) == null) return;
        StopIfCurrent(e);
        ActionManager.ExecuteAction(new ReplaceSoundAction(Data.Edits, SoundEdits.KeyOf(e), null));
    }

    /// <summary>Replaces the sound with the game's own audio (undoes a saved replacement once saved).</summary>
    public void RevertToVanilla(SoundEntry e)
    {
        if (!CanEdit || e == null) return;
        var edit = SoundImport.Vanilla(Data, e, out var error);
        if (edit == null)
        {
            Smithbox.LogError(this, error);
            return;
        }
        StopIfCurrent(e);
        ActionManager.ExecuteAction(new ReplaceSoundAction(Data.Edits, SoundEdits.KeyOf(e), edit));
    }

    /// <summary>Is a saved (project) version of this sound's files there, so Revert to Vanilla means something?</summary>
    public bool HasProjectVersion(SoundEntry e)
        => e != null && (e.Container.InProject || (e.StreamPath != null && Data.IsInProject(e.StreamPath)));

    private void StopIfCurrent(SoundEntry e)
    {
        if (IsCurrent(e))
            Player.Stop();
    }

    /// <summary>Saves every replacement (on a worker; <see cref="Update"/> picks up the result).</summary>
    public void SaveEdits()
    {
        if (Data.Edits.Count == 0 || Data.Edits.IsSaving)
            return;
        Player.Stop();
        Data.Edits.SaveAll();
    }

    /// <summary>Each frame: apply a finished save (reload rewritten banks, clear undo, reselect).</summary>
    public void Update()
    {
        var reloaded = Data?.Edits.ApplySaved();
        if (reloaded != null)
        {
            // the saved state is on disk; undo would no longer match it
            ActionManager.Clear();

            var c = SelectedContainer;
            if (c != null && reloaded.Contains(c))
            {
                _reselectMedia = SelectedEntry?.MediaId;
                SelectedContainer = null;
                SelectContainer(c);
            }
        }

        if (_reselectMedia is { } id && SelectedContainer?.State == SoundLoadState.Loaded)
        {
            _reselectMedia = null;
            var e = SelectedContainer.Entries.FirstOrDefault(x => x.MediaId == id);
            if (e != null)
            {
                SelectEntry(e);
                EntryList.ScrollToSelection = true;
            }
        }
    }

    public void SelectContainer(SoundContainer container)
    {
        if (SelectedContainer == container)
            return;

        SelectedContainer = container;
        SelectedEntry = null;
        SelectedEvent = null;
        EntryList.ResetScroll();
        container?.BeginLoad();
    }

    public void SelectEntry(SoundEntry entry, bool play = false)
    {
        SelectedEntry = entry;
        if (entry == null)
            return;

        RequestInfo(entry);

        if (play || CFG.Current.SoundEditor_Play_On_Select)
            Play(entry);
    }

    /// <summary>Selects an event; playing it plays the first sound it can reach.</summary>
    public void SelectEvent(SoundEvent ev, bool play = false)
    {
        SelectedEvent = ev;
        if (ev == null)
            return;

        var first = ev.Entries.FirstOrDefault(e => e.CanPlay);
        if (first != null)
            RequestInfo(first);

        if (first != null && (play || CFG.Current.SoundEditor_Play_On_Select))
            Play(first);
    }

    /// <summary>Shows one of an event's sounds in the Sounds tab (from Properties).</summary>
    public void GoToEntry(SoundEntry entry, bool play)
    {
        RequestedTab = SoundListTab.Sounds;
        SelectEntry(entry, play);
        EntryList.ScrollToSelection = true;
    }

    /// <summary>Shows an event in the Events tab (from a sound's "Played by").</summary>
    public void GoToEvent(SoundEvent ev)
    {
        RequestedTab = SoundListTab.Events;
        SelectEvent(ev);
        EventList.ScrollToSelection = true;
    }

    public void Play(SoundEntry entry)
    {
        if (entry == null || !entry.CanPlay)
            return;

        if (!VgmstreamCli.IsAvailable)
        {
            Smithbox.LogError(this, LOC.Get("SOUND_Log_No_Vgmstream", VgmstreamCli.ExePath));
            return;
        }

        Player.Play(entry.Source);
    }

    /// <summary>Asks vgmstream for the exact format and length of an entry, once, on a worker.</summary>
    public void RequestInfo(SoundEntry entry)
    {
        if (entry.InfoRequested || !entry.CanPlay || !VgmstreamCli.IsAvailable)
            return;

        entry.InfoRequested = true;
        Task.Run(() =>
        {
            try { entry.Info = VgmstreamCli.GetInfo(entry.Source); }
            catch (Exception e) { entry.InfoError = e.Message; }
        });
    }

    /// <summary>The running or last export (its progress shows in the Player window).</summary>
    public SoundExportJob ExportJob;

    public bool CanStartExport => ExportJob is not { Running: true };

    /// <summary>
    /// Asks for a folder and exports <paramref name="entries"/> into a sub-folder named <paramref name="label"/>
    /// (a single sound goes straight into the chosen folder).
    /// </summary>
    public void Export(string label, SoundExportFormat format, Func<IReadOnlyList<SoundEntry>> entries,
        SoundContainer waitFor = null, bool subFolder = true)
    {
        if (!CanStartExport)
            return;

        if (format == SoundExportFormat.Wav && !VgmstreamCli.IsAvailable)
        {
            Smithbox.LogError(this, LOC.Get("SOUND_Log_No_Vgmstream", VgmstreamCli.ExePath));
            return;
        }

        var last = CFG.Current.SoundEditor_Export_Folder;
        if (!PlatformUtils.Instance.OpenFolderDialog(LOC.Get("SOUND_Export_Dialog_Folder"), out var folder,
                Directory.Exists(last) ? last : null) || string.IsNullOrEmpty(folder))
            return;

        CFG.Current.SoundEditor_Export_Folder = folder;
        var target = subFolder ? Path.Combine(folder, SoundExportJob.Sanitize(label)) : folder;
        ExportJob = SoundExportJob.Start(target, label, format, entries, waitFor);
    }

    public void ExportEntry(SoundEntry e, SoundExportFormat format)
        => Export(e.DisplayName, format, () => new[] { e }, subFolder: false);

    public void ExportEvent(SoundEvent ev)
        => Export(ev.DisplayName, SoundExportFormat.Wav, () => ev.Entries);

    /// <summary>Exports a whole bank, loading it first if it wasn't opened.</summary>
    public void ExportContainer(SoundContainer c, SoundExportFormat format)
    {
        var label = string.IsNullOrEmpty(c.Language) ? c.Name : $"{c.Name}_{c.Language}";
        Export(label, format, () => c.Entries, c.State == SoundLoadState.Loaded ? null : c);
    }

    /// <summary>Is this entry the one the player has (playing, paused or loading)?</summary>
    public bool IsCurrent(SoundEntry entry)
    {
        var cur = Player.Current;
        return entry?.Source != null && cur != null && Player.IsActive &&
               cur.CacheKey == entry.Source.CacheKey && cur.Subsong == entry.Source.Subsong;
    }

    public void Display(uint dockspaceId)
    {
        if (Data == null)
            return;

        Window("SOUND_Window_Banks", "soundEditor_Banks", EditorFocusContext.SoundEditor_Banks, dockspaceId, ContainerList.Draw);
        Window("SOUND_Window_Sounds", "soundEditor_Sounds", EditorFocusContext.SoundEditor_Sounds, dockspaceId, EntryList.Draw);
        Window("SOUND_Window_Properties", "soundEditor_Properties", EditorFocusContext.SoundEditor_Properties, dockspaceId, Properties.Draw);
        Window("SOUND_Window_Player", "soundEditor_Player", EditorFocusContext.SoundEditor_Sounds, dockspaceId, PlayerBar.Draw);
        if (CFG.Current.Interface_SoundEditor_ToolWindow)
            DisplayToolWindow(dockspaceId);

        // keep settings in step with the player bar's controls
        CFG.Current.SoundEditor_Volume = Player.Volume;
        CFG.Current.SoundEditor_Loop = Player.Loop;
    }

    private void Window(string titleKey, string id, EditorFocusContext focus, uint dockspaceId, Action draw)
    {
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_SoundEditor);

        if (ImGui.Begin($"{LOC.Get(titleKey)}###{id}", GUI.GetInnerWindowFlags()))
        {
            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                FocusManager.SetFocus(focus);

            draw();
        }

        ImGui.End();
    }

    /// <summary>The Tools window has its own menu bar (View + tool navigation options), so main-window flags.</summary>
    private void DisplayToolWindow(uint dockspaceId)
    {
        ImGui.SetNextWindowDockID(dockspaceId, ImGuiCond.FirstUseEver);
        ImGui.SetNextWindowClass(ref GUI.DockGroup_SoundEditor);

        if (ImGui.Begin($"{LOC.Get("SOUND_Window_Tools")}{SoundToolWindow.WindowId}", GUI.GetMainWindowFlags()))
        {
            if (ImGui.IsWindowHovered(ImGuiHoveredFlags.ChildWindows))
                FocusManager.SetFocus(EditorFocusContext.SoundEditor_ToolWindow);

            ToolWindow.Display();
        }

        ImGui.End();
    }

    public static string FormatTime(double seconds)
    {
        if (seconds <= 0) return "-";
        var t = TimeSpan.FromSeconds(seconds);
        return t.TotalHours >= 1 ? t.ToString(@"h\:mm\:ss") : t.ToString(@"m\:ss\.f");
    }

    public void Dispose()
    {
        Player.Dispose();
    }
}
