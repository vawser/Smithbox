using Hexa.NET.ImGui;
using StudioCore.Editors.Common;
using System.Numerics;

namespace StudioCore.Editors.SoundEditor;

/// <summary>
/// The Sound Editor: lists the sound banks of FMOD games (DS1–SDT .fsb) and Wwise games (ER, NR, AC6, DB_NT .bnk/.wem)
/// and plays their sounds through vgmstream. S2 is read-only; see Claude/Documentation/Sound Editor Plan.md.
/// </summary>
public class SoundEditorScreen : EditorScreen, IDisposable
{
    private ProjectEntry Project;

    public SoundEditorView View;

    public SoundEditorScreen(ProjectEntry project)
    {
        Project = project;
        View = new SoundEditorView(this, project);
    }

    public string EditorName => "Sound Editor##SoundEditor";
    public string CommandEndpoint => "sound";
    public string SaveType => "Sound";
    public string WindowName => "";
    public bool HasDocked { get; set; }

    public void OnGUI(string[] commands)
    {
        ParseCommands(commands);
        ApplyPendingEntry();
        ApplyPendingEvent();

        if (ImGui.BeginMenuBar())
        {
            ViewMenu();
            ImGui.EndMenuBar();
        }

        var dsid = ImGui.GetID("DockSpace_SoundEditor");
        ImGui.DockSpace(dsid, new Vector2(0, 0), ImGuiDockNodeFlags.None, ref GUI.DockGroup_SoundEditor);

        View.Display(dsid);
    }

    /// <summary>
    /// sound/select/&lt;bank&gt;[/&lt;event ID, media ID or row&gt;] · sound/event/&lt;bank&gt;/&lt;event name&gt;[/play] ·
    /// sound/find/&lt;text&gt;. A bank is its name or its path with '|' for '/' (commands split on '/').
    /// </summary>
    private void ParseCommands(string[] commands)
    {
        var data = Project.Handler.SoundData;
        if (commands == null || commands.Length < 2 || data == null)
            return;

        if (commands[0] == "find")
        {
            View.SearchTool.SetQuery(string.Join("/", commands[1..]));
            return;
        }

        if (commands[0] is not ("select" or "event"))
            return;

        var key = commands[1].Replace('|', '/');
        var container = data.Containers.FirstOrDefault(c => string.Equals(c.Path, key, StringComparison.OrdinalIgnoreCase))
                        ?? data.Containers.FirstOrDefault(c => string.Equals(c.Name, key, StringComparison.OrdinalIgnoreCase));
        if (container == null)
            return;

        View.SelectContainer(container);
        View.ContainerList.ScrollToSelection = true;

        if (commands[0] == "select" && commands.Length > 2 && uint.TryParse(commands[2], out var n))
            _pendingEntry = (container, n);

        if (commands[0] == "event" && commands.Length > 2)
            _pendingEvent = (container, commands[2], commands.Length > 3 && commands[3] == "play");
    }

    // an event to select (and maybe play) once its bank has loaded (sound/event/...)
    private (SoundContainer Container, string Name, bool Play)? _pendingEvent;

    private void ApplyPendingEvent()
    {
        if (_pendingEvent is not { } p || p.Container.State is SoundLoadState.NotLoaded or SoundLoadState.Loading)
            return;

        _pendingEvent = null;
        if (p.Container.State != SoundLoadState.Loaded)
            return;

        var hash = WwiseBank.Hash(p.Name);
        var ev = p.Container.Events.FirstOrDefault(e => string.Equals(e.Name, p.Name, StringComparison.OrdinalIgnoreCase))
                 ?? p.Container.Events.FirstOrDefault(e => !e.FromFev && e.Id == hash);
        if (ev == null)
        {
            Smithbox.Log(this, LOC.Get("SOUND_Log_Event_Not_Found", p.Name, p.Container.Name));
            return;
        }

        View.GoToEvent(ev);
        if (p.Play)
            View.SelectEvent(ev, true);
    }

    // a row to select once its bank has loaded (sound/select/<bank>/<n>)
    private (SoundContainer Container, uint Key)? _pendingEntry;

    private void ApplyPendingEntry()
    {
        if (_pendingEntry is not { } p || p.Container.State != SoundLoadState.Loaded)
            return;

        _pendingEntry = null;

        // an event ID selects the event in the Events tab
        var ev = p.Container.Events.FirstOrDefault(x => x.Id == p.Key);
        if (ev != null)
        {
            View.GoToEvent(ev);
            return;
        }

        var e = p.Container.Entries.FirstOrDefault(x => x.MediaId == p.Key)
                ?? (p.Key < p.Container.Entries.Count ? p.Container.Entries[(int)p.Key] : null);
        if (e != null)
        {
            View.SelectEntry(e);
            View.EntryList.ScrollToSelection = true;
        }
    }

    public void ViewMenu()
    {
        if (ImGui.BeginMenu($"{LOC.Get("EDITOR_Menubar_Header_View")}##viewMenuHeader"))
        {
            if (ImGui.MenuItem($"{LOC.Get("SOUND_View_Toggle_Play_On_Select")}##playOnSelect"))
            {
                CFG.Current.SoundEditor_Play_On_Select = !CFG.Current.SoundEditor_Play_On_Select;
            }
            GUI.Tooltip(LOC.Get("SOUND_View_Toggle_Play_On_Select_TT"));
            GUI.ShowActiveStatus(CFG.Current.SoundEditor_Play_On_Select);

            if (ImGui.MenuItem($"{LOC.Get("SOUND_View_Toggle_Tools")}##toolsToggle"))
            {
                CFG.Current.Interface_SoundEditor_ToolWindow = !CFG.Current.Interface_SoundEditor_ToolWindow;
            }
            GUI.Tooltip(LOC.Get("SOUND_View_Toggle_Tools_TT"));
            GUI.ShowActiveStatus(CFG.Current.Interface_SoundEditor_ToolWindow);

            ImGui.EndMenu();
        }
    }

    public void Save()
    {
        // read-only in S2; keep the volume/loop settings
        Smithbox.Instance.SaveConfiguration();
    }

    public void OnDefocus()
    {
    }

    public void Dispose()
    {
        View.Dispose();
    }
}
