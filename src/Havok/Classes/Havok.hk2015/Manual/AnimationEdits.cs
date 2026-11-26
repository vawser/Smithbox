using Havok.Shared;

namespace Havok.hk2015;

// IHavokAnimationContainer (Havok.Shared/Animation.cs), for writing an edited clip back (the Animation Editor on DS1R).
// Hand-written; hkaAnimationContainer was made partial by hand in Autogen: add it to the generator's partial list on the
// next regeneration. As hk2014's, but spline only: the 2015 registry is built from DS1R's files, which hold no
// hkaInterleavedUncompressedAnimation, so that type can't be written.

public partial class hkaAnimationContainer : IHavokAnimationContainer
{
    IReadOnlyList<IHavokAnimationBinding> IHavokAnimationContainer.Bindings =>
        m_bindings.Where(b => b != null).Cast<IHavokAnimationBinding>().ToList();

    bool IHavokAnimationContainer.WritesInterleaved => false;

    void IHavokAnimationContainer.ReplaceAnimation(IHavokAnimationBinding binding, HavokAnimationContent content)
    {
        if (binding is not hkaAnimationBinding target || !m_bindings.Contains(target))
            throw new ArgumentException("The binding isn't one of this container's.", nameof(binding));

        if (content is not HavokSplineContent spline)
            throw new NotSupportedException($"{content.GetType().Name} can't be written in a DS1R file (spline clips only).");

        hkaAnimation? old = target.m_animation;
        hkaAnimation created = new hkaSplineCompressedAnimation
        {
            m_type = hkaAnimation.AnimationType.HK_SPLINE_COMPRESSED_ANIMATION,
            m_numFrames = spline.FrameCount,
            m_numBlocks = spline.BlockCount,
            m_maxFramesPerBlock = spline.MaxFramesPerBlock,
            m_maskAndQuantizationSize = spline.MaskAndQuantizationSize,
            m_blockDuration = spline.BlockDuration,
            m_blockInverseDuration = spline.BlockInverseDuration,
            m_frameDuration = spline.FrameDuration,
            m_blockOffsets = spline.BlockOffsets.ToList(),
            m_floatBlockOffsets = spline.FloatBlockOffsets.ToList(),
            m_data = spline.Data.ToList(),
            m_endian = (old as hkaSplineCompressedAnimation)?.m_endian ?? 0
        };

        created.m_duration = content.Duration;
        created.m_numberOfTransformTracks = content.TransformTrackCount;
        created.m_numberOfFloatTracks = content.FloatTrackCount;
        if (old != null)
        {
            created.m_extractedMotion = old.m_extractedMotion;
            created.m_annotationTracks = old.m_annotationTracks;
        }

        // A new root motion, in the old clip's object when it had one, so the rest of it stays as read
        if (content.ReferenceFrame is { } frame)
        {
            var motion = old?.m_extractedMotion as hkaDefaultAnimatedReferenceFrame ?? new hkaDefaultAnimatedReferenceFrame();
            motion.m_up = frame.Up;
            motion.m_forward = frame.Forward;
            motion.m_duration = frame.Duration;
            motion.m_referenceFrameSamples = frame.Samples.ToList();
            created.m_extractedMotion = motion;
        }

        target.m_animation = created;
        var index = old != null ? m_animations.IndexOf(old) : -1;
        if (index >= 0) m_animations[index] = created;
        else m_animations.Add(created);
    }
}
