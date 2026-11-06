// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkGpuTraceResultData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkGpuTraceResult>
{
    public hkGpuTraceResultData(Havok.hk2019.hkGpuTraceResult instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_id":
            case "id":
            {
                value = instance.m_id;
                return true;
            }
            case "m_parentScopeId":
            case "parentScopeId":
            {
                value = instance.m_parentScopeId;
                return true;
            }
            case "m_gpuTimeBegin":
            case "gpuTimeBegin":
            {
                value = instance.m_gpuTimeBegin;
                return true;
            }
            case "m_gpuTimeEnd":
            case "gpuTimeEnd":
            {
                value = instance.m_gpuTimeEnd;
                return true;
            }
            case "m_numPixelsTouched":
            case "numPixelsTouched":
            {
                value = instance.m_numPixelsTouched;
                return true;
            }
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_threadId":
            case "threadId":
            {
                value = instance.m_threadId;
                return true;
            }
            case "m_meta":
            case "meta":
            {
                value = instance.m_meta;
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
            case "m_id":
            case "id":
            {
                if (value is not ulong castValue) return false;
                instance.m_id = castValue;
                return true;
            }
            case "m_parentScopeId":
            case "parentScopeId":
            {
                if (value is not ulong castValue) return false;
                instance.m_parentScopeId = castValue;
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
                if (value is Havok.hk2019.hkGpuTraceResult.ScopeType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (Havok.hk2019.hkGpuTraceResult.ScopeType)ushortValue;
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
                if (value is Havok.hk2019.hkReferencedObject castValue)
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
