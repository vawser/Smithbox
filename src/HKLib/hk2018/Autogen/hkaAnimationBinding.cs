// Automatically Generated

namespace HKLib.hk2018;

public class hkaAnimationBinding : hkReferencedObject
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
        ADDITIVE_PARENT_SPACE = 1,
        ADDITIVE_CHILD_SPACE = 2
    }

}

