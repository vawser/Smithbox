// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkGpuTraceResultData : HavokData<hkGpuTraceResult> 
{
    public hkGpuTraceResultData(HavokType type, hkGpuTraceResult instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_id":
            case "id":
            {
                if (instance.m_id is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_gpuTimeBegin":
            case "gpuTimeBegin":
            {
                if (instance.m_gpuTimeBegin is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_gpuTimeEnd":
            case "gpuTimeEnd":
            {
                if (instance.m_gpuTimeEnd is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numPixelsTouched":
            case "numPixelsTouched":
            {
                if (instance.m_numPixelsTouched is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((ushort)instance.m_type is TGet ushortValue)
                {
                    value = ushortValue;
                    return true;
                }
                return false;
            }
            case "m_threadId":
            case "threadId":
            {
                if (instance.m_threadId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_meta":
            case "meta":
            {
                if (instance.m_meta is null)
                {
                    return true;
                }
                if (instance.m_meta is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_id":
            case "id":
            {
                if (value is not ulong castValue) return false;
                instance.m_id = castValue;
                return true;
            }
            case "m_gpuTimeBegin":
            case "gpuTimeBegin":
            {
                if (value is not double castValue) return false;
                instance.m_gpuTimeBegin = castValue;
                return true;
            }
            case "m_gpuTimeEnd":
            case "gpuTimeEnd":
            {
                if (value is not double castValue) return false;
                instance.m_gpuTimeEnd = castValue;
                return true;
            }
            case "m_numPixelsTouched":
            case "numPixelsTouched":
            {
                if (value is not uint castValue) return false;
                instance.m_numPixelsTouched = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (value is hkGpuTraceResult.ScopeType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (hkGpuTraceResult.ScopeType)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_threadId":
            case "threadId":
            {
                if (value is not ushort castValue) return false;
                instance.m_threadId = castValue;
                return true;
            }
            case "m_meta":
            case "meta":
            {
                if (value is null)
                {
                    instance.m_meta = default;
                    return true;
                }
                if (value is hkReferencedObject castValue)
                {
                    instance.m_meta = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
