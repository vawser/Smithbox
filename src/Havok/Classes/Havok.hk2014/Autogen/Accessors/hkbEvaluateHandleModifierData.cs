// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbEvaluateHandleModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbEvaluateHandleModifier>
{
    public hkbEvaluateHandleModifierData(Havok.hk2014.hkbEvaluateHandleModifier instance) : base(instance) { }

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
            case "m_handle":
            case "handle":
            {
                value = instance.m_handle;
                return true;
            }
            case "m_handlePositionOut":
            case "handlePositionOut":
            {
                value = instance.m_handlePositionOut;
                return true;
            }
            case "m_handleRotationOut":
            case "handleRotationOut":
            {
                value = instance.m_handleRotationOut;
                return true;
            }
            case "m_isValidOut":
            case "isValidOut":
            {
                value = instance.m_isValidOut;
                return true;
            }
            case "m_extrapolationTimeStep":
            case "extrapolationTimeStep":
            {
                value = instance.m_extrapolationTimeStep;
                return true;
            }
            case "m_handleChangeSpeed":
            case "handleChangeSpeed":
            {
                value = instance.m_handleChangeSpeed;
                return true;
            }
            case "m_handleChangeMode":
            case "handleChangeMode":
            {
                value = instance.m_handleChangeMode;
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
            case "m_handle":
            case "handle":
            {
                if (value is null)
                {
                    instance.m_handle = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbHandle castValue)
                {
                    instance.m_handle = castValue;
                    return true;
                }
                return false;
            }
            case "m_handlePositionOut":
            case "handlePositionOut":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_handlePositionOut = castValue;
                return true;
            }
            case "m_handleRotationOut":
            case "handleRotationOut":
            {
                if (value is not Quaternion castValue) return false;
                instance.m_handleRotationOut = castValue;
                return true;
            }
            case "m_isValidOut":
            case "isValidOut":
            {
                if (value is not bool castValue) return false;
                instance.m_isValidOut = castValue;
                return true;
            }
            case "m_extrapolationTimeStep":
            case "extrapolationTimeStep":
            {
                if (value is not float castValue) return false;
                instance.m_extrapolationTimeStep = castValue;
                return true;
            }
            case "m_handleChangeSpeed":
            case "handleChangeSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_handleChangeSpeed = castValue;
                return true;
            }
            case "m_handleChangeMode":
            case "handleChangeMode":
            {
                if (value is Havok.hk2014.hkbEvaluateHandleModifier.HandleChangeMode castValue)
                {
                    instance.m_handleChangeMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_handleChangeMode = (Havok.hk2014.hkbEvaluateHandleModifier.HandleChangeMode)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
