// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkpConstraintMotorData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkpConstraintMotor>
{
    public hkpConstraintMotorData(Havok.hk2016.hkpConstraintMotor instance) : base(instance) { }

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
                if (value is not Havok.hk2016.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2016.hkpConstraintMotor.MotorType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_type = (Havok.hk2016.hkpConstraintMotor.MotorType)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
