// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkpCallbackConstraintMotorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkpCallbackConstraintMotor>
{
    public hkpCallbackConstraintMotorData(Havok.hk2018.hkpCallbackConstraintMotor instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
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
            case "m_callbackType":
            case "callbackType":
            {
                value = instance.m_callbackType;
                return true;
            }
            case "m_userData0":
            case "userData0":
            {
                value = instance.m_userData0;
                return true;
            }
            case "m_userData1":
            case "userData1":
            {
                value = instance.m_userData1;
                return true;
            }
            case "m_userData2":
            case "userData2":
            {
                value = instance.m_userData2;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2018.hkpConstraintMotor.MotorType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_type = (Havok.hk2018.hkpConstraintMotor.MotorType)sbyteValue;
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
                if (value is Havok.hk2018.hkpCallbackConstraintMotor.CallbackType castValue)
                {
                    instance.m_callbackType = castValue;
                    return true;
                }
                if (value is uint uintValue)
                {
                    instance.m_callbackType = (Havok.hk2018.hkpCallbackConstraintMotor.CallbackType)uintValue;
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
