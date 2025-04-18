// Automatically Generated

namespace HKLib.hk2018;

public class hkbClipGenerator : hkbGenerator, hkbVerifiable
{
    public string? m_animationName;

    public hkbClipTriggerArray? m_triggers;

    public uint m_userPartitionMask;

    public float m_cropStartAmountLocalTime;

    public float m_cropEndAmountLocalTime;

    public float m_startTime;

    public float m_playbackSpeed;

    public float m_enforcedDuration;

    public float m_userControlledTimeFraction;

    public hkbClipGenerator.PlaybackMode m_mode;

    public sbyte m_flags;

    public short m_animationInternalId;


    public enum ClipFlags : int
    {
        FLAG_CONTINUE_MOTION_AT_END = 1,
        FLAG_SYNC_HALF_CYCLE_IN_PING_PONG_MODE = 2,
        FLAG_MIRROR = 4,
        FLAG_FORCE_DENSE_POSE = 8,
        FLAG_DONT_CONVERT_ANNOTATIONS_TO_TRIGGERS = 16,
        FLAG_IGNORE_MOTION = 32
    }

    public enum PlaybackMode : int
    {
        MODE_SINGLE_PLAY = 0,
        MODE_LOOPING = 1,
        MODE_USER_CONTROLLED = 2,
        MODE_PING_PONG = 3,
        MODE_COUNT = 4
    }

    public class Echo : IHavokObject
    {
        public float m_offsetLocalTime;

        public float m_weight;

        public float m_dwdt;

    }


}

