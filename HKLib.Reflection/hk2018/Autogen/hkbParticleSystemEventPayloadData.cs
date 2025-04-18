// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbParticleSystemEventPayloadData : HavokData<hkbParticleSystemEventPayload> 
{
    public hkbParticleSystemEventPayloadData(HavokType type, hkbParticleSystemEventPayload instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_type is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_emitBoneIndex":
            case "emitBoneIndex":
            {
                if (instance.m_emitBoneIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_offset":
            case "offset":
            {
                if (instance.m_offset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_direction":
            case "direction":
            {
                if (instance.m_direction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numParticles":
            case "numParticles":
            {
                if (instance.m_numParticles is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_speed":
            case "speed":
            {
                if (instance.m_speed is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (value is hkbParticleSystemEventPayload.SystemType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (hkbParticleSystemEventPayload.SystemType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_emitBoneIndex":
            case "emitBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_emitBoneIndex = castValue;
                return true;
            }
            case "m_offset":
            case "offset":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_offset = castValue;
                return true;
            }
            case "m_direction":
            case "direction":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_direction = castValue;
                return true;
            }
            case "m_numParticles":
            case "numParticles":
            {
                if (value is not int castValue) return false;
                instance.m_numParticles = castValue;
                return true;
            }
            case "m_speed":
            case "speed":
            {
                if (value is not float castValue) return false;
                instance.m_speed = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
