// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkpVelocityConstraintMotorData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkpVelocityConstraintMotor>
{
    public hkpVelocityConstraintMotorData(Havok.hk2019.hkpVelocityConstraintMotor instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_minForce":
            case "minForce":
            {
                value = instance.m_minForce;
                return true;
            }
            case "m_maxForce":
            case "maxForce":
            {
                value = instance.m_maxForce;
                return true;
            }
            case "m_tau":
            case "tau":
            {
                value = instance.m_tau;
                return true;
            }
            case "m_damping":
            case "damping":
            {
                value = instance.m_damping;
                return true;
            }
            case "m_velocityTarget":
            case "velocityTarget":
            {
                value = instance.m_velocityTarget;
                return true;
            }
            case "m_useVelocityTargetFromConstraintTargets":
            case "useVelocityTargetFromConstraintTargets":
            {
                value = instance.m_useVelocityTargetFromConstraintTargets;
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
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2019.hkpConstraintMotor.MotorType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_type = (Havok.hk2019.hkpConstraintMotor.MotorType)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_minForce":
            case "minForce":
            {
                if (value is not float castValue) return false;
                instance.m_minForce = castValue;
                return true;
            }
            case "m_maxForce":
            case "maxForce":
            {
                if (value is not float castValue) return false;
                instance.m_maxForce = castValue;
                return true;
            }
            case "m_tau":
            case "tau":
            {
                if (value is not float castValue) return false;
                instance.m_tau = castValue;
                return true;
            }
            case "m_damping":
            case "damping":
            {
                if (value is not float castValue) return false;
                instance.m_damping = castValue;
                return true;
            }
            case "m_velocityTarget":
            case "velocityTarget":
            {
                if (value is not float castValue) return false;
                instance.m_velocityTarget = castValue;
                return true;
            }
            case "m_useVelocityTargetFromConstraintTargets":
            case "useVelocityTargetFromConstraintTargets":
            {
                if (value is not bool castValue) return false;
                instance.m_useVelocityTargetFromConstraintTargets = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
