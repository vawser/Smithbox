// Automatically Generated

namespace Havok.hk2014;

public partial class hkaAnimationBinding : hkReferencedObject
{
    public string? m_originalSkeletonName;

    public hkaAnimation? m_animation;

    public List<short> m_transformTrackToBoneIndices = new();

    public List<short> m_floatTrackToFloatSlotIndices = new();

    public List<short> m_partitionIndices = new();

    public hkaAnimationBinding.BlendHint m_blendHint;


    public enum BlendHint : int
    {
        NORMAL = 0,
        ADDITIVE_DEPRECATED = 1,
        ADDITIVE = 2
    }

}
