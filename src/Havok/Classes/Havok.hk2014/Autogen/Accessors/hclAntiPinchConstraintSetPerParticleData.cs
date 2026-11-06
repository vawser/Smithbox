// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclAntiPinchConstraintSetPerParticleData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclAntiPinchConstraintSet.PerParticle>
{
    public hclAntiPinchConstraintSetPerParticleData(Havok.hk2014.hclAntiPinchConstraintSet.PerParticle instance) : base(instance) { }

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
            default:
            return false;
        }
    }

}
