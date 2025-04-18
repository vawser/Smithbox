// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclLocalRangeConstraintSetLocalConstraintData : HavokData<hclLocalRangeConstraintSet.LocalConstraint> 
{
    public hclLocalRangeConstraintSetLocalConstraintData(HavokType type, hclLocalRangeConstraintSet.LocalConstraint instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_particleIndex":
            case "particleIndex":
            {
                if (instance.m_particleIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_referenceVertex":
            case "referenceVertex":
            {
                if (instance.m_referenceVertex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maximumDistance":
            case "maximumDistance":
            {
                if (instance.m_maximumDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxNormalDistance":
            case "maxNormalDistance":
            {
                if (instance.m_maxNormalDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minNormalDistance":
            case "minNormalDistance":
            {
                if (instance.m_minNormalDistance is not TGet castValue) return false;
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
            case "m_particleIndex":
            case "particleIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_particleIndex = castValue;
                return true;
            }
            case "m_referenceVertex":
            case "referenceVertex":
            {
                if (value is not ushort castValue) return false;
                instance.m_referenceVertex = castValue;
                return true;
            }
            case "m_maximumDistance":
            case "maximumDistance":
            {
                if (value is not float castValue) return false;
                instance.m_maximumDistance = castValue;
                return true;
            }
            case "m_maxNormalDistance":
            case "maxNormalDistance":
            {
                if (value is not float castValue) return false;
                instance.m_maxNormalDistance = castValue;
                return true;
            }
            case "m_minNormalDistance":
            case "minNormalDistance":
            {
                if (value is not float castValue) return false;
                instance.m_minNormalDistance = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
