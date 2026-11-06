// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbComputeRotationToTargetModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbComputeRotationToTargetModifier>
{
    public hkbComputeRotationToTargetModifierData(Havok.hk2014.hkbComputeRotationToTargetModifier instance) : base(instance) { }

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
            case "m_rotationOut":
            case "rotationOut":
            {
                value = instance.m_rotationOut;
                return true;
            }
            case "m_targetPosition":
            case "targetPosition":
            {
                value = instance.m_targetPosition;
                return true;
            }
            case "m_currentPosition":
            case "currentPosition":
            {
                value = instance.m_currentPosition;
                return true;
            }
            case "m_currentRotation":
            case "currentRotation":
            {
                value = instance.m_currentRotation;
                return true;
            }
            case "m_localAxisOfRotation":
            case "localAxisOfRotation":
            {
                value = instance.m_localAxisOfRotation;
                return true;
            }
            case "m_localFacingDirection":
            case "localFacingDirection":
            {
                value = instance.m_localFacingDirection;
                return true;
            }
            case "m_resultIsDelta":
            case "resultIsDelta":
            {
                value = instance.m_resultIsDelta;
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
            case "m_rotationOut":
            case "rotationOut":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_rotationOut = castValue;
                return true;
            }
            case "m_targetPosition":
            case "targetPosition":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_targetPosition = castValue;
                return true;
            }
            case "m_currentPosition":
            case "currentPosition":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_currentPosition = castValue;
                return true;
            }
            case "m_currentRotation":
            case "currentRotation":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_currentRotation = castValue;
                return true;
            }
            case "m_localAxisOfRotation":
            case "localAxisOfRotation":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_localAxisOfRotation = castValue;
                return true;
            }
            case "m_localFacingDirection":
            case "localFacingDirection":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_localFacingDirection = castValue;
                return true;
            }
            case "m_resultIsDelta":
            case "resultIsDelta":
            {
                if (value is not bool castValue) return false;
                instance.m_resultIsDelta = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
