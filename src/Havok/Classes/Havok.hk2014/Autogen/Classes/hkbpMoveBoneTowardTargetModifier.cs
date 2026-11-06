// Automatically Generated

namespace Havok.hk2014;

public class hkbpMoveBoneTowardTargetModifier : hkbModifier
{
    public hkbpTarget? m_targetIn;

    public Vector4 m_offsetInBoneSpace = new();

    public Vector4 m_alignAxisBS = new();

    public Vector4 m_targetAlignAxisTS = new();

    public Vector4 m_alignWithCharacterForwardBS = new();

    public Vector4 m_currentBonePositionOut = new();

    public Quaternion m_currentBoneRotationOut = new();

    public hkbEventProperty m_eventToSendWhenTargetReached = new();

    public hkbGenerator? m_childGenerator;

    public float m_duration;

    public short m_ragdollBoneIndex;

    public short m_animationBoneIndex;

    public hkbpMoveBoneTowardTargetModifier.TargetModeMBTT m_targetMode;

    public sbyte m_alignMode;

    public bool m_useVelocityPrediction;

    public bool m_affectOrientation;

    public bool m_currentBoneIsValidOut;

    public Vector4 m_finalAnimBonePositionMS = new();

    public Vector4 m_initialAnimBonePositionMS = new();

    public Quaternion m_finalAnimBoneOrientationMS = new();

    public Quaternion m_animationFromRagdoll = new();

    public hkQsTransform m_totalMotion = new();

    public hkQsTransform m_accumulatedMotion = new();

    public bool m_useAnimationData;


    public enum TargetModeMBTT : int
    {
        TARGET_POSITION = 0,
        TARGET_COM = 1,
        TARGET_CONTACT_POINT = 2,
        TARGET_SHAPE_CENTROID = 3
    }

}
