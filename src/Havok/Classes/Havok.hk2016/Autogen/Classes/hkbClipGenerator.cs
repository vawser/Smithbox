// Automatically Generated

using Havok.Shared;

namespace Havok.hk2016;

public class hkbClipGenerator : hkbGenerator, IClipGenerator
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


    public enum PlaybackMode : int
    {
        MODE_SINGLE_PLAY = 0,
        MODE_LOOPING = 1,
        MODE_USER_CONTROLLED = 2,
        MODE_PING_PONG = 3,
        MODE_COUNT = 4
    }

}
