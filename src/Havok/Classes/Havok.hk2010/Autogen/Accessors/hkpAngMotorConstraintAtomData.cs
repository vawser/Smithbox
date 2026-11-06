// Automatically Generated

#nullable enable

namespace Havok.hk2010.Accessors;

internal sealed class hkpAngMotorConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2010.hkpAngMotorConstraintAtom>
{
    public hkpAngMotorConstraintAtomData(Havok.hk2010.hkpAngMotorConstraintAtom instance) : base(instance) { }

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
            case "m_isEnabled":
            case "isEnabled":
            {
                value = instance.m_isEnabled;
                return true;
            }
            case "m_motorAxis":
            case "motorAxis":
            {
                value = instance.m_motorAxis;
                return true;
            }
            case "m_initializedOffset":
            case "initializedOffset":
            {
                value = instance.m_initializedOffset;
                return true;
            }
            case "m_previousTargetAngleOffset":
            case "previousTargetAngleOffset":
            {
                value = instance.m_previousTargetAngleOffset;
                return true;
            }
            case "m_correspondingAngLimitSolverResultOffset":
            case "correspondingAngLimitSolverResultOffset":
            {
                value = instance.m_correspondingAngLimitSolverResultOffset;
                return true;
            }
            case "m_targetAngle":
            case "targetAngle":
            {
                value = instance.m_targetAngle;
                return true;
            }
            case "m_motor":
            case "motor":
            {
                value = instance.m_motor;
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
                if (value is Havok.hk2010.hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (Havok.hk2010.hkpConstraintAtom.AtomType)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_isEnabled":
            case "isEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_isEnabled = castValue;
                return true;
            }
            case "m_motorAxis":
            case "motorAxis":
            {
                if (value is not byte castValue) return false;
                instance.m_motorAxis = castValue;
                return true;
            }
            case "m_initializedOffset":
            case "initializedOffset":
            {
                if (value is not short castValue) return false;
                instance.m_initializedOffset = castValue;
                return true;
            }
            case "m_previousTargetAngleOffset":
            case "previousTargetAngleOffset":
            {
                if (value is not short castValue) return false;
                instance.m_previousTargetAngleOffset = castValue;
                return true;
            }
            case "m_correspondingAngLimitSolverResultOffset":
            case "correspondingAngLimitSolverResultOffset":
            {
                if (value is not short castValue) return false;
                instance.m_correspondingAngLimitSolverResultOffset = castValue;
                return true;
            }
            case "m_targetAngle":
            case "targetAngle":
            {
                if (value is not float castValue) return false;
                instance.m_targetAngle = castValue;
                return true;
            }
            case "m_motor":
            case "motor":
            {
                if (value is null)
                {
                    instance.m_motor = default;
                    return true;
                }
                if (value is Havok.hk2010.hkpConstraintMotor castValue)
                {
                    instance.m_motor = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
