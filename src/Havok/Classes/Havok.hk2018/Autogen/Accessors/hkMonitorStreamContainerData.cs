// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkMonitorStreamContainerData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkMonitorStreamContainer>
{
    public hkMonitorStreamContainerData(Havok.hk2018.hkMonitorStreamContainer instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_info":
            case "info":
            {
                value = instance.m_info;
                return true;
            }
            case "m_pointersAre64Bit":
            case "pointersAre64Bit":
            {
                value = instance.m_pointersAre64Bit;
                return true;
            }
            case "m_littleEndian":
            case "littleEndian":
            {
                value = instance.m_littleEndian;
                return true;
            }
            case "m_timersAre64Bit":
            case "timersAre64Bit":
            {
                value = instance.m_timersAre64Bit;
                return true;
            }
            case "m_timerFactor":
            case "timerFactor":
            {
                value = instance.m_timerFactor;
                return true;
            }
            case "m_stringMap":
            case "stringMap":
            {
                value = instance.m_stringMap;
                return true;
            }
            case "m_typeMap":
            case "typeMap":
            {
                value = instance.m_typeMap;
                return true;
            }
            case "m_trace":
            case "trace":
            {
                value = instance.m_trace;
                return true;
            }
            case "m_threadStartOffsets":
            case "threadStartOffsets":
            {
                value = instance.m_threadStartOffsets;
                return true;
            }
            case "m_frameStartOffsets":
            case "frameStartOffsets":
            {
                value = instance.m_frameStartOffsets;
                return true;
            }
            case "m_gpuTrace":
            case "gpuTrace":
            {
                value = instance.m_gpuTrace;
                return true;
            }
            case "m_gpuFrameStartOffsets":
            case "gpuFrameStartOffsets":
            {
                value = instance.m_gpuFrameStartOffsets;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_info":
            case "info":
            {
                if (value is null)
                {
                    instance.m_info = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_info = castValue;
                    return true;
                }
                return false;
            }
            case "m_pointersAre64Bit":
            case "pointersAre64Bit":
            {
                if (value is not bool castValue) return false;
                instance.m_pointersAre64Bit = castValue;
                return true;
            }
            case "m_littleEndian":
            case "littleEndian":
            {
                if (value is not bool castValue) return false;
                instance.m_littleEndian = castValue;
                return true;
            }
            case "m_timersAre64Bit":
            case "timersAre64Bit":
            {
                if (value is not bool castValue) return false;
                instance.m_timersAre64Bit = castValue;
                return true;
            }
            case "m_timerFactor":
            case "timerFactor":
            {
                if (value is not float castValue) return false;
                instance.m_timerFactor = castValue;
                return true;
            }
            case "m_stringMap":
            case "stringMap":
            {
                if (value is not Havok.hk2018.hkMonitorStreamStringMap castValue) return false;
                instance.m_stringMap = castValue;
                return true;
            }
            case "m_typeMap":
            case "typeMap":
            {
                if (value is not Havok.hk2018.hkMonitorStreamTypeMap castValue) return false;
                instance.m_typeMap = castValue;
                return true;
            }
            case "m_trace":
            case "trace":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_trace = castValue;
                return true;
            }
            case "m_threadStartOffsets":
            case "threadStartOffsets":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_threadStartOffsets = castValue;
                return true;
            }
            case "m_frameStartOffsets":
            case "frameStartOffsets":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_frameStartOffsets = castValue;
                return true;
            }
            case "m_gpuTrace":
            case "gpuTrace":
            {
                if (value is not List<Havok.hk2018.hkGpuTraceResult> castValue) return false;
                instance.m_gpuTrace = castValue;
                return true;
            }
            case "m_gpuFrameStartOffsets":
            case "gpuFrameStartOffsets":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_gpuFrameStartOffsets = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
