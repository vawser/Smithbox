// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpPoweredChainMapperLinkInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpPoweredChainMapperLinkInfo>
{
    public hkpPoweredChainMapperLinkInfoData(Havok.hk2014.hkpPoweredChainMapperLinkInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_firstTargetIdx":
            case "firstTargetIdx":
            {
                value = instance.m_firstTargetIdx;
                return true;
            }
            case "m_numTargets":
            case "numTargets":
            {
                value = instance.m_numTargets;
                return true;
            }
            case "m_limitConstraint":
            case "limitConstraint":
            {
                value = instance.m_limitConstraint;
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
            case "m_firstTargetIdx":
            case "firstTargetIdx":
            {
                if (value is not int castValue) return false;
                instance.m_firstTargetIdx = castValue;
                return true;
            }
            case "m_numTargets":
            case "numTargets":
            {
                if (value is not int castValue) return false;
                instance.m_numTargets = castValue;
                return true;
            }
            case "m_limitConstraint":
            case "limitConstraint":
            {
                if (value is null)
                {
                    instance.m_limitConstraint = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpConstraintInstance castValue)
                {
                    instance.m_limitConstraint = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
