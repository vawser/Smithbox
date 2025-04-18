// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpFirstPersonCharacterCinfoData : HavokData<hknpFirstPersonCharacterCinfo> 
{
    public hknpFirstPersonCharacterCinfoData(HavokType type, hknpFirstPersonCharacterCinfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_position":
            case "position":
            {
                if (instance.m_position is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_direction":
            case "direction":
            {
                if (instance.m_direction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_up":
            case "up":
            {
                if (instance.m_up is not TGet castValue) return false;
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
            case "m_mass":
            case "mass":
            {
                if (instance.m_mass is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_capsuleHeight":
            case "capsuleHeight":
            {
                if (instance.m_capsuleHeight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_capsuleRadius":
            case "capsuleRadius":
            {
                if (instance.m_capsuleRadius is not TGet castValue) return false;
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
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_flags is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
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
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_position":
            case "position":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_position = castValue;
                return true;
            }
            case "m_direction":
            case "direction":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_direction = castValue;
                return true;
            }
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            case "m_gravityStrength":
            case "gravityStrength":
            {
                if (value is not float castValue) return false;
                instance.m_gravityStrength = castValue;
                return true;
            }
            case "m_mass":
            case "mass":
            {
                if (value is not float castValue) return false;
                instance.m_mass = castValue;
                return true;
            }
            case "m_capsuleHeight":
            case "capsuleHeight":
            {
                if (value is not float castValue) return false;
                instance.m_capsuleHeight = castValue;
                return true;
            }
            case "m_capsuleRadius":
            case "capsuleRadius":
            {
                if (value is not float castValue) return false;
                instance.m_capsuleRadius = castValue;
                return true;
            }
            case "m_eyeHeight":
            case "eyeHeight":
            {
                if (value is not float castValue) return false;
                instance.m_eyeHeight = castValue;
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
            case "m_flags":
            case "flags":
            {
                if (value is hknpFirstPersonCharacter.ControlFlags castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_flags = (hknpFirstPersonCharacter.ControlFlags)intValue;
                    return true;
                }
                return false;
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
            default:
            return false;
        }
    }

}
