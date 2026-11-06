// Automatically Generated

namespace Havok.hk55;

public class hkaAnimationBinding : IHavokObject
{
    public hkaSkeletalAnimation? m_animation;

    public List<short> m_transformTrackToBoneIndices = new();

    public List<short> m_floatTrackToFloatSlotIndices = new();

    public hkaAnimationBinding.BlendHint m_blendHint;

    public List<short> m_animationTrackToBoneIndices = new();


    public enum BlendHint : int
    {
        NORMAL = 0,
        ADDITIVE = 1
    }

}
