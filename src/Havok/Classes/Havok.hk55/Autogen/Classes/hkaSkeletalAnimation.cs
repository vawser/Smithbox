// Automatically Generated

namespace Havok.hk55;

public class hkaSkeletalAnimation : hkReferencedObject
{
    public hkaSkeletalAnimation.AnimationType m_type;

    public float m_duration;

    public int m_numberOfTransformTracks;

    public int m_numberOfFloatTracks;

    public hkaAnimatedReferenceFrame? m_extractedMotion;

    public List<hkaAnnotationTrack?> m_annotationTracks = new();

    public int m_numberOfTracks;


    public enum AnimationType : int
    {
        HK_UNKNOWN_ANIMATION = 0,
        HK_INTERLEAVED_ANIMATION = 1,
        HK_DELTA_COMPRESSED_ANIMATION = 2,
        HK_WAVELET_COMPRESSED_ANIMATION = 3,
        HK_MIRRORED_ANIMATION = 4,
        HK_SPLINE_COMPRESSED_ANIMATION = 5
    }

}
