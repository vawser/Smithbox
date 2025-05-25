// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkMonitorStreamContainerData : HavokData<hkMonitorStreamContainer> 
{
    public hkMonitorStreamContainerData(HavokType type, hkMonitorStreamContainer instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_info":
            case "info":
            {
                if (instance.m_info is null)
                {
                    return true;
                }
                if (instance.m_info is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_pointersAre64Bit":
            case "pointersAre64Bit":
            {
                if (instance.m_pointersAre64Bit is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_littleEndian":
            case "littleEndian":
            {
                if (instance.m_littleEndian is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_timersAre64Bit":
            case "timersAre64Bit":
            {
                if (instance.m_timersAre64Bit is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_timerFactor":
            case "timerFactor":
            {
                if (instance.m_timerFactor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stringMap":
            case "stringMap":
            {
                if (instance.m_stringMap is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_typeMap":
            case "typeMap":
            {
                if (instance.m_typeMap is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_trace":
            case "trace":
            {
                if (instance.m_trace is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_threadStartOffsets":
            case "threadStartOffsets":
            {
                if (instance.m_threadStartOffsets is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_frameStartOffsets":
            case "frameStartOffsets":
            {
                if (instance.m_frameStartOffsets is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_gpuTrace":
            case "gpuTrace":
            {
                if (instance.m_gpuTrace is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_gpuFrameStartOffsets":
            case "gpuFrameStartOffsets":
            {
                if (instance.m_gpuFrameStartOffsets is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
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
                if (value is not hkMonitorStreamStringMap castValue) return false;
                instance.m_stringMap = castValue;
                return true;
            }
            case "m_typeMap":
            case "typeMap":
            {
                if (value is not hkMonitorStreamTypeMap castValue) return false;
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
                if (value is not List<hkGpuTraceResult> castValue) return false;
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
