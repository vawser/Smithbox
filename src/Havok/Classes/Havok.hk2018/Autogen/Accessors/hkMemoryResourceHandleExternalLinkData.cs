// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkMemoryResourceHandleExternalLinkData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkMemoryResourceHandle.ExternalLink>
{
    public hkMemoryResourceHandleExternalLinkData(Havok.hk2018.hkMemoryResourceHandle.ExternalLink instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_memberName":
            case "memberName":
            {
                value = instance.m_memberName;
                return true;
            }
            case "m_externalId":
            case "externalId":
            {
                value = instance.m_externalId;
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
