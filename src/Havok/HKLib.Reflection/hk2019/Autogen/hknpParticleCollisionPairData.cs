// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpParticleCollisionPairData : HavokData<HKLib.hk2019.hknpParticleCollisionPair> 
{
    public hknpParticleCollisionPairData(HavokType type, HKLib.hk2019.hknpParticleCollisionPair instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_particleIndexA":
            case "particleIndexA":
            {
                if (instance.m_particleIndexA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_particleIndexB":
            case "particleIndexB":
            {
                if (instance.m_particleIndexB is not TGet castValue) return false;
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
            case "m_particleIndexA":
            case "particleIndexA":
            {
                if (value is not int castValue) return false;
                instance.m_particleIndexA = castValue;
                return true;
            }
            case "m_particleIndexB":
            case "particleIndexB":
            {
                if (value is not int castValue) return false;
                instance.m_particleIndexB = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
