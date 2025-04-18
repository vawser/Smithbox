// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiMovementPropertiesData : HavokData<hkaiMovementProperties> 
{
    public hkaiMovementPropertiesData(HavokType type, hkaiMovementProperties instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_minVelocity":
            case "minVelocity":
            {
                if (instance.m_minVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxVelocity":
            case "maxVelocity":
            {
                if (instance.m_maxVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxAcceleration":
            case "maxAcceleration":
            {
                if (instance.m_maxAcceleration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxDeceleration":
            case "maxDeceleration":
            {
                if (instance.m_maxDeceleration is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_leftTurnRadius":
            case "leftTurnRadius":
            {
                if (instance.m_leftTurnRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rightTurnRadius":
            case "rightTurnRadius":
            {
                if (instance.m_rightTurnRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxAngularVelocity":
            case "maxAngularVelocity":
            {
                if (instance.m_maxAngularVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxTurnVelocity":
            case "maxTurnVelocity":
            {
                if (instance.m_maxTurnVelocity is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_kinematicConstraintType":
            case "kinematicConstraintType":
            {
                if (instance.m_kinematicConstraintType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_kinematicConstraintType is TGet byteValue)
                {
                    value = byteValue;
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
            case "m_minVelocity":
            case "minVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_minVelocity = castValue;
                return true;
            }
            case "m_maxVelocity":
            case "maxVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_maxVelocity = castValue;
                return true;
            }
            case "m_maxAcceleration":
            case "maxAcceleration":
            {
                if (value is not float castValue) return false;
                instance.m_maxAcceleration = castValue;
                return true;
            }
            case "m_maxDeceleration":
            case "maxDeceleration":
            {
                if (value is not float castValue) return false;
                instance.m_maxDeceleration = castValue;
                return true;
            }
            case "m_leftTurnRadius":
            case "leftTurnRadius":
            {
                if (value is not float castValue) return false;
                instance.m_leftTurnRadius = castValue;
                return true;
            }
            case "m_rightTurnRadius":
            case "rightTurnRadius":
            {
                if (value is not float castValue) return false;
                instance.m_rightTurnRadius = castValue;
                return true;
            }
            case "m_maxAngularVelocity":
            case "maxAngularVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_maxAngularVelocity = castValue;
                return true;
            }
            case "m_maxTurnVelocity":
            case "maxTurnVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_maxTurnVelocity = castValue;
                return true;
            }
            case "m_kinematicConstraintType":
            case "kinematicConstraintType":
            {
                if (value is hkaiMovementProperties.KinematicConstraintType castValue)
                {
                    instance.m_kinematicConstraintType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_kinematicConstraintType = (hkaiMovementProperties.KinematicConstraintType)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
