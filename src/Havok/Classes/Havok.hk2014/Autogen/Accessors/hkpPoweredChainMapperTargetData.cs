// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpPoweredChainMapperTargetData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpPoweredChainMapperTarget>
{
    public hkpPoweredChainMapperTargetData(Havok.hk2014.hkpPoweredChainMapperTarget instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_chain":
            case "chain":
            {
                value = instance.m_chain;
                return true;
            }
            case "m_infoIndex":
            case "infoIndex":
            {
                value = instance.m_infoIndex;
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
            case "m_chain":
            case "chain":
            {
                if (value is null)
                {
                    instance.m_chain = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpPoweredChainData castValue)
                {
                    instance.m_chain = castValue;
                    return true;
                }
                return false;
            }
            case "m_infoIndex":
            case "infoIndex":
            {
                if (value is not int castValue) return false;
                instance.m_infoIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
