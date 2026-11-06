// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpConstraintCinfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpConstraintCinfo>
{
    public hknpConstraintCinfoData(Havok.hk2014.hknpConstraintCinfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_constraintData":
            case "constraintData":
            {
                value = instance.m_constraintData;
                return true;
            }
            case "m_bodyA":
            case "bodyA":
            {
                value = instance.m_bodyA;
                return true;
            }
            case "m_bodyB":
            case "bodyB":
            {
                value = instance.m_bodyB;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
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
            case "m_constraintData":
            case "constraintData":
            {
                if (value is null)
                {
                    instance.m_constraintData = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpConstraintData castValue)
                {
                    instance.m_constraintData = castValue;
                    return true;
                }
                return false;
            }
            case "m_bodyA":
            case "bodyA":
            {
                if (value is not uint castValue) return false;
                instance.m_bodyA = castValue;
                return true;
            }
            case "m_bodyB":
            case "bodyB":
            {
                if (value is not uint castValue) return false;
                instance.m_bodyB = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is Havok.hk2014.hknpConstraint.FlagsEnum castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_flags = (Havok.hk2014.hknpConstraint.FlagsEnum)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
