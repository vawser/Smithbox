#nullable enable
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Editors.Common;
using StudioCore.Editors.MetadataEditor;
using StudioCore.Editors.SoundEditor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// Plays the sounds of sound events as the playback clock passes their start, as the game does when the
/// animation plays. The event gives a sound type and ID; the Sound Editor's index finds the bank that holds
/// its Play event, and one of the sounds the event can reach is played (picked at random, as the game picks
/// from a random container).
/// <para>Banks are read when an animation is selected, so they are usually ready by the time it plays. A sound
/// whose bank is still loading plays once it is ready, unless that took too long to still belong to the event.</para>
/// <para>Seeking does not play anything: only the clock running forward does.</para>
/// </summary>
public class TimeActSoundPlayback : IDisposable
{
    // Several sounds can overlap, such as a swing and a footstep, and each player plays one at a time
    private const int PlayerCount = 8;

    // A jump forward of more than this while playing is taken as a seek, which plays nothing
    private const float MaxStep = 0.5f;

    // A sound still waiting on its bank after this long has missed its moment
    private const float PendingTimeout = 1.5f;

    private readonly TimeActEditorView _view;
    private readonly ProjectEntry _project;

    private readonly List<SoundPlayer> _players = new();
    private int _nextPlayer;

    private readonly Random _random = new();

    private TAE.Animation? _animation;
    private float _lastTime;
    private bool _wasPlaying;

    // The banks of the animation's sound events have been asked for
    private bool _prepared;

    // What each sound event of the animation resolved to, filled in as the index and banks become ready
    private readonly Dictionary<TAE.Event, SoundMatch?> _matches = new();

    private readonly List<(SoundMatch Match, float Age)> _pending = new();

    private bool _warnedVgmstream;

    public TimeActSoundPlayback(TimeActEditorView view, ProjectEntry project)
    {
        _view = view;
        _project = project;
    }

    private SoundIndex? Index => _project.Handler?.SoundData?.Index;

    /// <summary>
    /// Whether the project has the sound data to play from
    /// </summary>
    public bool IsAvailable => Index != null && EditorConditions.SupportsSoundEditor(_project.Descriptor.ProjectType);

    public void Update(float time, bool playing)
    {
        var animation = _view.Selection.SelectedAnimation;

        if (animation != _animation)
        {
            _animation = animation;
            _matches.Clear();
            _pending.Clear();
            StopAll();

            _prepared = false;
        }

        // Waits for the index, which builds on a worker the first time
        if (!_prepared)
            _prepared = Prepare();

        if (!CFG.Current.TimeActEditor_Play_Event_Sounds || !IsAvailable || animation == null)
        {
            _lastTime = time;
            _wasPlaying = playing;
            _pending.Clear();
            return;
        }

        UpdatePending();

        if (playing)
        {
            // Starting to play counts the current frame, so an event at 0 plays on the first pass
            var from = _wasPlaying ? _lastTime : time - 0.0001f;

            if (time >= from)
            {
                if (time - from <= MaxStep)
                    Trigger(animation, from, time);
            }
            else
            {
                // Looped back to the start (or a seek back, which is a loop as far as the events can tell)
                Trigger(animation, -0.0001f, time);
            }
        }

        _lastTime = time;
        _wasPlaying = playing;
    }

    /// <summary>
    /// Stops every sound that is playing, for when the playback stops or the animation changes
    /// </summary>
    public void StopAll()
    {
        _pending.Clear();

        foreach (var player in _players)
        {
            if (player.IsActive)
                player.Stop();
        }
    }

    /// <summary>
    /// Starts the index and reads the banks the sound events of the animation play from, so that they are ready
    /// </summary>
    private bool Prepare()
    {
        if (!CFG.Current.TimeActEditor_Play_Event_Sounds || !IsAvailable || _animation == null)
            return false;

        Index!.EnsureBuilt();

        if (!Index.IsBuilt)
            return Index.Error != null;

        foreach (var ev in _animation.Events)
        {
            if (Resolve(ev) is { } match)
                match.Container.BeginLoad();
        }

        return true;
    }

    private void Trigger(TAE.Animation animation, float from, float to)
    {
        foreach (var ev in animation.Events)
        {
            if (ev.StartTime <= from || ev.StartTime > to)
                continue;

            if (Resolve(ev) is not { } match)
                continue;

            if (match.Container.State is SoundLoadState.NotLoaded or SoundLoadState.Loading)
            {
                match.Container.BeginLoad();
                _pending.Add((match, 0));
                continue;
            }

            Play(match);
        }
    }

    private void UpdatePending()
    {
        if (_pending.Count == 0)
            return;

        var dt = ImGui.GetIO().DeltaTime;

        for (var i = _pending.Count - 1; i >= 0; i--)
        {
            var (match, age) = _pending[i];

            if (match.Container.State is SoundLoadState.NotLoaded or SoundLoadState.Loading)
            {
                age += dt;

                if (age > PendingTimeout)
                    _pending.RemoveAt(i);
                else
                    _pending[i] = (match, age);

                continue;
            }

            _pending.RemoveAt(i);
            Play(match);
        }
    }

    /// <summary>
    /// The Play event a sound event points at, or null if it isn't a sound event or the sound can't be found.
    /// Nothing is kept until the index is built, so the events resolve once it is.
    /// </summary>
    private SoundMatch? Resolve(TAE.Event ev)
    {
        if (_matches.TryGetValue(ev, out var known))
            return known;

        var index = Index;

        if (index == null || ev.Parameters == null)
            return null;

        index.EnsureBuilt();

        if (!index.IsBuilt)
            return null;

        SoundMatch? match = null;

        if (GetSound(ev) is { } sound)
        {
            var matches = index.Find(sound.Id.ToString(), sound.Letter);

            // A type the enum has no letter for, or one that matches nothing, tries every letter
            if (matches.Count == 0 && sound.Letter != null)
                matches = index.Find(sound.Id.ToString());

            match = matches.FirstOrDefault();
        }

        _matches[ev] = match;
        return match;
    }

    /// <summary>
    /// The sound ID and type letter of a sound event: the parameter the meta tags with the Sounds alias (else
    /// SoundID), and SoundType through its enum. Null if the event plays no sound.
    /// </summary>
    private (long Id, string? Letter)? GetSound(TAE.Event ev)
    {
        var data = _project.Handler.TimeActData;
        var values = ev.Parameters!.Values;

        var idName = data?.GetAliasParameter(ev, ProjectAliasType.Sounds);

        if (idName == null)
        {
            if (!(ev.TypeName ?? "").Contains("Sound", StringComparison.OrdinalIgnoreCase))
                return null;

            idName = "SoundID";
        }

        if (!values.TryGetValue(idName, out var idValue) || idValue is not IConvertible idNumber)
            return null;

        long id;

        try
        {
            id = idNumber.ToInt64(null);
        }
        catch
        {
            return null;
        }

        if (id <= 0)
            return null;

        string? letter = null;

        if (values.TryGetValue("SoundType", out var typeValue) && typeValue is IConvertible typeNumber)
        {
            try
            {
                letter = data?.GetSoundTypeLetter(typeNumber.ToInt32(null));
            }
            catch
            {
                letter = null;
            }
        }

        return (id, letter);
    }

    private void Play(SoundMatch match)
    {
        if (match.Container.State != SoundLoadState.Loaded)
            return;

        if (!VgmstreamCli.IsAvailable)
        {
            if (!_warnedVgmstream)
            {
                _warnedVgmstream = true;
                Smithbox.LogError(this, LOC.Get("SOUND_Log_No_Vgmstream", VgmstreamCli.ExePath));
            }

            return;
        }

        var hash = WwiseBank.Hash(match.EventName);
        var ev = match.Container.Events.FirstOrDefault(e => string.Equals(e.Name, match.EventName, StringComparison.OrdinalIgnoreCase))
                 ?? match.Container.Events.FirstOrDefault(e => !e.FromFev && e.Id == hash);

        if (ev == null)
            return;

        var playable = ev.Entries.Where(e => e.CanPlay).ToList();

        if (playable.Count == 0)
            return;

        var entry = playable[_random.Next(playable.Count)];

        var player = GetPlayer(entry.Source);
        player.Volume = CFG.Current.SoundEditor_Volume;
        player.Loop = false;
        player.Play(entry.Source);
    }

    /// <summary>
    /// A player for the sound: one that last played it and is free (it replays from memory), else any free one,
    /// else the one that has gone longest since it was picked.
    /// </summary>
    private SoundPlayer GetPlayer(SoundSource source)
    {
        var free = _players.FirstOrDefault(p => !p.IsActive && p.Current?.CacheKey == source.CacheKey && p.Current?.Subsong == source.Subsong)
                   ?? _players.FirstOrDefault(p => !p.IsActive);

        if (free != null)
            return free;

        if (_players.Count < PlayerCount)
        {
            var player = new SoundPlayer();
            _players.Add(player);
            return player;
        }

        var oldest = _players[_nextPlayer];
        _nextPlayer = (_nextPlayer + 1) % _players.Count;
        return oldest;
    }

    public void Dispose()
    {
        foreach (var player in _players)
            player.Dispose();

        _players.Clear();
    }
}
