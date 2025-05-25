// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbCharacterControllerModifierData : HavokData<hkbCharacterControllerModifier> 
{
    public hkbCharacterControllerModifierData(HavokType type, hkbCharacterControllerModifier instance) : base(type, instance) {}

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
            case "m_controlData":
            case "controlData":
            {
                if (instance.m_controlData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_initialVelocity":
            case "initialVelocity":
            {
                if (instance.m_initialVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_initialVelocityCoordinates":
            case "initialVelocityCoordinates":
            {
                if (instance.m_initialVelocityCoordinates is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_initialVelocityCoordinates is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_motionMode":
            case "motionMode":
            {
                if (instance.m_motionMode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_motionMode is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_gravityFactor":
            case "gravityFactor":
            {
                if (instance.m_gravityFactor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_setInitialVelocity":
            case "setInitialVelocity":
            {
                if (instance.m_setInitialVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isTouchingGround":
            case "isTouchingGround":
            {
                if (instance.m_isTouchingGround is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_collisionShapeProfileIdx":
            case "collisionShapeProfileIdx":
            {
                if (instance.m_collisionShapeProfileIdx is not TGet castValue) return false;
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
            case "m_controlData":
            case "controlData":
            {
                if (value is not hkbCharacterControllerModifierControlData castValue) return false;
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
                if (value is hkbCharacterControllerModifier.InitialVelocityCoordinates castValue)
                {
                    instance.m_initialVelocityCoordinates = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_initialVelocityCoordinates = (hkbCharacterControllerModifier.InitialVelocityCoordinates)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_motionMode":
            case "motionMode":
            {
                if (value is hkbCharacterControllerModifier.MotionMode castValue)
                {
                    instance.m_motionMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_motionMode = (hkbCharacterControllerModifier.MotionMode)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_gravityFactor":
            case "gravityFactor":
            {
                if (value is not float castValue) return false;
                instance.m_gravityFactor = castValue;
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
            case "m_collisionShapeProfileIdx":
            case "collisionShapeProfileIdx":
            {
                if (value is not int castValue) return false;
                instance.m_collisionShapeProfileIdx = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
