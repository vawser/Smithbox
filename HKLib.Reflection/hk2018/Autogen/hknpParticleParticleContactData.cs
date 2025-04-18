// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpParticleParticleContactData : HavokData<hknpParticleParticleContact> 
{
    public hknpParticleParticleContactData(HavokType type, hknpParticleParticleContact instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_position":
            case "position":
            {
                if (instance.m_position is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_normal":
            case "normal":
            {
                if (instance.m_normal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
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
            case "m_distance":
            case "distance":
            {
                if (instance.m_distance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_impulse":
            case "impulse":
            {
                if (instance.m_impulse is not TGet castValue) return false;
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
