// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclStandardLinkConstraintSetMxSingleData : HavokData<hclStandardLinkConstraintSetMx.Single> 
{
    public hclStandardLinkConstraintSetMxSingleData(HavokType type, hclStandardLinkConstraintSetMx.Single instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_restLength":
            case "restLength":
            {
                if (instance.m_restLength is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stiffnessA":
            case "stiffnessA":
            {
                if (instance.m_stiffnessA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stiffnessB":
            case "stiffnessB":
            {
                if (instance.m_stiffnessB is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_particleA":
            case "particleA":
            {
                if (instance.m_particleA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_particleB":
            case "particleB":
            {
                if (instance.m_particleB is not TGet castValue) return false;
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
