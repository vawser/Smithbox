// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpParticleParticleContactData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpParticleParticleContact>
{
    public hknpParticleParticleContactData(Havok.hk2019.hknpParticleParticleContact instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_position":
            case "position":
            {
                value = instance.m_position;
                return true;
            }
            case "m_normal":
            case "normal":
            {
                value = instance.m_normal;
                return true;
            }
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
            case "m_distance":
            case "distance":
            {
                value = instance.m_distance;
                return true;
            }
            case "m_impulse":
            case "impulse":
            {
                value = instance.m_impulse;
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
            case "m_position":
            case "position":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_position = castValue;
                return true;
            }
            case "m_normal":
            case "normal":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_normal = castValue;
                return true;
            }
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
            case "m_distance":
            case "distance":
            {
                if (value is not float castValue) return false;
                instance.m_distance = castValue;
                return true;
            }
            case "m_impulse":
            case "impulse":
            {
                if (value is not float castValue) return false;
                instance.m_impulse = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
