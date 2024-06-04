// Automatically Generated

namespace HKLib.hk2018;

public class hkbPoseMatchingGenerator : hkbBlenderGenerator, hkbVerifiable
{
    public Quaternion m_worldFromModelRotation = new();

    public float m_blendSpeed;

    public float m_minSpeedToSwitch;

    public float m_minSwitchTimeNoError;

    public float m_minSwitchTimeFullError;

    public int m_startPlayingEventId;

    public int m_startMatchingEventId;

    public short m_rootBoneIndex;

    public short m_otherBoneIndex;

    public short m_anotherBoneIndex;

    public short m_pelvisIndex;

    public hkbPoseMatchingGenerator.Mode m_mode;


    public enum Mode : int
    {
        MODE_MATCH = 0,
        MODE_PLAY = 1
    }

}

