// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclStandardLinkConstraintSetMxSingleData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclStandardLinkConstraintSetMx.Single>
{
    public hclStandardLinkConstraintSetMxSingleData(Havok.hk2014.hclStandardLinkConstraintSetMx.Single instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_restLength":
            case "restLength":
            {
                value = instance.m_restLength;
                return true;
            }
            case "m_stiffnessA":
            case "stiffnessA":
            {
                value = instance.m_stiffnessA;
                return true;
            }
            case "m_stiffnessB":
            case "stiffnessB":
            {
                value = instance.m_stiffnessB;
                return true;
            }
            case "m_particleA":
            case "particleA":
            {
                value = instance.m_particleA;
                return true;
            }
            case "m_particleB":
            case "particleB":
            {
                value = instance.m_particleB;
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
            case "m_restLength":
            case "restLength":
            {
                if (value is not float castValue) return false;
                instance.m_restLength = castValue;
                return true;
            }
            case "m_stiffnessA":
            case "stiffnessA":
            {
                if (value is not float castValue) return false;
                instance.m_stiffnessA = castValue;
                return true;
            }
            case "m_stiffnessB":
            case "stiffnessB":
            {
                if (value is not float castValue) return false;
                instance.m_stiffnessB = castValue;
                return true;
            }
            case "m_particleA":
            case "particleA":
            {
                if (value is not ushort castValue) return false;
                instance.m_particleA = castValue;
                return true;
            }
            case "m_particleB":
            case "particleB":
            {
                if (value is not ushort castValue) return false;
                instance.m_particleB = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
