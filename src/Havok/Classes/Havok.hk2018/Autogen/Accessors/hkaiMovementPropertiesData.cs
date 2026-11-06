// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiMovementPropertiesData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiMovementProperties>
{
    public hkaiMovementPropertiesData(Havok.hk2018.hkaiMovementProperties instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_minVelocity":
            case "minVelocity":
            {
                value = instance.m_minVelocity;
                return true;
            }
            case "m_maxVelocity":
            case "maxVelocity":
            {
                value = instance.m_maxVelocity;
                return true;
            }
            case "m_maxAcceleration":
            case "maxAcceleration":
            {
                value = instance.m_maxAcceleration;
                return true;
            }
            case "m_maxDeceleration":
            case "maxDeceleration":
            {
                value = instance.m_maxDeceleration;
                return true;
            }
            case "m_leftTurnRadius":
            case "leftTurnRadius":
            {
                value = instance.m_leftTurnRadius;
                return true;
            }
            case "m_rightTurnRadius":
            case "rightTurnRadius":
            {
                value = instance.m_rightTurnRadius;
                return true;
            }
            case "m_maxAngularVelocity":
            case "maxAngularVelocity":
            {
                value = instance.m_maxAngularVelocity;
                return true;
            }
            case "m_maxTurnVelocity":
            case "maxTurnVelocity":
            {
                value = instance.m_maxTurnVelocity;
                return true;
            }
            case "m_kinematicConstraintType":
            case "kinematicConstraintType":
            {
                value = instance.m_kinematicConstraintType;
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
                if (value is Havok.hk2018.hkaiMovementProperties.KinematicConstraintType castValue)
                {
                    instance.m_kinematicConstraintType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_kinematicConstraintType = (Havok.hk2018.hkaiMovementProperties.KinematicConstraintType)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
