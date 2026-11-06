// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpParticleCollisionPairData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpParticleCollisionPair>
{
    public hknpParticleCollisionPairData(Havok.hk2019.hknpParticleCollisionPair instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_particleIndexA":
            case "particleIndexA":
            {
                value = instance.m_particleIndexA;
                return true;
            }
            case "m_particleIndexB":
            case "particleIndexB":
            {
                value = instance.m_particleIndexB;
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
