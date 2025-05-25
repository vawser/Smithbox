// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpCallbackConstraintMotorData : HavokData<hkpCallbackConstraintMotor> 
{
    public hkpCallbackConstraintMotorData(HavokType type, hkpCallbackConstraintMotor instance) : base(type, instance) {}

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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (value is hkpConstraintMotor.MotorType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_type = (hkpConstraintMotor.MotorType)sbyteValue;
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
                if (value is hkpCallbackConstraintMotor.CallbackType castValue)
                {
                    instance.m_callbackType = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_callbackType = (hkpCallbackConstraintMotor.CallbackType)uintValue;
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
