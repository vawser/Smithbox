// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpPoweredChainMapperData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpPoweredChainMapper>
{
    public hkpPoweredChainMapperData(Havok.hk2014.hkpPoweredChainMapper instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_links":
            case "links":
            {
                value = instance.m_links;
                return true;
            }
            case "m_targets":
            case "targets":
            {
                value = instance.m_targets;
                return true;
            }
            case "m_chains":
            case "chains":
            {
                value = instance.m_chains;
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
            case "m_links":
            case "links":
            {
                if (value is not List<Havok.hk2014.hkpPoweredChainMapperLinkInfo> castValue) return false;
                instance.m_links = castValue;
                return true;
            }
            case "m_targets":
            case "targets":
            {
                if (value is not List<Havok.hk2014.hkpPoweredChainMapperTarget> castValue) return false;
                instance.m_targets = castValue;
                return true;
            }
            case "m_chains":
            case "chains":
            {
                if (value is not List<Havok.hk2014.hkpConstraintChainInstance?> castValue) return false;
                instance.m_chains = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
