#nullable enable
using Havok.Shared;
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Numerics;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// An undoable change to one cut Havok file: its bytes before and after. Marks the cutscene changed, and puts its
/// changed state back on undo.
/// </summary>
public sealed class CutsceneHavokEditAction : EditorAction
{
    private readonly CutsceneFile File;
    private readonly MQB.Resource Hkt;
    private readonly byte[] Before;
    private readonly byte[] After;
    private readonly bool WasDirty;

    public CutsceneHavokEditAction(CutsceneFile file, MQB.Resource hkt, byte[] before, byte[] after)
    {
        File = file;
        Hkt = hkt;
        Before = before;
        After = after;
        WasDirty = file.IsDirty;
    }

    public override ActionEvent Execute()
    {
        File.SetHavokFile(Hkt, After);
        File.IsDirty = true;
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        File.SetHavokFile(Hkt, Before);
        File.IsDirty = WasDirty;
        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Writes a cutscene camera's motion back into its cut Havok file: the camera's binding gets a new clip (a spline when
/// it holds the motion within tolerance, else interleaved, see <see cref="HavokClipWriter" />) and every other object
/// of the file stays as it was read. Checked with Claude/Tools/SplineRoundTrip --replace: putting the vanilla camera
/// back gives the vanilla bytes.
/// </summary>
public static class CutsceneCameraEdit
{
    /// <summary>The motion to write, in the camera's HKT space, for frames 0 to the clip's last</summary>
    public delegate HavokTransformData BoneSampler(float frame);

    /// <summary>Its FOV in radians at a frame</summary>
    public delegate float FovSampler(float frame);

    /// <summary>
    /// Builds the edit (not yet run) that gives the camera new motion and FOV. Null samplers keep what the clip has.
    /// </summary>
    public static (CutsceneHavokEditAction Action, HavokClipWriteResult Result) Build(ProjectEntry project,
        CutsceneFile file, CutsceneCamera camera, BoneSampler? bone, FovSampler? fov,
        HavokClipFormat format = HavokClipFormat.Auto)
    {
        var mqb = file.Mqb ?? throw new InvalidOperationException("The cutscene isn't loaded.");
        var clip = camera.Clip ?? throw new InvalidOperationException($"{camera.Name} has no clip.");
        if (camera.Resource.ParentIndex < 0 || camera.Resource.ParentIndex >= mqb.Resources.Count)
            throw new InvalidOperationException($"{camera.Name} has no HKT.");

        var hkt = mqb.Resources[camera.Resource.ParentIndex];
        var before = file.GetHavokFile(hkt) ?? throw new InvalidOperationException($"No Havok file for {hkt.Name}.");

        var trackCount = 0;
        foreach (var t in clip.BoneToTrack)
            trackCount = Math.Max(trackCount, t + 1);

        var floatCount = 0;
        foreach (var t in clip.SlotToFloatTrack)
            floatCount = Math.Max(floatCount, t + 1);

        // The bone and the FOV slot the camera plays from, as tracks
        var boneTrack = clip.BoneToTrack.Length > 0 ? clip.BoneToTrack[0] : -1;
        var fovTrack = camera.FovSlot >= 0 && camera.FovSlot < clip.SlotToFloatTrack.Length
            ? clip.SlotToFloatTrack[camera.FovSlot]
            : -1;

        HavokTransformData Sample(int track, float frame)
        {
            if (track == boneTrack && bone != null)
                return bone(frame);

            var x = clip.Sample(track, frame);
            return new HavokTransformData { Translation = x.Translation, Rotation = x.Rotation, Scale = x.Scale };
        }

        float SampleFloat(int track, float frame) =>
            track == fovTrack && fov != null
                ? fov(frame)
                : clip.SampleFloat(track, frame) ?? 0;

        var document = Havok.Files.HavokFile.Read(before, HavokAnimationReader.ProfileOf(project.Descriptor.ProjectType));
        var result = HavokClipWriter.ReplaceClip(document, camera.Name, clip.FrameCount, trackCount, floatCount,
            Sample, SampleFloat, clip.FrameDuration, format);
        var after = Havok.Files.HavokFile.Write(document);

        return (new CutsceneHavokEditAction(file, hkt, before, after), result);
    }

    /// <summary>
    /// The camera's whole path moved by an offset in its HKT space, and its FOV changed by some degrees: the simplest
    /// edit of the motion, for checking a written camera in the game.
    /// </summary>
    public static (CutsceneHavokEditAction Action, HavokClipWriteResult Result) Offset(ProjectEntry project,
        CutsceneFile file, CutsceneCamera camera, Vector3 offset, float fovDegrees, HavokClipFormat format)
    {
        var clip = camera.Clip ?? throw new InvalidOperationException($"{camera.Name} has no clip.");
        var boneTrack = clip.BoneToTrack.Length > 0 ? clip.BoneToTrack[0] : -1;
        var skeleton = camera.Skeleton;

        BoneSampler bone = frame =>
        {
            var x = boneTrack >= 0
                ? clip.Sample(boneTrack, frame)
                : skeleton != null && skeleton.BoneCount > 0 ? skeleton.ReferencePose[0].ToBlendable() : NewBlendableTransform.Identity;
            return new HavokTransformData { Translation = x.Translation + offset, Rotation = x.Rotation, Scale = x.Scale };
        };

        var fovRadians = fovDegrees * MathF.PI / 180f;
        FovSampler? fov = fovDegrees == 0 || skeleton == null || camera.FovSlot < 0
            ? null
            : frame => clip.SampleSlot(skeleton, camera.FovSlot, frame) + fovRadians;

        return Build(project, file, camera, bone, fov, format);
    }
}
