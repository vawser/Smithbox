// Automatically Generated

#nullable enable

namespace Havok.hk2010.Accessors;

internal sealed class hkpPositionConstraintMotorData : Havok.Formats.HavokFieldAccess<Havok.hk2010.hkpPositionConstraintMotor>
{
    public hkpPositionConstraintMotorData(Havok.hk2010.hkpPositionConstraintMotor instance) : base(instance) { }

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
            case "m_proportionalRecoveryVelocity":
            case "proportionalRecoveryVelocity":
            {
                value = instance.m_proportionalRecoveryVelocity;
                return true;
            }
            case "m_constantRecoveryVelocity":
            case "constantRecoveryVelocity":
            {
                value = instance.m_constantRecoveryVelocity;
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
                if (value is Havok.hk2010.hkpConstraintMotor.MotorType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_type = (Havok.hk2010.hkpConstraintMotor.MotorType)sbyteValue;
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
            case "m_proportionalRecoveryVelocity":
            case "proportionalRecoveryVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_proportionalRecoveryVelocity = castValue;
                return true;
            }
            case "m_constantRecoveryVelocity":
            case "constantRecoveryVelocity":
            {
                if (value is not float castValue) return false;
                instance.m_constantRecoveryVelocity = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
