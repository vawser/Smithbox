// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpFirstPersonCharacterCinfoData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpFirstPersonCharacterCinfo>
{
    public hknpFirstPersonCharacterCinfoData(Havok.hk2019.hknpFirstPersonCharacterCinfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_position":
            case "position":
            {
                value = instance.m_position;
                return true;
            }
            case "m_direction":
            case "direction":
            {
                value = instance.m_direction;
                return true;
            }
            case "m_up":
            case "up":
            {
                value = instance.m_up;
                return true;
            }
            case "m_gravityStrength":
            case "gravityStrength":
            {
                value = instance.m_gravityStrength;
                return true;
            }
            case "m_mass":
            case "mass":
            {
                value = instance.m_mass;
                return true;
            }
            case "m_capsuleHeight":
            case "capsuleHeight":
            {
                value = instance.m_capsuleHeight;
                return true;
            }
            case "m_capsuleRadius":
            case "capsuleRadius":
            {
                value = instance.m_capsuleRadius;
                return true;
            }
            case "m_eyeHeight":
            case "eyeHeight":
            {
                value = instance.m_eyeHeight;
                return true;
            }
            case "m_verticalSensitivity":
            case "verticalSensitivity":
            {
                value = instance.m_verticalSensitivity;
                return true;
            }
            case "m_horizontalSensitivity":
            case "horizontalSensitivity":
            {
                value = instance.m_horizontalSensitivity;
                return true;
            }
            case "m_sensivityPadX":
            case "sensivityPadX":
            {
                value = instance.m_sensivityPadX;
                return true;
            }
            case "m_sensivityPadY":
            case "sensivityPadY":
            {
                value = instance.m_sensivityPadY;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_maxUpDownAngle":
            case "maxUpDownAngle":
            {
                value = instance.m_maxUpDownAngle;
                return true;
            }
            case "m_numFramesPerShot":
            case "numFramesPerShot":
            {
                value = instance.m_numFramesPerShot;
                return true;
            }
            case "m_forwardBackwardSpeedModifier":
            case "forwardBackwardSpeedModifier":
            {
                value = instance.m_forwardBackwardSpeedModifier;
                return true;
            }
            case "m_leftRightSpeedModifier":
            case "leftRightSpeedModifier":
            {
                value = instance.m_leftRightSpeedModifier;
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
                if (value is Havok.hk2019.hknpFirstPersonCharacter.ControlFlags castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_flags = (Havok.hk2019.hknpFirstPersonCharacter.ControlFlags)intValue;
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
