// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkpCallbackConstraintMotorData : HavokData<HKLib.hk2019.hkpCallbackConstraintMotor> 
{
    public hkpCallbackConstraintMotorData(HavokType type, HKLib.hk2019.hkpCallbackConstraintMotor instance) : base(type, instance) {}

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
                if ((sbyte)instance.m_type is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_minForce":
            case "minForce":
            {
                if (instance.m_minForce is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxForce":
            case "maxForce":
            {
                if (instance.m_maxForce is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_callbackType":
            case "callbackType":
            {
                if (instance.m_callbackType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((uint)instance.m_callbackType is TGet uintValue)
                {
                    value = uintValue;
                    return true;
                }
                return false;
            }
            case "m_userData0":
            case "userData0":
            {
                if (instance.m_userData0 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userData1":
            case "userData1":
            {
                if (instance.m_userData1 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userData2":
            case "userData2":
            {
                if (instance.m_userData2 is not TGet castValue) return false;
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
                if (value is HKLib.hk2019.hkpConstraintMotor.MotorType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_type = (HKLib.hk2019.hkpConstraintMotor.MotorType)sbyteValue;
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
            case "m_callbackType":
            case "callbackType":
            {
                if (value is HKLib.hk2019.hkpCallbackConstraintMotor.CallbackType castValue)
                {
                    instance.m_callbackType = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_callbackType = (HKLib.hk2019.hkpCallbackConstraintMotor.CallbackType)uintValue;
                    return true;
                }
                return false;
            }
            case "m_userData0":
            case "userData0":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData0 = castValue;
                return true;
            }
            case "m_userData1":
            case "userData1":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData1 = castValue;
                return true;
            }
            case "m_userData2":
            case "userData2":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData2 = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
