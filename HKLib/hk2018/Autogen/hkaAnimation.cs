// Automatically Generated

namespace HKLib.hk2018;

public class hkaAnimation : hkReferencedObject
{
    public hkaAnimation.AnimationType m_type;

    public float m_duration;

    public int m_numberOfTransformTracks;

    public int m_numberOfFloatTracks;

    public hkaAnimatedReferenceFrame? m_extractedMotion;

    public List<hkaAnnotationTrack> m_annotationTracks = new();


    public enum AnimationType : int
    {
        HK_UNKNOWN_ANIMATION = 0,
        HK_INTERLEAVED_ANIMATION = 1,
        HK_MIRRORED_ANIMATION = 2,
        HK_SPLINE_COMPRESSED_ANIMATION = 3,
        HK_QUANTIZED_COMPRESSED_ANIMATION = 4,
        HK_PREDICTIVE_COMPRESSED_ANIMATION = 5,
        HK_REFERENCE_POSE_ANIMATION = 6
    }

}

