// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclStretchLinkConstraintSetMxSingleData : HavokData<hclStretchLinkConstraintSetMx.Single> 
{
    public hclStretchLinkConstraintSetMxSingleData(HavokType type, hclStretchLinkConstraintSetMx.Single instance) : base(type, instance) {}

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
            case "m_stiffness":
            case "stiffness":
            {
                if (instance.m_stiffness is not TGet castValue) return false;
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
            case "m_stiffness":
            case "stiffness":
            {
                if (value is not float castValue) return false;
                instance.m_stiffness = castValue;
                return true;
            }
            case "m_particleA":
            case "particleA":
            {
                if (value is not uint castValue) return false;
                instance.m_particleA = castValue;
                return true;
            }
            case "m_particleB":
            case "particleB":
            {
                if (value is not uint castValue) return false;
                instance.m_particleB = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
