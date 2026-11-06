// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkpLinMotorConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkpLinMotorConstraintAtom>
{
    public hkpLinMotorConstraintAtomData(Havok.hk2018.hkpLinMotorConstraintAtom instance) : base(instance) { }

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
            case "m_targetPosition":
            case "targetPosition":
            {
                value = instance.m_targetPosition;
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
                if (value is Havok.hk2018.hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (Havok.hk2018.hkpConstraintAtom.AtomType)ushortValue;
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
                if (value is Havok.hk2018.hkpConstraintMotor castValue)
                {
                    instance.m_motor = castValue;
                    return true;
                }
                return false;
            }
            case "m_targetPosition":
            case "targetPosition":
            {
                if (value is not float castValue) return false;
                instance.m_targetPosition = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
