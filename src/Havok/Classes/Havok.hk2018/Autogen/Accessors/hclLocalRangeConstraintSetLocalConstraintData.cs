// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclLocalRangeConstraintSetLocalConstraintData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclLocalRangeConstraintSet.LocalConstraint>
{
    public hclLocalRangeConstraintSetLocalConstraintData(Havok.hk2018.hclLocalRangeConstraintSet.LocalConstraint instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_particleIndex":
            case "particleIndex":
            {
                value = instance.m_particleIndex;
                return true;
            }
            case "m_referenceVertex":
            case "referenceVertex":
            {
                value = instance.m_referenceVertex;
                return true;
            }
            case "m_maximumDistance":
            case "maximumDistance":
            {
                value = instance.m_maximumDistance;
                return true;
            }
            case "m_maxNormalDistance":
            case "maxNormalDistance":
            {
                value = instance.m_maxNormalDistance;
                return true;
            }
            case "m_minNormalDistance":
            case "minNormalDistance":
            {
                value = instance.m_minNormalDistance;
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
