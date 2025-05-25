// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpFirstPersonCharacterData : HavokData<hknpFirstPersonCharacter> 
{
    public hknpFirstPersonCharacterData(HavokType type, hknpFirstPersonCharacter instance) : base(type, instance) {}

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
            case "m_verticalSensitivity":
            case "verticalSensitivity":
            {
                if (instance.m_verticalSensitivity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_horizontalSensitivity":
            case "horizontalSensitivity":
            {
                if (instance.m_horizontalSensitivity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sensivityPadX":
            case "sensivityPadX":
            {
                if (instance.m_sensivityPadX is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sensivityPadY":
            case "sensivityPadY":
            {
                if (instance.m_sensivityPadY is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_eyeHeight":
            case "eyeHeight":
            {
                if (instance.m_eyeHeight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_gravityStrength":
            case "gravityStrength":
            {
                if (instance.m_gravityStrength is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxUpDownAngle":
            case "maxUpDownAngle":
            {
                if (instance.m_maxUpDownAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numFramesPerShot":
            case "numFramesPerShot":
            {
                if (instance.m_numFramesPerShot is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_forwardBackwardSpeedModifier":
            case "forwardBackwardSpeedModifier":
            {
                if (instance.m_forwardBackwardSpeedModifier is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_leftRightSpeedModifier":
            case "leftRightSpeedModifier":
            {
                if (instance.m_leftRightSpeedModifier is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_gunCounter":
            case "gunCounter":
            {
                if (instance.m_gunCounter is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_gunCounterRmb":
            case "gunCounterRmb":
            {
                if (instance.m_gunCounterRmb is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_currentAngle":
            case "currentAngle":
            {
                if (instance.m_currentAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_currentElevation":
            case "currentElevation":
            {
                if (instance.m_currentElevation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_specialGravity":
            case "specialGravity":
            {
                if (instance.m_specialGravity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_gravity":
            case "gravity":
            {
                if (instance.m_gravity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_currentGun":
            case "currentGun":
            {
                if (instance.m_currentGun is null)
                {
                    return true;
                }
                if (instance.m_currentGun is TGet castValue)
                {
                    value = castValue;
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
            case "m_verticalSensitivity":
            case "verticalSensitivity":
            {
                if (value is not float castValue) return false;
                instance.m_verticalSensitivity = castValue;
                return true;
            }
            case "m_horizontalSensitivity":
            case "horizontalSensitivity":
            {
                if (value is not float castValue) return false;
                instance.m_horizontalSensitivity = castValue;
                return true;
            }
            case "m_sensivityPadX":
            case "sensivityPadX":
            {
                if (value is not float castValue) return false;
                instance.m_sensivityPadX = castValue;
                return true;
            }
            case "m_sensivityPadY":
            case "sensivityPadY":
            {
                if (value is not float castValue) return false;
                instance.m_sensivityPadY = castValue;
                return true;
            }
            case "m_eyeHeight":
            case "eyeHeight":
            {
                if (value is not float castValue) return false;
                instance.m_eyeHeight = castValue;
                return true;
            }
            case "m_gravityStrength":
            case "gravityStrength":
            {
                if (value is not float castValue) return false;
                instance.m_gravityStrength = castValue;
                return true;
            }
            case "m_maxUpDownAngle":
            case "maxUpDownAngle":
            {
                if (value is not float castValue) return false;
                instance.m_maxUpDownAngle = castValue;
                return true;
            }
            case "m_numFramesPerShot":
            case "numFramesPerShot":
            {
                if (value is not uint castValue) return false;
                instance.m_numFramesPerShot = castValue;
                return true;
            }
            case "m_forwardBackwardSpeedModifier":
            case "forwardBackwardSpeedModifier":
            {
                if (value is not float castValue) return false;
                instance.m_forwardBackwardSpeedModifier = castValue;
                return true;
            }
            case "m_leftRightSpeedModifier":
            case "leftRightSpeedModifier":
            {
                if (value is not float castValue) return false;
                instance.m_leftRightSpeedModifier = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is not uint castValue) return false;
                instance.m_flags = castValue;
                return true;
            }
            case "m_gunCounter":
            case "gunCounter":
            {
                if (value is not int castValue) return false;
                instance.m_gunCounter = castValue;
                return true;
            }
            case "m_gunCounterRmb":
            case "gunCounterRmb":
            {
                if (value is not int castValue) return false;
                instance.m_gunCounterRmb = castValue;
                return true;
            }
            case "m_currentAngle":
            case "currentAngle":
            {
                if (value is not float castValue) return false;
                instance.m_currentAngle = castValue;
                return true;
            }
            case "m_currentElevation":
            case "currentElevation":
            {
                if (value is not float castValue) return false;
                instance.m_currentElevation = castValue;
                return true;
            }
            case "m_specialGravity":
            case "specialGravity":
            {
                if (value is not bool castValue) return false;
                instance.m_specialGravity = castValue;
                return true;
            }
            case "m_gravity":
            case "gravity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_gravity = castValue;
                return true;
            }
            case "m_currentGun":
            case "currentGun":
            {
                if (value is null)
                {
                    instance.m_currentGun = default;
                    return true;
                }
                if (value is hknpFirstPersonGun castValue)
                {
                    instance.m_currentGun = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
