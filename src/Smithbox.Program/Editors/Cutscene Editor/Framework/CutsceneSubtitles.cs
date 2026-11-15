#nullable enable
using Hexa.NET.ImGui;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Editors.SoundEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// The cut's subtitle events (resource types 27 and 28, which take a talkParamId) as the game shows them: the
/// TalkParam's text at the bottom centre of the viewport while an event covers the playhead, and the TalkParam's voice
/// (a (v) sound, see <see cref="CutsceneTalk.Voice" />) played from when the playing clock enters the event until it
/// leaves it, the clock stops, or the cut changes.
/// <para>The voice starts from its beginning: when playback starts part way into an event, the line starts there.
/// Seeking while stopped plays nothing.</para>
/// </summary>
public class CutsceneSubtitles : IDisposable
{
    /// <summary>The MQB resource types whose events show a subtitle (Subtitle_N, and the faded kind)</summary>
    public static readonly int[] SubtitleTypes = [27, 28];

    private readonly CutsceneEditorScreen Editor;
    private readonly CutsceneTalk Talk;

    // A TalkParam's text and voice, read again now and then so edits in the Param and Text Editors show
    private readonly Dictionary<int, (string? Text, (int Id, string Field)? Voice, DateTime At)> TalkCache = new();
    private static readonly TimeSpan TalkCacheLife = TimeSpan.FromSeconds(2);

    // The subtitle events of the shown cut, with their talkParamId
    private List<(MQB.Event Event, int TalkId)> Events = new();
    private (CutsceneScene? Scene, int Version) EventsFor;

    // The voice: which event it belongs to, the sound it waits on or plays, and the player
    private MQB.Event? VoiceEvent;
    private SoundMatch? VoiceMatch;
    private bool VoiceStarted;
    private SoundPlayer? Player;
    private bool WarnedVgmstream;
    private readonly HashSet<int> Logged = new();

    public CutsceneSubtitles(CutsceneEditorScreen editor)
    {
        Editor = editor;
        Talk = new CutsceneTalk(editor.Project);
    }

    /// <summary>The subtitle events covering a frame, in timeline order</summary>
    public IEnumerable<(MQB.Event Event, int TalkId)> ActiveAt(float frame) =>
        Events.Where(e => frame >= e.Event.StartFrame && frame < e.Event.StartFrame + Math.Max(1, e.Event.Duration));

    /// <summary>Each frame: finds the cut's subtitle events, and starts or stops the voice</summary>
    public void Update(CutsceneScene? scene, float frame, bool playing)
    {
        RefreshEvents(scene);

        var active = ActiveAt(frame).FirstOrDefault(e => Entry(e.TalkId).Voice != null);
        var activeEvent = playing && CFG.Current.CutsceneEditor_Play_Voices ? active.Event : null;

        // Leaving the event, stopping, or another line: the voice stops
        if (VoiceEvent != null && (activeEvent != VoiceEvent || !playing))
            StopVoice();

        // Entering an event while playing, or starting to play inside one. A line plays once per pass through its
        // event: VoiceEvent stays set after its sound ends, until the event is left.
        if (activeEvent != null && VoiceEvent == null)
            StartVoice(active.Event, active.TalkId);

        TickVoice();
    }

    private void RefreshEvents(CutsceneScene? scene)
    {
        var key = (scene, Editor.StructureVersion);
        if (EventsFor == key)
            return;

        EventsFor = key;
        StopVoice();
        Events = new();

        var mqb = scene?.File.Mqb;
        if (scene == null || mqb == null)
            return;

        foreach (var timeline in scene.Cut.Timelines)
        foreach (var e in timeline.Events)
        {
            if (e.ResourceIndex < 0 || e.ResourceIndex >= mqb.Resources.Count || !SubtitleTypes.Contains(mqb.Resources[e.ResourceIndex].Type))
                continue;

            var talk = e.Parameters.FirstOrDefault(p => p.Name.Equals("talkParamId", StringComparison.OrdinalIgnoreCase));
            if (talk != null && CutsceneTalk.ToInt(talk.Value) is { } id and > 0)
                Events.Add((e, id));
        }
    }

    private (string? Text, (int Id, string Field)? Voice) Entry(int talkId)
    {
        if (TalkCache.TryGetValue(talkId, out var cached) && DateTime.UtcNow - cached.At < TalkCacheLife)
            return (cached.Text, cached.Voice);

        var row = Talk.Row(talkId);
        var text = row != null && Talk.Text(row)?.Entry?.Text is { Length: > 0 } t ? t : null;
        var voice = row != null ? Talk.Voice(row) : null;
        TalkCache[talkId] = (text, voice, DateTime.UtcNow);
        return (text, voice);
    }

    // ---- the voice

    private void StartVoice(MQB.Event e, int talkId)
    {
        VoiceEvent = e;
        VoiceMatch = null;
        VoiceStarted = false;

        if (Entry(talkId).Voice is not { } voice || !SoundReferenceHelper.IsAvailable(Editor.Project))
            return;

        var matches = SoundReferenceHelper.Find(Editor.Project, voice.Id.ToString(), CutsceneTalk.VoiceLetter);
        VoiceMatch = matches.FirstOrDefault();
        VoiceMatch?.Container.BeginLoad();

        if (CFG.Current.CutsceneEditor_Diagnostics_Log && Logged.Add(talkId))
            Smithbox.Log(this, LOC.Get("CUTSCENE_Voice_Log", talkId, voice.Field, voice.Id,
                VoiceMatch?.Label ?? LOC.Get(Editor.Project.Handler.SoundData?.Index?.IsBuilt == true ? "CUTSCENE_Voice_Log_No_Event" : "CUTSCENE_Voice_Log_Indexing")));
    }

    /// <summary>Plays the voice once its bank has loaded, if its event still covers the playhead</summary>
    private void TickVoice()
    {
        if (VoiceEvent == null || VoiceStarted)
            return;

        // The index may still be building when the event starts: look again until it is
        if (VoiceMatch == null)
        {
            var talk = Events.FirstOrDefault(x => x.Event == VoiceEvent).TalkId;
            if (Entry(talk).Voice is { } voice && Editor.Project.Handler.SoundData?.Index is { IsBuilt: true } && SoundReferenceHelper.IsAvailable(Editor.Project))
            {
                VoiceMatch = SoundReferenceHelper.Find(Editor.Project, voice.Id.ToString(), CutsceneTalk.VoiceLetter).FirstOrDefault();
                VoiceMatch?.Container.BeginLoad();
                if (VoiceMatch == null)
                    VoiceStarted = true; // nothing to play for this line
            }
            return;
        }

        var container = VoiceMatch.Container;
        if (container.State is SoundLoadState.NotLoaded or SoundLoadState.Loading)
            return;

        VoiceStarted = true;
        if (container.State != SoundLoadState.Loaded)
            return; // the bank logged why

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
        var hash = WwiseBank.Hash(VoiceMatch.EventName);
        var ev = container.Events.FirstOrDefault(x => string.Equals(x.Name, VoiceMatch.EventName, StringComparison.OrdinalIgnoreCase))
                 ?? container.Events.FirstOrDefault(x => !x.FromFev && x.Id == hash);
        var entry = ev?.Entries.FirstOrDefault(x => x.CanPlay);
        if (entry == null)
            return;

        Player ??= new SoundPlayer();
        Player.Volume = CFG.Current.SoundEditor_Volume;
        Player.Loop = false;
        Player.Play(entry.Source);
    }

    public void StopVoice()
    {
        VoiceEvent = null;
        VoiceMatch = null;
        VoiceStarted = false;
        if (Player?.IsActive == true)
            Player.Stop();
    }

    // ---- the text

    /// <summary>
    /// Draws the subtitles covering the playhead at the bottom centre of the viewport, wrapped to most of its width,
    /// on a dark band. Called from the viewport's overlay, inside its window.
    /// </summary>
    public void DrawOverlay(float x, float y, float width, float height)
    {
        if (!CFG.Current.CutsceneEditor_Display_Subtitles || Editor.Scene == null || width <= 0 || height <= 0)
            return;

        var lines = new List<string>();
        foreach (var (_, talkId) in ActiveAt(Editor.Playback.Frame))
        {
            if (Entry(talkId).Text is { } text)
                lines.AddRange(Wrap(text, width * 0.7f));
        }

        if (lines.Count == 0)
            return;

        var drawList = ImGui.GetWindowDrawList();
        var lineHeight = ImGui.GetTextLineHeightWithSpacing();
        var widest = lines.Max(l => ImGui.CalcTextSize(l).X);
        var padding = new Vector2(12, 6);

        // Above the bottom edge, clear of the resource load panel and the scale bar
        var bottom = y + height - 48;
        var top = bottom - lines.Count * lineHeight;
        var centre = x + width / 2;

        drawList.AddRectFilled(new Vector2(centre - widest / 2, top) - padding, new Vector2(centre + widest / 2, bottom) + padding,
            ImGui.GetColorU32(new Vector4(0, 0, 0, 0.55f)), 4);

        var shadow = ImGui.GetColorU32(new Vector4(0, 0, 0, 1));
        var color = ImGui.GetColorU32(new Vector4(1, 1, 1, 1));
        for (var i = 0; i < lines.Count; i++)
        {
            var pos = new Vector2(centre - ImGui.CalcTextSize(lines[i]).X / 2, top + i * lineHeight);
            drawList.AddText(pos + Vector2.One, shadow, lines[i]);
            drawList.AddText(pos, color, lines[i]);
        }
    }

    /// <summary>Breaks text into lines no wider than a width: at its own line breaks, then between words</summary>
    private static IEnumerable<string> Wrap(string text, float maxWidth)
    {
        foreach (var paragraph in text.Replace("\r", "").Split('\n'))
        {
            var line = "";
            foreach (var word in paragraph.Split(' '))
            {
                var next = line.Length == 0 ? word : $"{line} {word}";
                if (line.Length > 0 && ImGui.CalcTextSize(next).X > maxWidth)
                {
                    yield return line;
                    line = word;
                }
                else
                {
                    line = next;
                }
            }

            yield return line;
        }
    }

    public void Dispose()
    {
        StopVoice();
        Player?.Dispose();
        Player = null;
    }
}
