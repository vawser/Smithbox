// Automatically Generated

namespace Havok.hk2014;

public class hkbpCatchFallModifier : hkbModifier
{
    public Vector4 m_directionOfFallForwardLS = new();

    public Vector4 m_directionOfFallRightLS = new();

    public Vector4 m_directionOfFallUpLS = new();

    public hkbBoneIndexArray? m_spineIndices;

    public hkbpCatchFallModifierHand m_leftHand = new();

    public hkbpCatchFallModifierHand m_rightHand = new();

    public hkbEventProperty m_catchFallDoneEvent = new();

    public float m_spreadHandsMultiplier;

    public float m_radarRange;

    public float m_previousTargetBlendWeight;

    public float m_handsBendDistance;

    public float m_maxReachDistanceForward;

    public float m_maxReachDistanceBackward;

    public float m_fadeInReachGainSpeed;

    public float m_fadeOutReachGainSpeed;

    public float m_fadeOutDuration;

    public float m_fadeInTwistSpeed;

    public float m_fadeOutTwistSpeed;

    public short m_raycastLayer;

    public short m_velocityRagdollBoneIndex;

    public short m_directionOfFallRagdollBoneIndex;

    public bool m_orientHands;

    public readonly Vector4[] m_catchFallPosInBS = new Vector4[2];

    public readonly float[] m_currentReachGain = new float[2];

    public float m_timeSinceLastModify;

    public float m_currentTwistGain;

    public short m_currentTwistDirection;

    public readonly bool[] m_catchFallPosIsValid = new bool[2];

}
