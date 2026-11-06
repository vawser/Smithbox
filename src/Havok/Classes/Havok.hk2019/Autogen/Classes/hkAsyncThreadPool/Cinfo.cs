// Automatically Generated

namespace Havok.hk2019.hkAsyncThreadPool;

public class Cinfo : IHavokObject
{
    public hkAsyncThreadPool.HardwareThreadBinding m_hardwareThreadBinding;

    public int m_numThreads;

    public List<uint> m_hardwareThreadMasksOrIds = new();

    public ulong m_stackSize;

    public string? m_threadName;

    public List<hkAsyncThreadPool.ThreadPriority> m_threadBackgroundPriorities = new();

    public int m_timerBufferPerThreadAllocation;

}
