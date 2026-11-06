// Automatically Generated

namespace Havok.hk2014;

public class hkbpReachTowardTargetModifier : hkbModifier
{
    public hkbpReachTowardTargetModifierHand m_leftHand = new();

    public hkbpReachTowardTargetModifierHand m_rightHand = new();

    public hkbpTarget? m_targetIn;

    public float m_distanceBetweenHands;

    public float m_reachDistance;

    public float m_fadeInGainSpeed;

    public float m_fadeOutGainSpeed;

    public float m_fadeOutDuration;

    public float m_targetChangeSpeed;

    public bool m_holdTarget;

    public bool m_reachPastTarget;

    public bool m_giveUpIfNoTarget;

}
