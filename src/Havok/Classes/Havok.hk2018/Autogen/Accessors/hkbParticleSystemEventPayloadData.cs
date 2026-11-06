// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbParticleSystemEventPayloadData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbParticleSystemEventPayload>
{
    public hkbParticleSystemEventPayloadData(Havok.hk2018.hkbParticleSystemEventPayload instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_emitBoneIndex":
            case "emitBoneIndex":
            {
                value = instance.m_emitBoneIndex;
                return true;
            }
            case "m_offset":
            case "offset":
            {
                value = instance.m_offset;
                return true;
            }
            case "m_direction":
            case "direction":
            {
                value = instance.m_direction;
                return true;
            }
            case "m_numParticles":
            case "numParticles":
            {
                value = instance.m_numParticles;
                return true;
            }
            case "m_speed":
            case "speed":
            {
                value = instance.m_speed;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2018.hkbParticleSystemEventPayload.SystemType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (Havok.hk2018.hkbParticleSystemEventPayload.SystemType)byteValue;
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
