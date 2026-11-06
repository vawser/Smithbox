// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkpAngMotorConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkpAngMotorConstraintAtom>
{
    public hkpAngMotorConstraintAtomData(Havok.hk2016.hkpAngMotorConstraintAtom instance) : base(instance) { }

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
            case "m_motor":
            case "motor":
            {
                value = instance.m_motor;
                return true;
            }
            case "m_targetAngle":
            case "targetAngle":
            {
                value = instance.m_targetAngle;
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
                if (value is Havok.hk2016.hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (Havok.hk2016.hkpConstraintAtom.AtomType)ushortValue;
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
            case "m_motor":
            case "motor":
            {
                if (value is null)
                {
                    instance.m_motor = default;
                    return true;
                }
                if (value is Havok.hk2016.hkpConstraintMotor castValue)
                {
                    instance.m_motor = castValue;
                    return true;
                }
                return false;
            }
            case "m_targetAngle":
            case "targetAngle":
            {
                if (value is not float castValue) return false;
                instance.m_targetAngle = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
