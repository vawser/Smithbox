#nullable enable
using Havok;
using Havok.Shared;
using SoulsFormats;
using StudioCore.Application;
using StudioCore.Renderer;
using StudioCore.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace StudioCore.Editors.TimeActEditor;

/// <summary>
/// One step of a combo as it is baked: the stretch of its clip it plays (in seconds of the clip), the animation its
/// events come from, and how long it crossfades in from the step before (0 for none)
/// </summary>
public sealed record TimeActComboBakeStep(TAE.Animation Animation, HavokClip Clip, float Start, float End, float BlendIn)
{
    public float Length => Math.Max(End - Start, 0);
}

/// <summary>What <see cref="TimeActComboBake.BakeClip" /> wrote.</summary>
public sealed record TimeActComboBakeResult(byte[] Bytes, float Duration, int FrameCount, bool HasRootMotion,
    HavokClipWriteResult Write);

/// <summary>
/// Bakes a combo into one clip, as the Combo Builder plays it: each step plays its stretch of its clip one after
/// another, crossfading in from where the last step got to over its blend, with the root motion of each carrying on
/// from where the last one left the character. The root motion is baked unscaled, without the editor's scale
/// settings or the turn of TurnSpeed events (the game turns the baked animation by its events, as it did the steps).
/// </summary>
public static class TimeActComboBake
{
    private const float DefaultFrameDuration = 1f / 30f;

    /// <summary>
    /// The HKX file of the first step's clip with its animation (and root motion) replaced by the combo. The rest of
    /// the file (the binding, its annotations) is kept from that clip, so it binds to the skeleton the same way.
    /// </summary>
    /// <param name="baseFile">The first step's HKX file, decompressed</param>
    /// <param name="compendium">The type compendium of the binder the file is from, for a tagfile that uses one</param>
    public static TimeActComboBakeResult BakeClip(ProjectType game, byte[] baseFile, HavokCompendium? compendium,
        HavokSkeletonData skeleton, IReadOnlyList<TimeActComboBakeStep> steps)
    {
        if (steps.Count == 0)
            throw new ArgumentException("The combo has no steps.", nameof(steps));

        var document = Havok.Files.HavokFile.Read(baseFile, HavokAnimationReader.ProfileOf(game), compendium);

        var binding = document.ObjectsOf<IHavokAnimationBinding>().FirstOrDefault()
                      ?? throw new InvalidOperationException("The first step's file has no animation binding.");

        var animation = binding.Animation
                        ?? throw new InvalidOperationException("The first step's file has no animation.");

        var trackToBone = binding.TransformTrackToBoneIndices;
        var floatToSlot = binding.FloatTrackToFloatSlotIndices;

        var trackCount = trackToBone.Count > 0 ? trackToBone.Count : animation.TransformTrackCount;
        var floatCount = floatToSlot.Count > 0 ? floatToSlot.Count : animation.FloatTrackCount;

        var frameDuration = steps[0].Clip.FrameDuration > 0 ? steps[0].Clip.FrameDuration : DefaultFrameDuration;

        // Where each step starts in the combo
        var offsets = new float[steps.Count];
        var total = 0f;

        for (var i = 0; i < steps.Count; i++)
        {
            offsets[i] = total;
            total += steps[i].Length;
        }

        if (total <= 0)
            throw new InvalidOperationException("The combo plays for no time.");

        var frameCount = Math.Max((int)MathF.Round(total / frameDuration) + 1, 2);
        var duration = (frameCount - 1) * frameDuration;

        // The step playing at a time of the combo, and how far into it the time is
        int Locate(float time, out float elapsed)
        {
            time = Math.Clamp(time, 0, total);

            for (var i = steps.Count - 1; i > 0; i--)
            {
                if (time >= offsets[i])
                {
                    elapsed = time - offsets[i];
                    return i;
                }
            }

            elapsed = time;
            return 0;
        }

        int BoneOf(int track) => trackToBone.Count == 0 ? track : track < trackToBone.Count ? trackToBone[track] : -1;
        int SlotOf(int track) => floatToSlot.Count == 0 ? track : track < floatToSlot.Count ? floatToSlot[track] : -1;

        NewBlendableTransform SampleBone(HavokClip clip, int track, int bone, float time)
        {
            var frame = ClipFrame(clip, time);

            if (bone < 0)
                return steps[0].Clip.Sample(track, 0);

            var clipTrack = bone < clip.BoneToTrack.Length ? clip.BoneToTrack[bone] : -1;

            if (clipTrack < 0)
                return bone < skeleton.ReferencePose.Length ? skeleton.ReferencePose[bone].ToBlendable() : NewBlendableTransform.Identity;

            return clip.Sample(clipTrack, frame);
        }

        HavokTransformData SampleTransform(int track, float frame)
        {
            var bone = BoneOf(track);
            var index = Locate(frame * frameDuration, out var elapsed);
            var step = steps[index];

            var local = SampleBone(step.Clip, track, bone, step.Start + elapsed);

            // The last step plays on as it fades out, as the viewport's crossfade has it
            if (index > 0 && step.BlendIn > 0 && elapsed < step.BlendIn)
            {
                var previous = steps[index - 1];
                var weight = 1 - elapsed / step.BlendIn;
                var other = SampleBone(previous.Clip, track, bone, Math.Min(previous.End + elapsed, previous.Clip.Duration));

                local = new NewBlendableTransform
                {
                    Translation = Vector3.Lerp(local.Translation, other.Translation, weight),
                    Rotation = Quaternion.Slerp(local.Rotation, other.Rotation, weight),
                    Scale = Vector3.Lerp(local.Scale, other.Scale, weight)
                };
            }

            return new HavokTransformData { Translation = local.Translation, Rotation = local.Rotation, Scale = local.Scale };
        }

        float SampleSlot(HavokClip clip, int slot, float time)
        {
            return slot >= 0 ? clip.SampleSlot(skeleton, slot, ClipFrame(clip, time)) : 0;
        }

        float SampleFloat(int track, float frame)
        {
            var slot = SlotOf(track);
            var index = Locate(frame * frameDuration, out var elapsed);
            var step = steps[index];

            var value = SampleSlot(step.Clip, slot, step.Start + elapsed);

            if (index > 0 && step.BlendIn > 0 && elapsed < step.BlendIn)
            {
                var previous = steps[index - 1];
                var weight = 1 - elapsed / step.BlendIn;
                var other = SampleSlot(previous.Clip, slot, Math.Min(previous.End + elapsed, previous.Clip.Duration));

                value += (other - value) * weight;
            }

            return value;
        }

        var referenceFrame = BakeRootMotion(document, steps, offsets, total, frameCount, frameDuration, duration);

        var write = HavokClipWriter.ReplaceClip(document, null, frameCount, trackCount, floatCount, SampleTransform,
            floatCount > 0 ? SampleFloat : null, frameDuration, HavokClipFormat.Auto, SplineEncodeSettings.ForCharacterClips(), referenceFrame);

        return new TimeActComboBakeResult(Havok.Files.HavokFile.Write(document), duration, frameCount,
            referenceFrame != null, write);
    }

    private static float ClipFrame(HavokClip clip, float time)
    {
        var frameDuration = clip.FrameDuration > 0 ? clip.FrameDuration : DefaultFrameDuration;

        return Math.Clamp(time / frameDuration, 0, Math.Max(clip.FrameCount - 1, 0));
    }

    /// <summary>
    /// The root motion of the combo, a sample a frame: each step's motion from its start, carried on from where the
    /// last step left the character (as the Combo Builder carries it). Null when no step's clip moves the character.
    /// </summary>
    private static HavokReferenceFrameContent? BakeRootMotion(HavokDocument document,
        IReadOnlyList<TimeActComboBakeStep> steps, float[] offsets, float total, int frameCount, float frameDuration,
        float duration)
    {
        if (steps.All(s => s.Clip.RootMotion == null))
            return null;

        var up = steps.First(s => s.Clip.RootMotion != null).Clip.RootMotion!.Up;

        // Where each step starts the character, as a matrix and its turn so far
        var carried = new Matrix4x4[steps.Count];
        var carriedTurn = new float[steps.Count];

        var place = Matrix4x4.Identity;
        var turn = 0f;

        for (var i = 0; i < steps.Count; i++)
        {
            carried[i] = place;
            carriedTurn[i] = turn;

            var (relative, relativeTurn) = Relative(steps[i], steps[i].End);

            place = relative * place;
            turn += relativeTurn;
        }

        var samples = new Vector4[frameCount];

        for (var f = 0; f < frameCount; f++)
        {
            var time = Math.Clamp(f * frameDuration, 0, total);
            var index = steps.Count - 1;

            while (index > 0 && time < offsets[index])
                index--;

            var step = steps[index];
            var (relative, relativeTurn) = Relative(step, step.Start + time - offsets[index]);
            var combined = relative * carried[index];

            samples[f] = new Vector4(combined.Translation, carriedTurn[index] + relativeTurn);
        }

        var forward = document.ObjectsOf<IHavokReferenceFrame>().FirstOrDefault()?.Forward ?? new Vector4(0, 0, 1, 0);

        return new HavokReferenceFrameContent
        {
            Up = new Vector4(up, 0),
            Forward = forward,
            Duration = duration,
            Samples = samples
        };
    }

    /// <summary>
    /// How far a step's root has moved and turned from where the step starts, at a time of its clip, as the Combo
    /// Builder's root offset has it (the start's inverse, then the time's). Unscaled.
    /// </summary>
    private static (Matrix4x4 Relative, float Turn) Relative(TimeActComboBakeStep step, float time)
    {
        if (step.Clip.RootMotion is not { } motion)
            return (Matrix4x4.Identity, 0);

        var start = motion.SampleRaw(step.Start);
        var at = motion.SampleRaw(time);

        var startMatrix = ToMatrix(start, motion.Up);
        var atMatrix = ToMatrix(at, motion.Up);

        var relative = Matrix4x4.Invert(startMatrix, out var inverseStart) ? atMatrix * inverseStart : atMatrix;

        return (relative, at.W - start.W);
    }

    private static Matrix4x4 ToMatrix(Vector4 sample, Vector3 up)
    {
        return Matrix4x4.CreateFromAxisAngle(up, sample.W) *
               Matrix4x4.CreateTranslation(new Vector3(sample.X, sample.Y, sample.Z));
    }

    /// <summary>
    /// The events of the combo: each step's events that fall within its stretch, moved to where the step starts in
    /// the combo and cut short where it ends. The last step's events keep their ends, so those that last to the end of
    /// the animation still do. Each step's event groups come along with its events.
    /// </summary>
    public static TAE.Animation BuildAnimation(long id, string animFileName, IReadOnlyList<TimeActComboBakeStep> steps,
        bool bigEndian)
    {
        // Plays its own (baked) file
        var animation = new TAE.Animation(id, new TAE.Animation.AnimMiniHeader.Standard(), animFileName);

        var offset = 0f;

        for (var i = 0; i < steps.Count; i++)
        {
            var step = steps[i];
            var last = i == steps.Count - 1;
            var groups = new Dictionary<TAE.EventGroup, TAE.EventGroup>();

            foreach (var ev in step.Animation.Events.OrderBy(e => e.StartTime))
            {
                var end = ev.IsInfiniteEnd ? float.MaxValue : ev.EndTime;

                // Events that a step doesn't reach: before its start (a point event at its start counts), or after its cut
                if (ev.StartTime >= step.End && !(last && ev.StartTime == step.End))
                    continue;

                if (end <= step.Start && !(end == ev.StartTime && ev.StartTime >= step.Start))
                    continue;

                var newStart = offset + Math.Max(ev.StartTime, step.Start) - step.Start;

                float newEnd;

                if (last)
                {
                    // As stored, so an infinite end stays infinite
                    newEnd = ev.IsInfiniteEnd ? ev.EndTime : offset + ev.EndTime - step.Start;
                }
                else
                {
                    newEnd = offset + Math.Min(end, step.End) - step.Start;
                }

                newEnd = Math.Max(newEnd, newStart);

                var copy = new TAE.Event(newStart, newEnd, ev.Type, ev.Unk04, ev.GetParameterBytes(bigEndian), bigEndian);

                if (ev.Group != null)
                {
                    if (!groups.TryGetValue(ev.Group, out var group))
                    {
                        group = ev.Group.GetClone();
                        groups[ev.Group] = group;
                        animation.EventGroups.Add(group);
                    }

                    copy.Group = group;
                }

                animation.Events.Add(copy);
            }

            offset += step.Length;
        }

        return animation;
    }
}
