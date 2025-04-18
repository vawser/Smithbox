// Automatically Generated

namespace HKLib.hk2018.hkAsyncThreadPool;

public class Cinfo : IHavokObject
{
    public hkAsyncThreadPool.HardwareThreadBinding m_hardwareThreadBinding;

    public int m_numThreads;

    public List<uint> m_hardwareThreadMasksOrIds = new();

    public int m_stackSize;

    public string? m_threadName;

    public int m_normalPriority;

    public int m_lowPriority;

    public List<hkAsyncThreadPool.ThreadPriority> m_threadBackgroundPriorities = new();

    public int m_timerBufferPerThreadAllocation;

}

