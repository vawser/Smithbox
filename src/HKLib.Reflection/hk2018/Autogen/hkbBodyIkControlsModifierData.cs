// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbBodyIkControlsModifierData : HavokData<hkbBodyIkControlsModifier> 
{
    public hkbBodyIkControlsModifierData(HavokType type, hkbBodyIkControlsModifier instance) : base(type, instance) {}

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
            case "m_profileName":
            case "profileName":
            {
                if (instance.m_profileName is null)
                {
                    return true;
                }
                if (instance.m_profileName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_controlDatas":
            case "controlDatas":
            {
                if (instance.m_controlDatas is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_posePredictionMode":
            case "posePredictionMode":
            {
                if (instance.m_posePredictionMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_posePredictionMode is TGet sbyteValue)
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
            case "m_profileName":
            case "profileName":
            {
                if (value is null)
                {
                    instance.m_profileName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_profileName = castValue;
                    return true;
                }
                return false;
            }
            case "m_controlDatas":
            case "controlDatas":
            {
                if (value is not List<hkbBodyIkControlsModifier.ControlData> castValue) return false;
                instance.m_controlDatas = castValue;
                return true;
            }
            case "m_posePredictionMode":
            case "posePredictionMode":
            {
                if (value is hkbBodyIkControlsModifier.PosePredictionMode castValue)
                {
                    instance.m_posePredictionMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_posePredictionMode = (hkbBodyIkControlsModifier.PosePredictionMode)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
