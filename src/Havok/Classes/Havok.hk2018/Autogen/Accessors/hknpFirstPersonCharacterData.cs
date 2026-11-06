// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpFirstPersonCharacterData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpFirstPersonCharacter>
{
    public hknpFirstPersonCharacterData(Havok.hk2018.hknpFirstPersonCharacter instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
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
            case "m_eyeHeight":
            case "eyeHeight":
            {
                value = instance.m_eyeHeight;
                return true;
            }
            case "m_gravityStrength":
            case "gravityStrength":
            {
                value = instance.m_gravityStrength;
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
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_gunCounter":
            case "gunCounter":
            {
                value = instance.m_gunCounter;
                return true;
            }
            case "m_gunCounterRmb":
            case "gunCounterRmb":
            {
                value = instance.m_gunCounterRmb;
                return true;
            }
            case "m_currentAngle":
            case "currentAngle":
            {
                value = instance.m_currentAngle;
                return true;
            }
            case "m_currentElevation":
            case "currentElevation":
            {
                value = instance.m_currentElevation;
                return true;
            }
            case "m_specialGravity":
            case "specialGravity":
            {
                value = instance.m_specialGravity;
                return true;
            }
            case "m_gravity":
            case "gravity":
            {
                value = instance.m_gravity;
                return true;
            }
            case "m_currentGun":
            case "currentGun":
            {
                value = instance.m_currentGun;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
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
                if (value is Havok.hk2018.hknpFirstPersonGun castValue)
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
