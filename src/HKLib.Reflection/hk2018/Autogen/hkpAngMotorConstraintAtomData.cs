// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpAngMotorConstraintAtomData : HavokData<hkpAngMotorConstraintAtom> 
{
    public hkpAngMotorConstraintAtomData(HavokType type, hkpAngMotorConstraintAtom instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((ushort)instance.m_type is TGet ushortValue)
                {
                    value = ushortValue;
                    return true;
                }
                return false;
            }
            case "m_isEnabled":
            case "isEnabled":
            {
                if (instance.m_isEnabled is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_motorAxis":
            case "motorAxis":
            {
                if (instance.m_motorAxis is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_motor":
            case "motor":
            {
                if (instance.m_motor is null)
                {
                    return true;
                }
                if (instance.m_motor is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_targetAngle":
            case "targetAngle":
            {
                if (instance.m_targetAngle is not TGet castValue) return false;
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
            case "m_type":
            case "type":
            {
                if (value is hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (hkpConstraintAtom.AtomType)ushortValue;
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
                if (value is hkpConstraintMotor castValue)
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
