// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpSpringActionData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpSpringAction>
{
    public hknpSpringActionData(Havok.hk2019.hknpSpringAction instance) : base(instance) { }

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
            case "m_bodyIdA":
            case "bodyIdA":
            {
                value = instance.m_bodyIdA;
                return true;
            }
            case "m_bodyIdB":
            case "bodyIdB":
            {
                value = instance.m_bodyIdB;
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
            case "m_bodyIdA":
            case "bodyIdA":
            {
                if (value is not Havok.hk2019.hknpBodyId castValue) return false;
                instance.m_bodyIdA = castValue;
                return true;
            }
            case "m_bodyIdB":
            case "bodyIdB":
            {
                if (value is not Havok.hk2019.hknpBodyId castValue) return false;
                instance.m_bodyIdB = castValue;
                return true;
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
