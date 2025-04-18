// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkMemoryResourceHandleExternalLinkData : HavokData<hkMemoryResourceHandle.ExternalLink> 
{
    public hkMemoryResourceHandleExternalLinkData(HavokType type, hkMemoryResourceHandle.ExternalLink instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_memberName":
            case "memberName":
            {
                if (instance.m_memberName is null)
                {
                    return true;
                }
                if (instance.m_memberName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_externalId":
            case "externalId":
            {
                if (instance.m_externalId is null)
                {
                    return true;
                }
                if (instance.m_externalId is TGet castValue)
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
            case "m_memberName":
            case "memberName":
            {
                if (value is null)
                {
                    instance.m_memberName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_memberName = castValue;
                    return true;
                }
                return false;
            }
            case "m_externalId":
            case "externalId":
            {
                if (value is null)
                {
                    instance.m_externalId = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_externalId = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
