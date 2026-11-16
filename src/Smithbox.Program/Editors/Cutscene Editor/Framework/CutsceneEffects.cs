#nullable enable
using SoulsFormats;
using StudioCore.Editors.FxEditor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// The cut's effect events in the viewport, drawn by the shared FX display (<see cref="FxDisplay"/>, as the FX Editor
/// draws its preview). An effect event is any event with an sfxId (type 103, SFX, in every game):
/// <list type="bullet">
/// <item>It runs from the event's start, and is told to stop at its end (its particles then die out as the effect's
/// Termination says). Scrubbing or looping back replays it, so a frame always shows the same particles.</item>
/// <item>It is placed on the dmypolyId dummy of the actor whose timeline holds the event, following the bone the
/// dummy is on; with no dummy (-1), or before the actor's model has loaded, on the actor's root. An event under an
/// offset.sib follows that placement; any other is at the world origin. The event's own transform is applied on
/// top.</item>
/// <item>The effects come from the project's FX bank (with any unsaved FX Editor edits), so the FX Editor has to
/// be enabled for the project. The bank is indexed in the background the first time a cut with effects is shown.</item>
/// </list>
/// </summary>
public sealed class CutsceneEffects : IDisposable
{
    private readonly CutsceneEditorView Editor;
    private readonly FxDisplay? Display;
    private readonly FxDefCache Defs;
    private readonly FxCuePlayer Player;

    // What the cues were made for: an edit, or an actor's rig arriving (its dummies), makes them again
    private (CutsceneScene? Scene, int Version, int Rigs) CuesFor;

    private readonly HashSet<int> LoggedMissing = new();
    private bool MissingLogged;

    public CutsceneEffects(CutsceneEditorView editor)
    {
        Editor = editor;
        var project = editor.Project;

        Defs = new FxDefCache(() => project.Handler.FxData);
        Player = new FxCuePlayer(Defs);

        if (editor.RenderScene != null && project.Handler.FxData != null)
        {
            Display = new FxDisplay(new FxDisplayHost(project, () => editor.RenderScene!,
                () => FxDisplayHost.PipelineOf(editor.ViewportWindow?.Viewport)));
        }
    }

    /// <summary>Whether the project has effects to show (the FX Editor is enabled for it)</summary>
    public bool IsAvailable => Display?.IsAvailable == true;

    public FxBank? Bank => Editor.Project.Handler.FxData?.Bank;

    /// <summary>Effect events in the cut, effects running now, events whose effect the bank hasn't got, particles</summary>
    public int EventCount => Player.CueCount;
    public int RunningCount => Player.Instances.Count;
    public int MissingCount => Player.MissingCount;
    public int ParticleCount => Player.ParticleCount;

    /// <summary>Whether the cut's effects are still loading: the clock waits, so their first showing doesn't stall</summary>
    public bool IsLoading => Player.IsLoading;
    public int LoadingCount => Player.Preload.Count;
    public int LoadedCount => Player.Preload.ReadyCount;

    /// <param name="frame">The cut's frame</param>
    /// <param name="eye">The viewport camera's position and forward direction, for sorting and facing particles</param>
    public void Update(CutsceneScene? scene, float frame, Vector3 eye, Vector3 forward)
    {
        if (!CFG.Current.CutsceneEditor_Display_Effects || scene == null || !IsAvailable)
        {
            if (CuesFor.Scene != null || Player.CueCount > 0)
            {
                Player.Clear();
                Display?.Clear();
                CuesFor = default;
            }
            return;
        }

        var key = (scene, Editor.StructureVersion, scene.Actors.Count(a => a.Rig != null));
        if (key != CuesFor)
        {
            CuesFor = key;
            Player.SetCues(BuildCues(scene));
            MissingLogged = false;
        }

        // The bank is read only once a cut has something to show from it (indexing reads every sfx binder)
        if (Player.CueCount > 0 && Bank is { } bank)
        {
            bank.StartIndexing();
            bank.Update();
        }

        Player.Prepare(Display);
        Player.Update(frame / scene.Framerate, eye);
        Display!.Update(Player.Instances, eye, forward);

        if (CFG.Current.CutsceneEditor_Diagnostics_Log && Bank is { IsIndexed: true })
            LogMissing();
    }

    private List<FxCue> BuildCues(CutsceneScene scene)
    {
        var cues = new List<FxCue>();
        var cut = scene.Cut;
        var fps = scene.Framerate;
        var actors = scene.Actors.ToDictionary(a => a.ResourceIndex);
        var log = CFG.Current.CutsceneEditor_Diagnostics_Log;

        for (var t = 0; t < cut.Timelines.Count; t++)
        {
            var timeline = cut.Timelines[t];
            foreach (var e in timeline.Events)
            {
                if (Parameter(e, "sfxId") is not { } sfx || sfx <= 0)
                    continue;

                var dummy = Parameter(e, "dmypolyId") ?? -1;
                var local = e.Transforms.Count > 0
                    ? CutscenePlacement.ToMatrix(e.Transforms[0].Translation, e.Transforms[0].Rotation, e.Transforms[0].Scale)
                    : Matrix4x4.Identity;

                // What the event's timeline hangs from: an actor, or a cut placement
                CutsceneActor? actor = null;
                CutscenePlacement? placement = null;
                if (timeline.ParentIndex >= 0 && timeline.ParentIndex < cut.Timelines.Count)
                {
                    var parent = cut.Timelines[timeline.ParentIndex];
                    actor = parent.Events.Select(p => actors.GetValueOrDefault(p.ResourceIndex)).FirstOrDefault(a => a != null);
                    if (actor == null)
                        placement = scene.Placements.Values.FirstOrDefault(p => p.OffsetEvent != null && parent.Events.Contains(p.OffsetEvent));
                }

                Func<float, Matrix4x4> worldAt = actor != null
                    ? s => { var f = s * fps; return local * actor.AnchorWorld(dummy, f, scene.RootOf(actor.Resource, f)); }
                    : placement != null
                        ? s => local * placement.At(s * fps)
                        : _ => local;

                cues.Add(new FxCue
                {
                    EffectId = sfx,
                    Start = e.StartFrame / fps,
                    End = (e.StartFrame + Math.Max(1, e.Duration)) / fps,
                    WorldAt = worldAt,
                    Tag = e,
                    // Two events of one effect don't show the same particles
                    Seed = (uint)HashCode.Combine(sfx, e.ID, e.StartFrame) | 1,
                });

                if (log)
                {
                    var anchor = actor != null
                        ? $"actor {actor.Name} dummy {dummy} ({(actor.Rig == null ? "no rig yet: root" : actor.Rig.FindDummy(dummy) != null ? "found" : "not in model: root")})"
                        : placement != null ? $"placement {placement.HktName}" : "origin";
                    Smithbox.Log(this, $"[Cutscene Editor] {scene.File.Name} {cut.Name}: effect {sfx} event {e.ID} " +
                                       $"frames {e.StartFrame}+{e.Duration} on {anchor}, timeline {t} (parent {timeline.ParentIndex})");
                }
            }
        }

        return cues;
    }

    private void LogMissing()
    {
        if (MissingLogged)
            return;
        MissingLogged = true;

        foreach (var cue in EnumerateMissing())
            if (LoggedMissing.Add(cue))
                Smithbox.Log(this, $"[Cutscene Editor] Effect {cue} isn't in the project's FX bank");
    }

    private IEnumerable<int> EnumerateMissing()
    {
        if (CuesFor.Scene is not { } scene)
            yield break;

        foreach (var timeline in scene.Cut.Timelines)
        foreach (var e in timeline.Events)
            if (Parameter(e, "sfxId") is { } sfx and > 0 && Defs.Has(sfx) == false)
                yield return sfx;
    }

    private static int? Parameter(MQB.Event e, string name)
    {
        var p = e.Parameters.FirstOrDefault(p => p.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        return p == null ? null : CutsceneTalk.ToInt(p.Value);
    }

    /// <summary>Drops what is drawn (another cut, or effects turned off)</summary>
    public void Clear()
    {
        Player.Clear();
        Display?.Clear();
        CuesFor = default;
    }

    public void Dispose()
    {
        Player.Clear();
        Display?.Dispose();
    }
}
