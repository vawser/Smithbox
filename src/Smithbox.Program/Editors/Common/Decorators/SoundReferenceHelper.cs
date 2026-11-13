using Hexa.NET.ImGui;
using StudioCore.Application;
using StudioCore.Editors.SoundEditor;
using StudioCore.Keybinds;
using System.Globalization;

namespace StudioCore.Editors.Common;

/// <summary>
/// Quick links from a field holding a sound ID (the fields the meta tags with the Sounds alias) to that sound in the
/// Sound Editor, as <see cref="FxReferenceHelper"/> does for effects: Play / Open entries in the field's context menu,
/// and Ctrl+click to open. The field gives only the number, so every type letter with an event is offered
/// (usually one). Uses the Sound Editor's index (soundbanksinfo.mobnkinfo / .fev names), built on first use.
/// Shared by the Param, Map and Time Act editors.
/// </summary>
public static class SoundReferenceHelper
{
    /// <summary>Whether the project has a Sound Editor to jump to.</summary>
    public static bool IsAvailable(ProjectEntry project) =>
        project?.Descriptor != null
        && project.Descriptor.EnableSoundEditor
        && EditorConditions.SupportsSoundEditor(project.Descriptor.ProjectType)
        && project.Handler?.SoundEditor != null
        && project.Handler.SoundData?.Index != null;

    /// <summary>The sound ID a field value holds; 0 and below (-1 is "none") are no sound.</summary>
    public static bool TryGetId(object value, out string id)
    {
        id = null;
        if (value == null)
            return false;

        try
        {
            var n = Convert.ToInt64(value, CultureInfo.InvariantCulture);
            if (n <= 0)
                return false;
            id = n.ToString(CultureInfo.InvariantCulture);
            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>The context menu entries: play or open each event of the sound ID. Call inside an open popup.</summary>
    public static void ContextMenu(ProjectEntry project, object value)
    {
        if (!IsAvailable(project) || !TryGetId(value, out var id))
            return;

        var index = project.Handler.SoundData.Index;
        index.EnsureBuilt();
        if (!index.IsBuilt)
        {
            ImGui.TextDisabled(LOC.Get(index.Error != null ? "SOUND_Reference_Index_Failed" : "SOUND_Reference_Indexing"));
            return;
        }

        var matches = index.Find(id);
        if (matches.Count == 0)
        {
            ImGui.TextDisabled(LOC.Get("SOUND_Reference_Missing", id));
            return;
        }

        ImGui.PushStyleColor(ImGuiCol.Text, UI.Current.ImGui_AliasName_Text);
        int i = 0;
        foreach (var m in matches.Take(8))
        {
            if (ImGui.Selectable($"{LOC.Get("SOUND_Reference_Play", m.Label)}##soundRefPlay{i}"))
                EditorCommandQueue.AddCommand(SoundIndex.Command(m, true));
            GUI.Tooltip(LOC.Get("SOUND_Reference_Play_TT"));

            if (ImGui.Selectable($"{LOC.Get("SOUND_Reference_Go_To", m.Label)}##soundRefGoTo{i}"))
                EditorCommandQueue.AddCommand(SoundIndex.Command(m, false));
            GUI.Tooltip(LOC.Get("SOUND_Reference_Go_To_TT"));
            i++;
        }
        ImGui.PopStyleColor();

        if (matches.Count > 8)
        {
            if (ImGui.Selectable($"{LOC.Get("SOUND_Reference_More", matches.Count - 8)}##soundRefMore"))
                EditorCommandQueue.AddCommand($"sound/find/{id}");
        }
    }

    // Inline play: a request waits here while the index builds and the bank loads, then plays on the Sound Editor's
    // player. Ticked by the buttons themselves, so it only advances while one is drawn.
    private static ProjectEntry _pendingProject;
    private static string _pendingId;
    private static SoundContainer _pendingContainer;
    private static string _pendingEvent;

    // the sound ID the inline button last started, so its buttons show Stop while it plays
    private static string _playingId;

    private static SoundPlayer Player(ProjectEntry project) => project?.Handler?.SoundEditor?.View?.Player;

    /// <summary>
    /// A small Play / Stop button for the sound ID in <paramref name="value"/>, followed by SameLine so the alias text
    /// sits after it. Plays on the Sound Editor's player without switching to it. Draws nothing (and returns false)
    /// when the project has no Sound Editor or the value is no sound.
    /// </summary>
    public static bool PlayButton(ProjectEntry project, object value, string imguiId)
    {
        if (!IsAvailable(project) || !TryGetId(value, out var id))
            return false;

        Tick(project);

        var player = Player(project);
        bool pending = _pendingId == id && _pendingProject == project;
        bool playing = !pending && _playingId == id && player?.IsActive == true;

        if (ImGui.SmallButton($"{(pending || playing ? Icons.Stop : Icons.Play)}##soundRefInline_{imguiId}"))
        {
            if (pending || playing)
            {
                ClearPending();
                _playingId = null;
                player?.Stop();
            }
            else
            {
                Request(project, id);
            }
        }
        GUI.Tooltip(LOC.Get(pending ? "SOUND_Reference_Inline_Loading_TT"
            : playing ? "SOUND_Reference_Inline_Stop_TT" : "SOUND_Reference_Inline_Play_TT"));

        ImGui.SameLine();
        return true;
    }

    private static void Request(ProjectEntry project, string id)
    {
        Player(project)?.Stop();
        _playingId = null;
        _pendingProject = project;
        _pendingId = id;
        _pendingContainer = null;
        _pendingEvent = null;
        project.Handler.SoundData.Index.EnsureBuilt();
        Tick(project);
    }

    private static void ClearPending()
    {
        _pendingProject = null;
        _pendingId = null;
        _pendingContainer = null;
        _pendingEvent = null;
    }

    /// <summary>Moves a waiting request on: index built → bank loading → bank loaded → play.</summary>
    private static void Tick(ProjectEntry project)
    {
        if (_pendingId == null || _pendingProject != project)
            return;

        var data = project.Handler.SoundData;
        var index = data.Index;

        if (_pendingContainer == null)
        {
            if (!index.IsBuilt)
            {
                if (index.Error != null)
                    ClearPending(); // the index logged why
                else
                    index.EnsureBuilt();
                return;
            }

            var matches = index.Find(_pendingId);
            if (matches.Count == 0)
            {
                Smithbox.Log(typeof(SoundReferenceHelper), LOC.Get("SOUND_Reference_Missing", _pendingId));
                ClearPending();
                return;
            }

            _pendingContainer = matches[0].Container;
            _pendingEvent = matches[0].EventName;
            _pendingContainer.BeginLoad();
        }

        var c = _pendingContainer;
        if (c.State is SoundLoadState.NotLoaded or SoundLoadState.Loading)
            return;

        var id = _pendingId;
        var name = _pendingEvent;
        ClearPending();

        if (c.State != SoundLoadState.Loaded)
            return; // the bank logged why

        // the same lookup as the Sound Editor's sound/event command
        var hash = WwiseBank.Hash(name);
        var ev = c.Events.FirstOrDefault(e => string.Equals(e.Name, name, StringComparison.OrdinalIgnoreCase))
                 ?? c.Events.FirstOrDefault(e => !e.FromFev && e.Id == hash);
        if (ev == null)
        {
            Smithbox.Log(typeof(SoundReferenceHelper), LOC.Get("SOUND_Log_Event_Not_Found", name, c.Name));
            return;
        }

        var entry = ev.Entries.FirstOrDefault(e => e.CanPlay);
        var view = project.Handler.SoundEditor?.View;
        if (entry == null || view == null)
        {
            Smithbox.Log(typeof(SoundReferenceHelper), LOC.Get("SOUND_Reference_Inline_No_Audio", name));
            return;
        }

        view.Play(entry);
        _playingId = id;
    }

    /// <summary>Ctrl+click on the last item opens the sound (its first event). Call right after the item it applies to.</summary>
    public static void Click(ProjectEntry project, object value)
    {
        if (!ImGui.IsItemClicked(ImGuiMouseButton.Left) || !InputManager.HasCtrlDown() || !IsAvailable(project) || !TryGetId(value, out var id))
            return;

        var index = project.Handler.SoundData.Index;
        index.EnsureBuilt();
        var matches = index.Find(id);
        if (matches.Count == 1)
            EditorCommandQueue.AddCommand(SoundIndex.Command(matches[0], false));
        else
            EditorCommandQueue.AddCommand($"sound/find/{id}");
    }
}
