// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkpLimitedForceConstraintMotorData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkpLimitedForceConstraintMotor>
{
    public hkpLimitedForceConstraintMotorData(Havok.hk2019.hkpLimitedForceConstraintMotor instance) : base(instance) { }

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
                if (value is Havok.hk2019.hkpConstraintMotor.MotorType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_type = (Havok.hk2019.hkpConstraintMotor.MotorType)sbyteValue;
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
            default:
            return false;
        }
    }

}
