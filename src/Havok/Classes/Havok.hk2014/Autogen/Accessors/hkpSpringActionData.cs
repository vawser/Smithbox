// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpSpringActionData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpSpringAction>
{
    public hkpSpringActionData(Havok.hk2014.hkpSpringAction instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_entityA":
            case "entityA":
            {
                value = instance.m_entityA;
                return true;
            }
            case "m_entityB":
            case "entityB":
            {
                value = instance.m_entityB;
                return true;
            }
            case "m_lastForce":
            case "lastForce":
            {
                value = instance.m_lastForce;
                return true;
            }
            case "m_positionAinA":
            case "positionAinA":
            {
                value = instance.m_positionAinA;
                return true;
            }
            case "m_positionBinB":
            case "positionBinB":
            {
                value = instance.m_positionBinB;
                return true;
            }
            case "m_restLength":
            case "restLength":
            {
                value = instance.m_restLength;
                return true;
            }
            case "m_strength":
            case "strength":
            {
                value = instance.m_strength;
                return true;
            }
            case "m_damping":
            case "damping":
            {
                value = instance.m_damping;
                return true;
            }
            case "m_onCompression":
            case "onCompression":
            {
                value = instance.m_onCompression;
                return true;
            }
            case "m_onExtension":
            case "onExtension":
            {
                value = instance.m_onExtension;
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
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_entityA":
            case "entityA":
            {
                if (value is null)
                {
                    instance.m_entityA = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpEntity castValue)
                {
                    instance.m_entityA = castValue;
                    return true;
                }
                return false;
            }
            case "m_entityB":
            case "entityB":
            {
                if (value is null)
                {
                    instance.m_entityB = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpEntity castValue)
                {
                    instance.m_entityB = castValue;
                    return true;
                }
                return false;
            }
            case "m_lastForce":
            case "lastForce":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_lastForce = castValue;
                return true;
            }
            case "m_positionAinA":
            case "positionAinA":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_positionAinA = castValue;
                return true;
            }
            case "m_positionBinB":
            case "positionBinB":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_positionBinB = castValue;
                return true;
            }
            case "m_restLength":
            case "restLength":
            {
                if (value is not float castValue) return false;
                instance.m_restLength = castValue;
                return true;
            }
            case "m_strength":
            case "strength":
            {
                if (value is not float castValue) return false;
                instance.m_strength = castValue;
                return true;
            }
            case "m_damping":
            case "damping":
            {
                if (value is not float castValue) return false;
                instance.m_damping = castValue;
                return true;
            }
            case "m_onCompression":
            case "onCompression":
            {
                if (value is not bool castValue) return false;
                instance.m_onCompression = castValue;
                return true;
            }
            case "m_onExtension":
            case "onExtension":
            {
                if (value is not bool castValue) return false;
                instance.m_onExtension = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
