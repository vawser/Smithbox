// Automatically Generated

namespace Havok.hk2010;

public class hkaAnimationBinding : hkReferencedObject
{
    public string? m_originalSkeletonName;

    public hkaAnimation? m_animation;

    public List<short> m_transformTrackToBoneIndices = new();

    public List<short> m_floatTrackToFloatSlotIndices = new();

    public hkaAnimationBinding.BlendHint m_blendHint;


    public enum BlendHint : int
    {
        NORMAL = 0,
        ADDITIVE = 1
    }

}
