// Automatically Generated

namespace Havok.hk2014;

public class CustomClipGenerator : hkbGenerator
{
    public float m_playbackSpeed;

    public CustomClipGenerator.PlaybackMode m_mode;

    public int m_animId;

    public CustomClipGenerator.AnimeEndEventType m_animeEndEventType;

    public hkbEvent m_endEvent = new();

    public float m_cropStartAmountLocalTime;

    public float m_cropEndAmountLocalTime;

    public float m_startTime;

    public float m_enforcedDuration;

    public sbyte m_flags;


    public enum PlaybackMode : int
    {
        MODE_SINGLE_PLAY = 0,
        MODE_LOOPING = 1
    }

    public enum AnimeEndEventType : int
    {
        FireNextStateEvent = 0,
        FireStateEndEvent = 1,
        FireIdleEvent = 2
    }

}
