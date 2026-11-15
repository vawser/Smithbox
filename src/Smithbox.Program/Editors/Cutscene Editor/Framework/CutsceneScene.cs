#nullable enable
using SoulsFormats;
using StudioCore.Renderer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// The placement of one HKT of a cut: its timeline's parent is an offset.sib (resource type 265), whose transform keys
/// put the cut's animation in the world. Without one the HKT is at the origin.
/// </summary>
public sealed class CutscenePlacement
{
    public int HktResourceIndex;
    public string HktName = "";

    /// <summary>The offset.sib event, whose keys these are; null for a placement at the origin, which can't be moved</summary>
    public MQB.Event? OffsetEvent;

    /// <summary>The offset.sib event's keys, by frame: the MQB's own objects, so moving them edits the file. Empty for the origin.</summary>
    public List<MQB.Transform> Keys = new();

    public Matrix4x4 At(float frame)
    {
        if (Keys.Count == 0)
            return Matrix4x4.Identity;

        if (Keys.Count == 1 || frame <= Keys[0].Frame)
            return ToMatrix(Keys[0].Translation, Keys[0].Rotation, Keys[0].Scale);

        for (var i = 1; i < Keys.Count; i++)
        {
            var a = Keys[i - 1];
            var b = Keys[i];
            if (frame > b.Frame)
                continue;

            var span = b.Frame - a.Frame;
            var t = span > 0 ? (frame - a.Frame) / span : 0;
            return ToMatrix(Vector3.Lerp(a.Translation, b.Translation, t), Vector3.Lerp(a.Rotation, b.Rotation, t),
                Vector3.Lerp(a.Scale, b.Scale, t));
        }

        var last = Keys[^1];
        return ToMatrix(last.Translation, last.Rotation, last.Scale);
    }

    // Rotation in degrees, as MSB parts store theirs (see Entity, which reads them the same way)
    private static Matrix4x4 ToMatrix(Vector3 translation, Vector3 rotationDegrees, Vector3 scale)
    {
        var radians = rotationDegrees * (MathF.PI / 180f);
        if (scale == Vector3.Zero)
            scale = Vector3.One;

        return new Transform(translation, radians, scale).WorldMatrix;
    }
}

/// <summary>
/// One cut of a cutscene as the viewport plays it: the HKT Havok files it uses and where they are placed, its actors
/// and cameras, each paired with its skeleton and clip by name. Built from the MQB's timeline tree:
/// offset.sib (265) → HKT (266) → models (1) and cameras (3), whose resources name the HKT as their parent.
/// </summary>
public sealed class CutsceneScene : IDisposable
{
    public CutsceneFile File;
    public int CutIndex;
    public MQB.Cut Cut;
    public float Framerate;

    public List<CutsceneActor> Actors = new();
    public List<CutsceneCamera> Cameras = new();
    public Dictionary<int, CutscenePlacement> Placements = new();

    /// <summary>What went wrong building it, shown in the editor; empty if nothing did</summary>
    public List<string> Problems = new();

    public int FrameCount => Math.Max(1, Cut.Duration);

    private CutsceneScene(CutsceneFile file, int cutIndex)
    {
        File = file;
        CutIndex = cutIndex;
        Cut = file.Mqb!.Cuts[cutIndex];
        Framerate = file.Mqb.Framerate > 0 ? file.Mqb.Framerate : 30;
    }

    /// <summary>Reads the cut: safe to call on a worker thread. Models start loading in <see cref="StartLoads" />.</summary>
    public static CutsceneScene Prepare(ProjectEntry project, CutsceneFile file, int cutIndex)
    {
        var scene = new CutsceneScene(file, cutIndex);
        scene.Read(project);
        return scene;
    }

    private void Read(ProjectEntry project)
    {
        var mqb = File.Mqb!;
        var resources = mqb.Resources;

        MQB.Resource? ResourceOf(MQB.Event e) => e.ResourceIndex >= 0 && e.ResourceIndex < resources.Count ? resources[e.ResourceIndex] : null;

        // Placements: each HKT timeline, and the offset.sib of its parent timeline
        for (var t = 0; t < Cut.Timelines.Count; t++)
        {
            var timeline = Cut.Timelines[t];
            foreach (var e in timeline.Events)
            {
                if (ResourceOf(e) is not { Type: 266 } hkt || Placements.ContainsKey(e.ResourceIndex))
                    continue;

                var placement = new CutscenePlacement { HktResourceIndex = e.ResourceIndex, HktName = hkt.Name };

                if (timeline.ParentIndex >= 0 && timeline.ParentIndex < Cut.Timelines.Count)
                {
                    var offset = Cut.Timelines[timeline.ParentIndex].Events.FirstOrDefault(p => ResourceOf(p) is { Type: 265 });
                    if (offset != null)
                    {
                        placement.OffsetEvent = offset;
                        placement.Keys = offset.Transforms.OrderBy(k => k.Frame).ToList();
                    }
                }

                Placements[e.ResourceIndex] = placement;
            }
        }

        // The skeletons and clips of each HKT, read once
        var tracks = new Dictionary<int, Dictionary<string, HavokSceneTrack>>();
        foreach (var hktIndex in Placements.Keys)
        {
            var hkt = resources[hktIndex];
            var bytes = File.GetHavokFile(hkt);
            if (bytes == null)
            {
                Problems.Add(LOC.Get("CUTSCENE_Problem_No_Havok_File", hkt.Name, hkt.Path ?? ""));
                continue;
            }

            try
            {
                var byName = new Dictionary<string, HavokSceneTrack>(StringComparer.OrdinalIgnoreCase);
                foreach (var track in HavokAnimationReader.ReadScene(project.Descriptor.ProjectType, bytes, null))
                    byName.TryAdd(track.Skeleton.Name, track);
                tracks[hktIndex] = byName;
            }
            catch (Exception ex)
            {
                Problems.Add(LOC.Get("CUTSCENE_Problem_Havok_Read", hkt.Name, ex.Message));
            }
        }

        // Models and cameras with events in this cut
        var actors = new Dictionary<int, CutsceneActor>();
        var cameras = new Dictionary<int, CutsceneCamera>();

        foreach (var timeline in Cut.Timelines)
        {
            foreach (var e in timeline.Events)
            {
                var resource = ResourceOf(e);
                if (resource == null)
                    continue;

                if (resource.Type == 1)
                {
                    if (!actors.TryGetValue(e.ResourceIndex, out var actor))
                        actors[e.ResourceIndex] = actor = new CutsceneActor(e.ResourceIndex, resource);
                    actor.Events.Add(e);
                }
                else if (resource.Type == 3)
                {
                    if (!cameras.TryGetValue(e.ResourceIndex, out var camera))
                        cameras[e.ResourceIndex] = camera = new CutsceneCamera(e.ResourceIndex, resource);
                    camera.Events.Add(e);
                }
            }
        }

        HavokSceneTrack? TrackOf(MQB.Resource resource) =>
            tracks.TryGetValue(resource.ParentIndex, out var byName) && byName.TryGetValue(resource.Name, out var track) ? track : null;

        foreach (var actor in actors.Values)
        {
            var track = TrackOf(actor.Resource);
            actor.Skeleton = track?.Skeleton;
            actor.Clip = track?.Clip;
            if (track == null)
                Problems.Add(LOC.Get("CUTSCENE_Problem_No_Skeleton", actor.Name));
        }

        foreach (var camera in cameras.Values)
        {
            var track = TrackOf(camera.Resource);
            camera.Skeleton = track?.Skeleton;
            camera.Clip = track?.Clip;
            camera.FindFovSlot();
            camera.ReadEventParameters();
            if (track == null)
                Problems.Add(LOC.Get("CUTSCENE_Problem_No_Skeleton", camera.Name));
        }

        Actors = actors.Values.OrderBy(a => a.Name, StringComparer.OrdinalIgnoreCase).ToList();
        Cameras = cameras.Values.OrderBy(c => c.Events.Count > 0 ? c.Events.Min(e => e.StartFrame) : 0).ToList();
    }

    /// <summary>
    /// Finds again which events show each actor and camera, after an edit has moved, added or removed some. The cut's
    /// Havok data is unchanged, so nothing else is read again.
    /// </summary>
    public void RefreshEvents()
    {
        foreach (var actor in Actors)
            actor.Events.Clear();
        foreach (var camera in Cameras)
            camera.Events.Clear();

        var actors = Actors.ToDictionary(a => a.ResourceIndex);
        var cameras = Cameras.ToDictionary(c => c.ResourceIndex);

        foreach (var timeline in Cut.Timelines)
        {
            foreach (var e in timeline.Events)
            {
                if (actors.TryGetValue(e.ResourceIndex, out var actor))
                    actor.Events.Add(e);
                else if (cameras.TryGetValue(e.ResourceIndex, out var camera))
                    camera.Events.Add(e);
            }
        }
    }

    /// <summary>Starts loading the actors' models. Call on the main thread.</summary>
    public void StartLoads(ProjectEntry project, RenderScene? renderScene)
    {
        if (!CFG.Current.CutsceneEditor_Load_Models)
            return;

        foreach (var actor in Actors)
            actor.StartLoad(project, renderScene);
    }

    /// <summary>The placement of an actor's or camera's HKT at a frame.</summary>
    public Matrix4x4 RootOf(MQB.Resource resource, float frame) =>
        Placements.TryGetValue(resource.ParentIndex, out var placement) ? placement.At(frame) : Matrix4x4.Identity;

    /// <summary>The camera that shows the frame: the first whose events cover it, else the first.</summary>
    public CutsceneCamera? ActiveCamera(float frame) =>
        Cameras.FirstOrDefault(c => c.IsActiveAt(frame)) ?? Cameras.FirstOrDefault();

    public void Update(float frame)
    {
        foreach (var actor in Actors)
            actor.Update(frame, RootOf(actor.Resource, frame));
    }

    /// <summary>What the cut is built from, for the log: for checking the camera axes and placement.</summary>
    public string Describe()
    {
        var sb = new StringBuilder();
        sb.AppendLine($"[Cutscene Editor] {File.Name} {Cut.Name}: {FrameCount} frames at {Framerate} fps, {Actors.Count} actors, {Cameras.Count} cameras");

        foreach (var p in Placements.Values)
        {
            var keys = string.Join(" ", p.Keys.Select(k => $"[f{k.Frame} pos={k.Translation} rot={k.Rotation} scale={k.Scale}]"));
            sb.AppendLine($"  placement {p.HktName} (resource {p.HktResourceIndex}): {(p.Keys.Count == 0 ? "origin" : keys)}");
        }

        foreach (var c in Cameras)
        {
            var world = c.World(0, RootOf(c.Resource, 0));
            Matrix4x4.Decompose(world, out var scale, out var rotation, out var translation);
            sb.AppendLine($"  camera {c.Name}: skeleton={(c.Skeleton != null)} clip={c.Clip?.GetType().Name ?? "none"} fovSlot={c.FovSlot} " +
                          $"fov@0={c.FovRadians(0)?.ToString("0.###") ?? "none"} rad near={c.NearPlane} far={c.FarPlane} " +
                          $"frame0 pos={translation} rot={rotation} scale={scale}");
            sb.AppendLine($"    frame0 axes: +X={Vector3.TransformNormal(Vector3.UnitX, world)} +Y={Vector3.TransformNormal(Vector3.UnitY, world)} +Z={Vector3.TransformNormal(Vector3.UnitZ, world)}");
        }

        foreach (var a in Actors)
        {
            var root = a.RootWorld(0, RootOf(a.Resource, 0)).Translation;
            sb.AppendLine($"  actor {a.Name}: model={a.Model?.ModelName ?? "none"} bones={a.Skeleton?.BoneCount ?? 0} clip={a.Clip?.GetType().Name ?? "none"} " +
                          $"root@0={root} events={string.Join(",", a.Events.Select(e => $"{e.StartFrame}+{e.Duration}"))} state={a.State} " +
                          $"mappedNodes={a.MappedNodeCount}/{a.ModelNodeCount} {a.Status}");
        }

        foreach (var problem in Problems)
            sb.AppendLine($"  problem: {problem}");

        return sb.ToString();
    }

    public void Dispose()
    {
        foreach (var actor in Actors)
            actor.Dispose();
        Actors.Clear();
    }
}
