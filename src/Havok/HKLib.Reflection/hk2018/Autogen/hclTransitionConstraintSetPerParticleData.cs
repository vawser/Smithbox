// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclTransitionConstraintSetPerParticleData : HavokData<hclTransitionConstraintSet.PerParticle> 
{
    public hclTransitionConstraintSetPerParticleData(HavokType type, hclTransitionConstraintSet.PerParticle instance) : base(type, instance) {}

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
            case "m_toAnimDelay":
            case "toAnimDelay":
            {
                if (instance.m_toAnimDelay is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_toSimDelay":
            case "toSimDelay":
            {
                if (instance.m_toSimDelay is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_toSimMaxDistance":
            case "toSimMaxDistance":
            {
                if (instance.m_toSimMaxDistance is not TGet castValue) return false;
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
