// Automatically Generated

namespace HKLib.hk2018;

public class hkbBlendingTransitionEffectInternalState : hkReferencedObject
{
    public Vector4 m_fromPos = new();

    public Quaternion m_fromRot = new();

    public Vector4 m_toPos = new();

    public Quaternion m_toRot = new();

    public Vector4 m_lastPos = new();

    public Quaternion m_lastRot = new();

    public List<hkQsTransform> m_characterPoseAtBeginningOfTransition = new();

    public float m_timeRemaining;

    public float m_timeInTransition;

    public hkbTransitionEffect.SelfTransitionMode m_toGeneratorSelfTranstitionMode;

    public bool m_initializeCharacterPose;

    public bool m_alignThisFrame;

    public bool m_alignmentFinished;

}

