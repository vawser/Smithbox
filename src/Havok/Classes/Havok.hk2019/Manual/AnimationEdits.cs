using Havok.Shared;

namespace Havok.hk2019;

// IHavokAnimationContainer (Havok.Shared/Animation.cs), for writing an edited clip back (the Cutscene Editor's camera).
// Hand-written; hkaAnimationContainer was made partial by hand in Autogen: add it to the generator's partial list on the
// next regeneration. The same in every SDK except for the property bag, which only some hkReferencedObjects have.

public partial class hkaAnimationContainer : IHavokAnimationContainer
{
    IReadOnlyList<IHavokAnimationBinding> IHavokAnimationContainer.Bindings =>
        m_bindings.Where(b => b != null).Cast<IHavokAnimationBinding>().ToList();

    void IHavokAnimationContainer.ReplaceAnimation(IHavokAnimationBinding binding, HavokAnimationContent content)
    {
        if (binding is not hkaAnimationBinding target || !m_bindings.Contains(target))
            throw new ArgumentException("The binding isn't one of this container's.", nameof(binding));

        hkaAnimation? old = target.m_animation;
        hkaAnimation created = content switch
        {
            HavokSplineContent spline => new hkaSplineCompressedAnimation
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
            },
            HavokInterleavedContent interleaved => new hkaInterleavedUncompressedAnimation
            {
                m_type = hkaAnimation.AnimationType.HK_INTERLEAVED_ANIMATION,
                m_transforms = interleaved.Transforms.Select(t => new hkQsTransform
                {
                    // As vanilla clips store them: translation W 1, scale W 0
                    m_translation = new System.Numerics.Vector4(t.Translation, 1),
                    m_rotation = t.Rotation,
                    m_scale = new System.Numerics.Vector4(t.Scale, 0)
                }).ToList(),
                m_floats = interleaved.Floats.ToList()
            },
            _ => throw new NotSupportedException($"{content.GetType().Name} can't be written.")
        };

        created.m_duration = content.Duration;
        created.m_numberOfTransformTracks = content.TransformTrackCount;
        created.m_numberOfFloatTracks = content.FloatTrackCount;
        if (old != null)
        {
            created.m_extractedMotion = old.m_extractedMotion;
            created.m_annotationTracks = old.m_annotationTracks;
        }

        // A new root motion (a baked combo), in the old clip's object when it had one, so the rest of it stays as read
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
