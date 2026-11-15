#nullable enable
using SoulsFormats;
using StudioCore.Editors.Common;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;

namespace StudioCore.Editors.CutsceneEditor;

/// <summary>
/// An undoable change to one camera's keys (mode, format, keys), as copies before and after. Marks the cutscene
/// changed, and puts its changed state back on undo. The viewport plays the camera from its keys straight away; they
/// are baked into the cut's Havok file on save.
/// </summary>
public sealed class CutsceneCameraKeysAction : EditorAction
{
    private readonly CutsceneFile File;
    private readonly string KeyId;
    private readonly CutsceneCameraKeys? Before;
    private readonly CutsceneCameraKeys After;
    private readonly bool WasDirty;

    public CutsceneCameraKeysAction(CutsceneFile file, string keyId, CutsceneCameraKeys? before, CutsceneCameraKeys after)
    {
        File = file;
        KeyId = keyId;
        Before = before?.Clone();
        After = after.Clone();
        WasDirty = file.IsDirty;
    }

    public override ActionEvent Execute()
    {
        File.SetCameraKeys(KeyId, After.Clone());
        File.IsDirty = true;
        return ActionEvent.NoEvent;
    }

    public override ActionEvent Undo()
    {
        File.SetCameraKeys(KeyId, Before?.Clone());
        File.IsDirty = WasDirty;
        return ActionEvent.NoEvent;
    }
}

/// <summary>
/// Bakes cameras into a cut Havok file: each camera's binding gets a new clip from its keys over the game's own
/// camera (a spline when it holds the motion within tolerance, else interleaved, see <see cref="HavokClipWriter" />),
/// and every other object of the file stays as it was read. Checked with Claude/Tools/SplineRoundTrip --replace:
/// putting the vanilla camera back gives the vanilla bytes. Tested in the game (phase 5b).
/// </summary>
public static class CutsceneCameraEdit
{
    /// <summary>
    /// The cut Havok file with the given cameras baked from their keys, starting from <paramref name="baseFile" />
    /// (the game's own, so keys never stack on an earlier bake).
    /// </summary>
    public static (byte[] Bytes, List<(string Camera, HavokClipWriteResult Result)> Results) Bake(ProjectType game,
        byte[] baseFile, IEnumerable<(string CameraName, CutsceneCameraKeys Keys)> cameras)
    {
        var scene = HavokAnimationReader.ReadScene(game, baseFile, null);
        var document = Havok.Files.HavokFile.Read(baseFile, HavokAnimationReader.ProfileOf(game));
        var results = new List<(string, HavokClipWriteResult)>();

        foreach (var (name, keys) in cameras)
        {
            var track = scene.FirstOrDefault(t => string.Equals(t.Skeleton.Name, name, StringComparison.OrdinalIgnoreCase));
            if (track?.Clip is not { } clip)
                throw new InvalidOperationException($"{name} has no skeleton and clip in the cut's Havok file.");

            var skeleton = track.Skeleton;
            var trackCount = clip.BoneToTrack.DefaultIfEmpty(-1).Max() + 1;
            var floatCount = clip.SlotToFloatTrack.DefaultIfEmpty(-1).Max() + 1;
            var boneTrack = clip.BoneToTrack.Length > 0 ? clip.BoneToTrack[0] : -1;
            var fovSlot = Array.FindIndex(skeleton.FloatSlots, s => s.EndsWith("_FOV", StringComparison.OrdinalIgnoreCase));
            var fovTrack = fovSlot >= 0 && fovSlot < clip.SlotToFloatTrack.Length ? clip.SlotToFloatTrack[fovSlot] : -1;

            HavokTransformData BaseBone(float frame)
            {
                var x = clip.Sample(boneTrack, frame);
                return new HavokTransformData { Translation = x.Translation, Rotation = x.Rotation, Scale = x.Scale };
            }

            float BaseFov(float frame) => fovSlot >= 0 ? clip.SampleSlot(skeleton, fovSlot, frame) : 0;

            HavokTransformData Sample(int t, float frame)
            {
                if (t == boneTrack)
                    return keys.Evaluate(frame, BaseBone(frame), BaseFov(frame)).Bone;

                var x = clip.Sample(t, frame);
                return new HavokTransformData { Translation = x.Translation, Rotation = x.Rotation, Scale = x.Scale };
            }

            float SampleFloat(int t, float frame) => t == fovTrack
                ? keys.Evaluate(frame, BaseBone(frame), BaseFov(frame)).Fov
                : clip.SampleFloat(t, frame) ?? 0;

            if (boneTrack < 0)
                throw new InvalidOperationException($"{name}'s bone has no track to write.");

            var result = HavokClipWriter.ReplaceClip(document, skeleton.Name, clip.FrameCount, trackCount, floatCount,
                Sample, SampleFloat, clip.FrameDuration, keys.Format);

            results.Add((name, result));
        }

        return (Havok.Files.HavokFile.Write(document), results);
    }
}
