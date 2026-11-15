#nullable enable
using Havok;
using Havok.Shared;
using StudioCore.Renderer;
using System;
using System.Linq;
using System.Numerics;

namespace StudioCore.Utilities;

public enum HavokClipFormat
{
    // A spline when it holds the clip within tolerance, else interleaved
    Auto,
    Spline,
    Interleaved
}

/// <summary>What <see cref="HavokClipWriter.ReplaceClip" /> wrote.</summary>
public sealed record HavokClipWriteResult(HavokClipFormat Format, int DataBytes, double WorstErrorRatio,
    int MaxFramesPerBlock);

/// <summary>
///     Puts a new clip into one binding of a Havok document (a cutscene cut's camera), leaving every other object as it
///     was read. The binding keeps its track to bone and track to float slot maps, so the samplers are by track.
/// </summary>
public static class HavokClipWriter
{
    /// <param name="skeletonName">The binding's original skeleton name (camNNNN in a cut)</param>
    /// <param name="sampleTransform">(track, frame) → local transform, for frames 0 to frameCount - 1</param>
    /// <param name="sampleFloat">(float track, frame) → value</param>
    public static HavokClipWriteResult ReplaceClip(HavokDocument document, string skeletonName, int frameCount,
        int trackCount, int floatCount, Func<int, float, HavokTransformData> sampleTransform,
        Func<int, float, float>? sampleFloat, float frameDuration, HavokClipFormat format = HavokClipFormat.Auto,
        SplineEncodeSettings? settings = null)
    {
        IHavokAnimationContainer container = document.ObjectsOf<IHavokAnimationContainer>().FirstOrDefault()
                                             ?? throw new InvalidOperationException("The file has no animation container.");

        IHavokAnimationBinding binding = container.Bindings.FirstOrDefault(b =>
                                             string.Equals(b.OriginalSkeletonName, skeletonName, StringComparison.Ordinal))
                                         ?? throw new InvalidOperationException($"No binding is for skeleton {skeletonName}.");

        if (binding.TransformTrackToBoneIndices.Count > 0 && binding.TransformTrackToBoneIndices.Count != trackCount)
        {
            throw new ArgumentException(
                $"The binding maps {binding.TransformTrackToBoneIndices.Count} transform tracks, not {trackCount}.");
        }

        if (binding.FloatTrackToFloatSlotIndices.Count > 0 && binding.FloatTrackToFloatSlotIndices.Count != floatCount)
        {
            throw new ArgumentException(
                $"The binding maps {binding.FloatTrackToFloatSlotIndices.Count} float tracks, not {floatCount}.");
        }

        if (format != HavokClipFormat.Interleaved)
        {
            SplineEncodedClip spline = SplineCompressedEncoder.Encode(frameCount, trackCount, floatCount,
                sampleTransform, sampleFloat, frameDuration, settings);

            if (format == HavokClipFormat.Spline || spline.WorstErrorRatio <= 1)
            {
                container.ReplaceAnimation(binding, new HavokSplineContent
                {
                    Duration = spline.Duration,
                    TransformTrackCount = trackCount,
                    FloatTrackCount = floatCount,
                    FrameCount = spline.FrameCount,
                    BlockCount = spline.BlockCount,
                    MaxFramesPerBlock = spline.MaxFramesPerBlock,
                    MaskAndQuantizationSize = spline.MaskAndQuantizationSize,
                    BlockDuration = spline.BlockDuration,
                    BlockInverseDuration = spline.BlockInverseDuration,
                    FrameDuration = spline.FrameDuration,
                    Data = spline.Data,
                    BlockOffsets = spline.BlockOffsets,
                    FloatBlockOffsets = spline.FloatBlockOffsets
                });

                return new HavokClipWriteResult(HavokClipFormat.Spline, spline.Data.Length, spline.WorstErrorRatio,
                    spline.MaxFramesPerBlock);
            }
        }

        // Every frame as it is: exact, at 48 bytes a transform
        var transforms = new HavokQsTransform[frameCount * trackCount];
        var floats = new float[frameCount * floatCount];
        // Neighbouring frames are blended, so each track stays in one hemisphere
        var previous = new Quaternion?[trackCount];
        for (var f = 0; f < frameCount; f++)
        {
            for (var t = 0; t < trackCount; t++)
            {
                HavokTransformData x = sampleTransform(t, f);
                Quaternion q = Quaternion.Normalize(x.Rotation);
                if (previous[t] is { } p && Quaternion.Dot(p, q) < 0)
                {
                    q = -q;
                }

                previous[t] = q;
                transforms[f * trackCount + t] = new HavokQsTransform(x.Translation, q, x.Scale);
            }

            for (var t = 0; t < floatCount; t++)
            {
                floats[f * floatCount + t] = sampleFloat!(t, f);
            }
        }

        container.ReplaceAnimation(binding, new HavokInterleavedContent
        {
            Duration = (frameCount - 1) * frameDuration,
            TransformTrackCount = trackCount,
            FloatTrackCount = floatCount,
            Transforms = transforms,
            Floats = floats
        });

        return new HavokClipWriteResult(HavokClipFormat.Interleaved, (transforms.Length * 48) + (floats.Length * 4), 0,
            0);
    }
}
