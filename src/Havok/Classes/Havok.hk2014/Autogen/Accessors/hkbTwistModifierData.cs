// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbTwistModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbTwistModifier>
{
    public hkbTwistModifierData(Havok.hk2014.hkbTwistModifier instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                value = instance.m_variableBindingSet;
                return true;
            }
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
            case "m_enable":
            case "enable":
            {
                value = instance.m_enable;
                return true;
            }
            case "m_axisOfRotation":
            case "axisOfRotation":
            {
                value = instance.m_axisOfRotation;
                return true;
            }
            case "m_twistAngle":
            case "twistAngle":
            {
                value = instance.m_twistAngle;
                return true;
            }
            case "m_startBoneIndex":
            case "startBoneIndex":
            {
                value = instance.m_startBoneIndex;
                return true;
            }
            case "m_endBoneIndex":
            case "endBoneIndex":
            {
                value = instance.m_endBoneIndex;
                return true;
            }
            case "m_setAngleMethod":
            case "setAngleMethod":
            {
                value = instance.m_setAngleMethod;
                return true;
            }
            case "m_rotationAxisCoordinates":
            case "rotationAxisCoordinates":
            {
                value = instance.m_rotationAxisCoordinates;
                return true;
            }
            case "m_isAdditive":
            case "isAdditive":
            {
                value = instance.m_isAdditive;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbVariableBindingSet castValue)
                {
                    instance.m_variableBindingSet = castValue;
                    return true;
                }
                return false;
            }
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
            case "m_enable":
            case "enable":
            {
                if (value is not bool castValue) return false;
                instance.m_enable = castValue;
                return true;
            }
            case "m_axisOfRotation":
            case "axisOfRotation":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_axisOfRotation = castValue;
                return true;
            }
            case "m_twistAngle":
            case "twistAngle":
            {
                if (value is not float castValue) return false;
                instance.m_twistAngle = castValue;
                return true;
            }
            case "m_startBoneIndex":
            case "startBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_startBoneIndex = castValue;
                return true;
            }
            case "m_endBoneIndex":
            case "endBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_endBoneIndex = castValue;
                return true;
            }
            case "m_setAngleMethod":
            case "setAngleMethod":
            {
                if (value is Havok.hk2014.hkbTwistModifier.SetAngleMethod castValue)
                {
                    instance.m_setAngleMethod = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_setAngleMethod = (Havok.hk2014.hkbTwistModifier.SetAngleMethod)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_rotationAxisCoordinates":
            case "rotationAxisCoordinates":
            {
                if (value is Havok.hk2014.hkbTwistModifier.RotationAxisCoordinates castValue)
                {
                    instance.m_rotationAxisCoordinates = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_rotationAxisCoordinates = (Havok.hk2014.hkbTwistModifier.RotationAxisCoordinates)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_isAdditive":
            case "isAdditive":
            {
                if (value is not bool castValue) return false;
                instance.m_isAdditive = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
