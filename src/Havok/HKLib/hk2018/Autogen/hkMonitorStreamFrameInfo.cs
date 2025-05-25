// Automatically Generated

namespace HKLib.hk2018;

public class hkMonitorStreamFrameInfo : IHavokObject
{
    public string? m_heading;

    public int m_indexOfTimer0;

    public int m_indexOfTimer1;

    public hkMonitorStreamFrameInfo.AbsoluteTimeCounter m_absoluteTimeCounter;

    public float m_timerFactor0;

    public float m_timerFactor1;

    public int m_threadId;

    public int m_frameStreamStart;

    public int m_frameStreamEnd;


    public enum AbsoluteTimeCounter : int
    {
        ABSOLUTE_TIME_TIMER_0 = 0,
        ABSOLUTE_TIME_TIMER_1 = 1,
        ABSOLUTE_TIME_NOT_TIMED = -1
    }

}

