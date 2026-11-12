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
    /// sound/select/&lt;bank name or path&gt;[/&lt;row index or media ID&gt;], e.g. sound/select/cs_c2010/12
    /// </summary>
    private void ParseCommands(string[] commands)
    {
        var data = Project.Handler.SoundData;
        if (commands == null || commands.Length < 2 || commands[0] != "select" || data == null)
            return;

        var container = data.Containers.FirstOrDefault(c => string.Equals(c.Name, commands[1], StringComparison.OrdinalIgnoreCase)
                                                           || string.Equals(c.Path, commands[1], StringComparison.OrdinalIgnoreCase));
        if (container == null)
            return;

        View.SelectContainer(container);
        View.ContainerList.ScrollToSelection = true;

        if (commands.Length > 2 && uint.TryParse(commands[2], out var n))
            _pendingEntry = (container, n);
    }

    // a row to select once its bank has loaded (sound/select/<bank>/<n>)
    private (SoundContainer Container, uint Key)? _pendingEntry;

    private void ApplyPendingEntry()
    {
        if (_pendingEntry is not { } p || p.Container.State != SoundLoadState.Loaded)
            return;

        _pendingEntry = null;
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
