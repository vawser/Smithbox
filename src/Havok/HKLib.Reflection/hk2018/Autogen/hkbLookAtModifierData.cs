// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbLookAtModifierData : HavokData<hkbLookAtModifier> 
{
    public hkbLookAtModifierData(HavokType type, hkbLookAtModifier instance) : base(type, instance) {}

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
            case "m_targetWS":
            case "targetWS":
            {
                if (instance.m_targetWS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_headForwardLS":
            case "headForwardLS":
            {
                if (instance.m_headForwardLS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_neckForwardLS":
            case "neckForwardLS":
            {
                if (instance.m_neckForwardLS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_neckRightLS":
            case "neckRightLS":
            {
                if (instance.m_neckRightLS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_eyePositionHS":
            case "eyePositionHS":
            {
                if (instance.m_eyePositionHS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_newTargetGain":
            case "newTargetGain":
            {
                if (instance.m_newTargetGain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_onGain":
            case "onGain":
            {
                if (instance.m_onGain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_offGain":
            case "offGain":
            {
                if (instance.m_offGain is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_limitAngleDegrees":
            case "limitAngleDegrees":
            {
                if (instance.m_limitAngleDegrees is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_limitAngleLeft":
            case "limitAngleLeft":
            {
                if (instance.m_limitAngleLeft is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_limitAngleRight":
            case "limitAngleRight":
            {
                if (instance.m_limitAngleRight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_limitAngleUp":
            case "limitAngleUp":
            {
                if (instance.m_limitAngleUp is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_limitAngleDown":
            case "limitAngleDown":
            {
                if (instance.m_limitAngleDown is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_headIndex":
            case "headIndex":
            {
                if (instance.m_headIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_neckIndex":
            case "neckIndex":
            {
                if (instance.m_neckIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isOn":
            case "isOn":
            {
                if (instance.m_isOn is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_individualLimitsOn":
            case "individualLimitsOn":
            {
                if (instance.m_individualLimitsOn is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isTargetInsideLimitCone":
            case "isTargetInsideLimitCone":
            {
                if (instance.m_isTargetInsideLimitCone is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_SensingAngle":
            case "SensingAngle":
            {
                if (instance.m_SensingAngle is not TGet castValue) return false;
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
            case "m_targetWS":
            case "targetWS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_targetWS = castValue;
                return true;
            }
            case "m_headForwardLS":
            case "headForwardLS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_headForwardLS = castValue;
                return true;
            }
            case "m_neckForwardLS":
            case "neckForwardLS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_neckForwardLS = castValue;
                return true;
            }
            case "m_neckRightLS":
            case "neckRightLS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_neckRightLS = castValue;
                return true;
            }
            case "m_eyePositionHS":
            case "eyePositionHS":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_eyePositionHS = castValue;
                return true;
            }
            case "m_newTargetGain":
            case "newTargetGain":
            {
                if (value is not float castValue) return false;
                instance.m_newTargetGain = castValue;
                return true;
            }
            case "m_onGain":
            case "onGain":
            {
                if (value is not float castValue) return false;
                instance.m_onGain = castValue;
                return true;
            }
            case "m_offGain":
            case "offGain":
            {
                if (value is not float castValue) return false;
                instance.m_offGain = castValue;
                return true;
            }
            case "m_limitAngleDegrees":
            case "limitAngleDegrees":
            {
                if (value is not float castValue) return false;
                instance.m_limitAngleDegrees = castValue;
                return true;
            }
            case "m_limitAngleLeft":
            case "limitAngleLeft":
            {
                if (value is not float castValue) return false;
                instance.m_limitAngleLeft = castValue;
                return true;
            }
            case "m_limitAngleRight":
            case "limitAngleRight":
            {
                if (value is not float castValue) return false;
                instance.m_limitAngleRight = castValue;
                return true;
            }
            case "m_limitAngleUp":
            case "limitAngleUp":
            {
                if (value is not float castValue) return false;
                instance.m_limitAngleUp = castValue;
                return true;
            }
            case "m_limitAngleDown":
            case "limitAngleDown":
            {
                if (value is not float castValue) return false;
                instance.m_limitAngleDown = castValue;
                return true;
            }
            case "m_headIndex":
            case "headIndex":
            {
                if (value is not short castValue) return false;
                instance.m_headIndex = castValue;
                return true;
            }
            case "m_neckIndex":
            case "neckIndex":
            {
                if (value is not short castValue) return false;
                instance.m_neckIndex = castValue;
                return true;
            }
            case "m_isOn":
            case "isOn":
            {
                if (value is not bool castValue) return false;
                instance.m_isOn = castValue;
                return true;
            }
            case "m_individualLimitsOn":
            case "individualLimitsOn":
            {
                if (value is not bool castValue) return false;
                instance.m_individualLimitsOn = castValue;
                return true;
            }
            case "m_isTargetInsideLimitCone":
            case "isTargetInsideLimitCone":
            {
                if (value is not bool castValue) return false;
                instance.m_isTargetInsideLimitCone = castValue;
                return true;
            }
            case "m_SensingAngle":
            case "SensingAngle":
            {
                if (value is not short castValue) return false;
                instance.m_SensingAngle = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
