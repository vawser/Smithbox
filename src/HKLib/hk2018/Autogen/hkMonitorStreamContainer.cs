// Automatically Generated

namespace HKLib.hk2018;

public class hkMonitorStreamContainer : hkReferencedObject
{
    public string? m_info;

    public bool m_pointersAre64Bit;

    public bool m_littleEndian;

    public bool m_timersAre64Bit;

    public float m_timerFactor;

    public hkMonitorStreamStringMap m_stringMap = new();

    public hkMonitorStreamTypeMap m_typeMap = new();

    public List<byte> m_trace = new();

    public List<uint> m_threadStartOffsets = new();

    public List<uint> m_frameStartOffsets = new();

    public List<hkGpuTraceResult> m_gpuTrace = new();

    public List<uint> m_gpuFrameStartOffsets = new();

}

