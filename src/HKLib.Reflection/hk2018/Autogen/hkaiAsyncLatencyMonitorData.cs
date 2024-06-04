// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiAsyncLatencyMonitorData : HavokData<hkaiAsyncLatencyMonitor> 
{
    public hkaiAsyncLatencyMonitorData(HavokType type, hkaiAsyncLatencyMonitor instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_pendingLatency":
            case "pendingLatency":
            {
                if (instance.m_pendingLatency is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_processingLatency":
            case "processingLatency":
            {
                if (instance.m_processingLatency is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_pendingLatency":
            case "pendingLatency":
            {
                if (value is not float castValue) return false;
                instance.m_pendingLatency = castValue;
                return true;
            }
            case "m_processingLatency":
            case "processingLatency":
            {
                if (value is not float castValue) return false;
                instance.m_processingLatency = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
