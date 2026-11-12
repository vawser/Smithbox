using Hexa.NET.ImGui;
using StudioCore.Editors.Common;

namespace StudioCore.Editors.SoundEditor;

/// <summary>
/// The editor's windows: Banks, Sounds, Properties and Player. Read-only in S2: list and play.
/// </summary>
public class SoundEditorView : IDisposable
{
    public SoundEditorScreen Editor;
    public ProjectEntry Project;

    public SoundContainerList ContainerList;
    public SoundEntryList EntryList;
    public SoundProperties Properties;
    public SoundPlayerBar PlayerBar;

    public SoundPlayer Player = new();

    public SoundContainer SelectedContainer { get; private set; }
    public SoundEntry SelectedEntry { get; private set; }

    public SoundEditorView(SoundEditorScreen editor, ProjectEntry project)
    {
        Editor = editor;
        Project = project;

        ContainerList = new(this);
        EntryList = new(this);
        Properties = new(this);
        PlayerBar = new(this);

        Player.Volume = CFG.Current.SoundEditor_Volume;
        Player.Loop = CFG.Current.SoundEditor_Loop;
    }

    public SoundData Data => Project.Handler.SoundData;

    public void SelectContainer(SoundContainer container)
    {
        if (SelectedContainer == container)
            return;

        SelectedContainer = container;
        SelectedEntry = null;
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
