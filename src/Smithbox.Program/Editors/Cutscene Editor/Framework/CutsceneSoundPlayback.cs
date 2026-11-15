#nullable enable
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.SoundEditor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// Plays the cut's sound events (any event with an seId, type 135 in every game) as the playing clock passes their
/// start, as the Time Act Editor plays its sound events. The sound is the seId of the category its seType gives (the
/// TAE SoundType numbering, see <see cref="SoundReferenceHelper.TypeLetter" />), found with the Sound Editor's index;
/// one of the sounds its Play event can reach is played, picked at random as the game picks from a random container.
/// <list type="bullet">
/// <item>A sound plays to its end: the events are short (ER's c1801 steps last 15 frames), so their length is taken as
/// when they fire, not how long the sound lasts. Stopping the clock or changing cut stops them all.</item>
/// <item>The banks are read when the cut is shown, so they are usually ready when it plays. A sound still waiting on
/// its bank plays once it is ready, unless that took too long to still belong to its event.</item>
/// <item>Seeking plays nothing, only the clock running forward does. Looping back to the start counts as running on.</item>
/// </list>
/// </summary>
public class CutsceneSoundPlayback : IDisposable
{
    // Several sounds overlap in a cut (steps, cloth, impacts), each player plays one at a time
    private const int PlayerCount = 12;

    // A jump forward of more than this many frames while playing is taken as a seek, which plays nothing
    private const float MaxStepFrames = 15;

    // A sound still waiting on its bank after this long has missed its moment
    private const float PendingTimeout = 1.5f;

    private readonly CutsceneEditorScreen Editor;
    private readonly List<SoundPlayer> Players = new();
    private int NextPlayer;
    private readonly Random Random = new();

    // The sound events of the shown cut, and what each resolved to (null: no sound; missing: not resolved yet)
    private List<(MQB.Event Event, int Id, string? Letter)> Events = new();
    private readonly Dictionary<MQB.Event, SoundMatch?> Matches = new();
    private (CutsceneScene? Scene, int Version) EventsFor;
    private bool Prepared;

    private readonly List<(SoundMatch Match, float Age)> Pending = new();
    private float LastFrame;
    private bool WasPlaying;
    private bool WarnedVgmstream;
    private readonly HashSet<(int, string?)> Logged = new();

    public CutsceneSoundPlayback(CutsceneEditorScreen editor)
    {
        Editor = editor;
    }

    private ProjectEntry Project => Editor.Project;

    private bool IsAvailable => SoundReferenceHelper.IsAvailable(Project);

    public void Update(CutsceneScene? scene, float frame, bool playing)
    {
        RefreshEvents(scene);

        if (!CFG.Current.CutsceneEditor_Play_Sounds || !IsAvailable || scene == null)
        {
            if (WasPlaying)
                StopAll();
            LastFrame = frame;
            WasPlaying = false;
            return;
        }

        // The index builds on a worker the first time; the banks are read once it is
        if (!Prepared)
            Prepared = Prepare();

        UpdatePending();

        if (playing)
        {
            // Starting to play counts the current frame, so an event at 0 plays on the first pass
            var from = WasPlaying ? LastFrame : frame - 0.001f;

            if (frame >= from)
            {
                if (frame - from <= MaxStepFrames)
                    Trigger(from, frame);
            }
            else
            {
                // Looped back to the start (or a seek back, which is a loop as far as the events can tell)
                Trigger(-0.001f, frame);
            }
        }
        else if (WasPlaying)
        {
            StopAll();
        }

        LastFrame = frame;
        WasPlaying = playing;
    }

    private void RefreshEvents(CutsceneScene? scene)
    {
        var key = (scene, Editor.StructureVersion);
        if (EventsFor == key)
            return;

        // Another cut stops what the last one played; an edit of this one only finds its events again
        if (EventsFor.Scene != scene)
            StopAll();

        EventsFor = key;
        Events = new();
        Matches.Clear();
        Prepared = false;

        var mqb = scene?.File.Mqb;
        if (scene == null || mqb == null)
            return;

        foreach (var timeline in scene.Cut.Timelines)
        foreach (var e in timeline.Events)
        {
            var se = e.Parameters.FirstOrDefault(p => p.Name.Equals("seId", StringComparison.OrdinalIgnoreCase));
            if (se == null || CutsceneTalk.ToInt(se.Value) is not { } id || id <= 0)
                continue;

            var type = e.Parameters.FirstOrDefault(p => p.Name.Equals("seType", StringComparison.OrdinalIgnoreCase));
            var letter = type != null && CutsceneTalk.ToInt(type.Value) is { } t and >= 0 ? SoundReferenceHelper.TypeLetter(Project, t) : null;
            Events.Add((e, id, letter));
        }
    }

    /// <summary>Starts reading the banks the cut's sounds play from, so they're ready. False until the index is built.</summary>
    private bool Prepare()
    {
        var index = Project.Handler.SoundData?.Index;
        if (index == null)
            return false;

        index.EnsureBuilt();
        if (!index.IsBuilt)
            return index.Error != null;

        foreach (var (e, _, _) in Events)
            Resolve(e)?.Container.BeginLoad();

        return true;
    }

    private SoundMatch? Resolve(MQB.Event e)
    {
        if (Matches.TryGetValue(e, out var known))
            return known;

        if (Project.Handler.SoundData?.Index is not { IsBuilt: true })
            return null;

        var (_, id, letter) = Events.First(x => x.Event == e);
        var match = SoundReferenceHelper.Find(Project, id.ToString(), letter).FirstOrDefault();
        Matches[e] = match;

        if (CFG.Current.CutsceneEditor_Diagnostics_Log && match == null && Logged.Add((id, letter)))
            Smithbox.Log(this, LOC.Get("CUTSCENE_Sound_Log_Missing", id, letter ?? "?"));

        return match;
    }

    private void Trigger(float from, float to)
    {
        foreach (var (e, _, _) in Events)
        {
            if (e.StartFrame <= from || e.StartFrame > to)
                continue;

            if (Resolve(e) is not { } match)
                continue;

            if (match.Container.State is SoundLoadState.NotLoaded or SoundLoadState.Loading)
            {
                match.Container.BeginLoad();
                Pending.Add((match, 0));
                continue;
            }

            Play(match);
        }
    }

    private void UpdatePending()
    {
        if (Pending.Count == 0)
            return;

        var dt = ImGui.GetIO().DeltaTime;
        for (var i = Pending.Count - 1; i >= 0; i--)
        {
            var (match, age) = Pending[i];
            if (match.Container.State is SoundLoadState.NotLoaded or SoundLoadState.Loading)
            {
                age += dt;
                if (age > PendingTimeout)
                    Pending.RemoveAt(i);
                else
                    Pending[i] = (match, age);
                continue;
            }

            Pending.RemoveAt(i);
            Play(match);
        }
    }

    private void Play(SoundMatch match)
    {
        if (match.Container.State != SoundLoadState.Loaded)
            return;

        if (!VgmstreamCli.IsAvailable)
        {
            if (!WarnedVgmstream)
            {
                WarnedVgmstream = true;
                Smithbox.LogError(this, LOC.Get("SOUND_Log_No_Vgmstream", VgmstreamCli.ExePath));
            }
            return;
        }

        // The same lookup as the Sound Editor's sound/event command
        var hash = WwiseBank.Hash(match.EventName);
        var ev = match.Container.Events.FirstOrDefault(e => string.Equals(e.Name, match.EventName, StringComparison.OrdinalIgnoreCase))
                 ?? match.Container.Events.FirstOrDefault(e => !e.FromFev && e.Id == hash);
        var playable = ev?.Entries.Where(e => e.CanPlay).ToList();
        if (playable is not { Count: > 0 })
            return;

        var entry = playable[Random.Next(playable.Count)];
        var player = GetPlayer(entry.Source);
        player.Volume = CFG.Current.SoundEditor_Volume;
        player.Loop = false;
        player.Play(entry.Source);
    }

    // A free player that last played this sound (it replays from memory), else any free one, else the oldest
    private SoundPlayer GetPlayer(SoundSource source)
    {
        var free = Players.FirstOrDefault(p => !p.IsActive && p.Current?.CacheKey == source.CacheKey && p.Current?.Subsong == source.Subsong)
                   ?? Players.FirstOrDefault(p => !p.IsActive);
        if (free != null)
            return free;

        if (Players.Count < PlayerCount)
        {
            var player = new SoundPlayer();
            Players.Add(player);
            return player;
        }

        var oldest = Players[NextPlayer];
        NextPlayer = (NextPlayer + 1) % Players.Count;
        return oldest;
    }

    public void StopAll()
    {
        Pending.Clear();
        foreach (var player in Players)
        {
            if (player.IsActive)
                player.Stop();
        }
    }

    public void Dispose()
    {
        StopAll();
        foreach (var player in Players)
            player.Dispose();
        Players.Clear();
    }
}
