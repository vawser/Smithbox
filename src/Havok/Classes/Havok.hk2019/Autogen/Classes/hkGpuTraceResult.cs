// Automatically Generated

namespace Havok.hk2019;

public class hkGpuTraceResult : IHavokObject
{
    public ulong m_id;

    public ulong m_parentScopeId;

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
