// Automatically Generated

namespace Havok.hk2014;

public class hkbpCheckBalanceModifier : hkbModifier
{
    public short m_ragdollLeftFootBoneIndex;

    public short m_ragdollRightFootBoneIndex;

    public float m_balanceOnAnklesFraction;

    public hkbEvent m_eventToSendWhenOffBalance = new();

    public float m_offBalanceEventThreshold;

    public int m_worldUpAxisIndex;

    public float m_comBiasX;

    public bool m_extractRagdollPose;

}
