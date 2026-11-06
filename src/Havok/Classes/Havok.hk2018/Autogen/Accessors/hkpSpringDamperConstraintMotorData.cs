// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkpSpringDamperConstraintMotorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkpSpringDamperConstraintMotor>
{
    public hkpSpringDamperConstraintMotorData(Havok.hk2018.hkpSpringDamperConstraintMotor instance) : base(instance) { }

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
            case "m_springConstant":
            case "springConstant":
            {
                value = instance.m_springConstant;
                return true;
            }
            case "m_springDamping":
            case "springDamping":
            {
                value = instance.m_springDamping;
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
            case "m_springConstant":
            case "springConstant":
            {
                if (value is not float castValue) return false;
                instance.m_springConstant = castValue;
                return true;
            }
            case "m_springDamping":
            case "springDamping":
            {
                if (value is not float castValue) return false;
                instance.m_springDamping = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
