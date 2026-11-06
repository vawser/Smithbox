// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclTransitionConstraintSetPerParticleData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclTransitionConstraintSet.PerParticle>
{
    public hclTransitionConstraintSetPerParticleData(Havok.hk2016.hclTransitionConstraintSet.PerParticle instance) : base(instance) { }

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
            case "m_toAnimDelay":
            case "toAnimDelay":
            {
                value = instance.m_toAnimDelay;
                return true;
            }
            case "m_toSimDelay":
            case "toSimDelay":
            {
                value = instance.m_toSimDelay;
                return true;
            }
            case "m_toSimMaxDistance":
            case "toSimMaxDistance":
            {
                value = instance.m_toSimMaxDistance;
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
            case "m_toAnimDelay":
            case "toAnimDelay":
            {
                if (value is not float castValue) return false;
                instance.m_toAnimDelay = castValue;
                return true;
            }
            case "m_toSimDelay":
            case "toSimDelay":
            {
                if (value is not float castValue) return false;
                instance.m_toSimDelay = castValue;
                return true;
            }
            case "m_toSimMaxDistance":
            case "toSimMaxDistance":
            {
                if (value is not float castValue) return false;
                instance.m_toSimMaxDistance = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
