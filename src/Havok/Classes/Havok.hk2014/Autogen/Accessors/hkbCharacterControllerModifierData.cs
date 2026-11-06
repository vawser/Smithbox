// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbCharacterControllerModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbCharacterControllerModifier>
{
    public hkbCharacterControllerModifierData(Havok.hk2014.hkbCharacterControllerModifier instance) : base(instance) { }

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
            case "m_controlData":
            case "controlData":
            {
                value = instance.m_controlData;
                return true;
            }
            case "m_initialVelocity":
            case "initialVelocity":
            {
                value = instance.m_initialVelocity;
                return true;
            }
            case "m_initialVelocityCoordinates":
            case "initialVelocityCoordinates":
            {
                value = instance.m_initialVelocityCoordinates;
                return true;
            }
            case "m_motionMode":
            case "motionMode":
            {
                value = instance.m_motionMode;
                return true;
            }
            case "m_forceDownwardMomentum":
            case "forceDownwardMomentum":
            {
                value = instance.m_forceDownwardMomentum;
                return true;
            }
            case "m_applyGravity":
            case "applyGravity":
            {
                value = instance.m_applyGravity;
                return true;
            }
            case "m_setInitialVelocity":
            case "setInitialVelocity":
            {
                value = instance.m_setInitialVelocity;
                return true;
            }
            case "m_isTouchingGround":
            case "isTouchingGround":
            {
                value = instance.m_isTouchingGround;
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
            case "m_controlData":
            case "controlData":
            {
                if (value is not Havok.hk2014.hkbCharacterControllerModifierControlData castValue) return false;
                instance.m_controlData = castValue;
                return true;
            }
            case "m_initialVelocity":
            case "initialVelocity":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_initialVelocity = castValue;
                return true;
            }
            case "m_initialVelocityCoordinates":
            case "initialVelocityCoordinates":
            {
                if (value is Havok.hk2014.hkbCharacterControllerModifier.InitialVelocityCoordinates castValue)
                {
                    instance.m_initialVelocityCoordinates = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_initialVelocityCoordinates = (Havok.hk2014.hkbCharacterControllerModifier.InitialVelocityCoordinates)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_motionMode":
            case "motionMode":
            {
                if (value is Havok.hk2014.hkbCharacterControllerModifier.MotionMode castValue)
                {
                    instance.m_motionMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_motionMode = (Havok.hk2014.hkbCharacterControllerModifier.MotionMode)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_forceDownwardMomentum":
            case "forceDownwardMomentum":
            {
                if (value is not bool castValue) return false;
                instance.m_forceDownwardMomentum = castValue;
                return true;
            }
            case "m_applyGravity":
            case "applyGravity":
            {
                if (value is not bool castValue) return false;
                instance.m_applyGravity = castValue;
                return true;
            }
            case "m_setInitialVelocity":
            case "setInitialVelocity":
            {
                if (value is not bool castValue) return false;
                instance.m_setInitialVelocity = castValue;
                return true;
            }
            case "m_isTouchingGround":
            case "isTouchingGround":
            {
                if (value is not bool castValue) return false;
                instance.m_isTouchingGround = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
