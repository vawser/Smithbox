// Automatically Generated

namespace HKLib.hk2018;

public class hkGpuTraceResult : IHavokObject
{
    public ulong m_id;

    public double m_gpuTimeBegin;

    public double m_gpuTimeEnd;

    public uint m_numPixelsTouched;

    public hkGpuTraceResult.ScopeType m_type;

    public ushort m_threadId;

    public hkReferencedObject? m_meta;


    public enum ScopeType : int
    {
        SCOPE_PROBE = 0,
        SCOPE_CALL = 1
    }

}

