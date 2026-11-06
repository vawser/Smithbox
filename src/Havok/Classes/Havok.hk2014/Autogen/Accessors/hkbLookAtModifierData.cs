// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbLookAtModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbLookAtModifier>
{
    public hkbLookAtModifierData(Havok.hk2014.hkbLookAtModifier instance) : base(instance) { }

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
            case "m_targetWS":
            case "targetWS":
            {
                value = instance.m_targetWS;
                return true;
            }
            case "m_headForwardLS":
            case "headForwardLS":
            {
                value = instance.m_headForwardLS;
                return true;
            }
            case "m_neckForwardLS":
            case "neckForwardLS":
            {
                value = instance.m_neckForwardLS;
                return true;
            }
            case "m_neckRightLS":
            case "neckRightLS":
            {
                value = instance.m_neckRightLS;
                return true;
            }
            case "m_eyePositionHS":
            case "eyePositionHS":
            {
                value = instance.m_eyePositionHS;
                return true;
            }
            case "m_newTargetGain":
            case "newTargetGain":
            {
                value = instance.m_newTargetGain;
                return true;
            }
            case "m_onGain":
            case "onGain":
            {
                value = instance.m_onGain;
                return true;
            }
            case "m_offGain":
            case "offGain":
            {
                value = instance.m_offGain;
                return true;
            }
            case "m_limitAngleDegrees":
            case "limitAngleDegrees":
            {
                value = instance.m_limitAngleDegrees;
                return true;
            }
            case "m_limitAngleLeft":
            case "limitAngleLeft":
            {
                value = instance.m_limitAngleLeft;
                return true;
            }
            case "m_limitAngleRight":
            case "limitAngleRight":
            {
                value = instance.m_limitAngleRight;
                return true;
            }
            case "m_limitAngleUp":
            case "limitAngleUp":
            {
                value = instance.m_limitAngleUp;
                return true;
            }
            case "m_limitAngleDown":
            case "limitAngleDown":
            {
                value = instance.m_limitAngleDown;
                return true;
            }
            case "m_headIndex":
            case "headIndex":
            {
                value = instance.m_headIndex;
                return true;
            }
            case "m_neckIndex":
            case "neckIndex":
            {
                value = instance.m_neckIndex;
                return true;
            }
            case "m_isOn":
            case "isOn":
            {
                value = instance.m_isOn;
                return true;
            }
            case "m_individualLimitsOn":
            case "individualLimitsOn":
            {
                value = instance.m_individualLimitsOn;
                return true;
            }
            case "m_isTargetInsideLimitCone":
            case "isTargetInsideLimitCone":
            {
                value = instance.m_isTargetInsideLimitCone;
                return true;
            }
            case "m_SensingAngle":
            case "SensingAngle":
            {
                value = instance.m_SensingAngle;
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
