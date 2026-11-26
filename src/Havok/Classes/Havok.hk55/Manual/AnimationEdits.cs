using Havok.Shared;

namespace Havok.hk55;

// IHavokAnimationContainer (Havok.Shared/Animation.cs), for writing an edited clip back (the Animation Editor on DeS).
// Hand-written; hkaAnimationContainer was made partial by hand in Autogen: add it to the generator's partial list on the
// next regeneration. Havok 5.x has no spline class, and the vanilla wavelet / delta clips have no encoder, so an edited
// clip is written as hkaInterleavedSkeletalAnimation: every frame of every track, exact, but larger than the original.

public partial class hkaAnimationContainer : IHavokAnimationContainer
{
    IReadOnlyList<IHavokAnimationBinding> IHavokAnimationContainer.Bindings =>
        m_bindings.Where(b => b != null).Cast<IHavokAnimationBinding>().ToList();

    bool IHavokAnimationContainer.WritesSpline => false;

    void IHavokAnimationContainer.ReplaceAnimation(IHavokAnimationBinding binding, HavokAnimationContent content)
    {
        if (binding is not hkaAnimationBinding target || !m_bindings.Contains(target))
            throw new ArgumentException("The binding isn't one of this container's.", nameof(binding));

        if (content is not HavokInterleavedContent interleaved)
            throw new NotSupportedException($"{content.GetType().Name} can't be written in a DeS file (interleaved clips only).");

        hkaSkeletalAnimation? old = target.m_animation;
        var created = new hkaInterleavedSkeletalAnimation
        {
            m_type = hkaSkeletalAnimation.AnimationType.HK_INTERLEAVED_ANIMATION,
            m_transforms = interleaved.Transforms.Select(t => new hkQsTransform
            {
                // As the hk2014+ writers store them: translation W 1, scale W 0
                m_translation = new System.Numerics.Vector4(t.Translation, 1),
                m_rotation = t.Rotation,
                m_scale = new System.Numerics.Vector4(t.Scale, 0)
            }).ToList(),
            m_floats = interleaved.Floats.ToList(),
            m_duration = content.Duration,
            m_numberOfTransformTracks = content.TransformTrackCount,
            m_numberOfFloatTracks = content.FloatTrackCount,
            // Havok 5.1 files (the DES_51 overlay) give the track count here instead; 5.5 doesn't serialize it
            m_numberOfTracks = content.TransformTrackCount
        };

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
