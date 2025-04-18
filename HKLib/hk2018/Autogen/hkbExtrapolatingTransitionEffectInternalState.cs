// Automatically Generated

namespace HKLib.hk2018;

public class hkbExtrapolatingTransitionEffectInternalState : hkReferencedObject
{
    public hkbGeneratorSyncInfo m_fromGeneratorSyncInfo = new();

    public hkbGeneratorPartitionInfo m_fromGeneratorPartitionInfo = new();

    public hkQsTransform m_worldFromModel = new();

    public hkQsTransform m_motion = new();

    public List<hkQsTransform> m_pose = new();

    public List<hkQsTransform> m_additivePose = new();

    public List<float> m_boneWeights = new();

    public float m_toGeneratorDuration;

    public bool m_isFromGeneratorActive;

    public bool m_gotPose;

    public bool m_gotAdditivePose;

}

