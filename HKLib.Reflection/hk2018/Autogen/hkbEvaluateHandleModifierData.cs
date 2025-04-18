// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbEvaluateHandleModifierData : HavokData<hkbEvaluateHandleModifier> 
{
    public hkbEvaluateHandleModifierData(HavokType type, hkbEvaluateHandleModifier instance) : base(type, instance) {}

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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (instance.m_variableBindingSet is null)
                {
                    return true;
                }
                if (instance.m_variableBindingSet is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_enable":
            case "enable":
            {
                if (instance.m_enable is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_handle":
            case "handle":
            {
                if (instance.m_handle is null)
                {
                    return true;
                }
                if (instance.m_handle is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_handlePositionOut":
            case "handlePositionOut":
            {
                if (instance.m_handlePositionOut is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_handleRotationOut":
            case "handleRotationOut":
            {
                if (instance.m_handleRotationOut is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isValidOut":
            case "isValidOut":
            {
                if (instance.m_isValidOut is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_extrapolationTimeStep":
            case "extrapolationTimeStep":
            {
                if (instance.m_extrapolationTimeStep is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_handleChangeSpeed":
            case "handleChangeSpeed":
            {
                if (instance.m_handleChangeSpeed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_handleChangeMode":
            case "handleChangeMode":
            {
                if (instance.m_handleChangeMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_handleChangeMode is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is hkbVariableBindingSet castValue)
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
                if (value is hkbHandle castValue)
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
                if (value is hkbEvaluateHandleModifier.HandleChangeMode castValue)
                {
                    instance.m_handleChangeMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_handleChangeMode = (hkbEvaluateHandleModifier.HandleChangeMode)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
