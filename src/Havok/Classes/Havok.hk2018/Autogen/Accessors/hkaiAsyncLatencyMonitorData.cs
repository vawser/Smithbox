// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiAsyncLatencyMonitorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiAsyncLatencyMonitor>
{
    public hkaiAsyncLatencyMonitorData(Havok.hk2018.hkaiAsyncLatencyMonitor instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_pendingLatency":
            case "pendingLatency":
            {
                value = instance.m_pendingLatency;
                return true;
            }
            case "m_processingLatency":
            case "processingLatency":
            {
                value = instance.m_processingLatency;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
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
