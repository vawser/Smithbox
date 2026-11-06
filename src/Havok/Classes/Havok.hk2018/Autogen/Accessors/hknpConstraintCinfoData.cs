// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hknpConstraintCinfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hknpConstraintCinfo>
{
    public hknpConstraintCinfoData(Havok.hk2018.hknpConstraintCinfo instance) : base(instance) { }

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
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_desiredConstraintId":
            case "desiredConstraintId":
            {
                value = instance.m_desiredConstraintId;
                return true;
            }
            case "m_constraintGroupId":
            case "constraintGroupId":
            {
                value = instance.m_constraintGroupId;
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
                if (value is Havok.hk2018.hkpConstraintData castValue)
                {
                    instance.m_constraintData = castValue;
                    return true;
                }
                return false;
            }
            case "m_bodyA":
            case "bodyA":
            {
                if (value is not Havok.hk2018.hknpBodyId castValue) return false;
                instance.m_bodyA = castValue;
                return true;
            }
            case "m_bodyB":
            case "bodyB":
            {
                if (value is not Havok.hk2018.hknpBodyId castValue) return false;
                instance.m_bodyB = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is Havok.hk2018.hknpConstraint.FlagsEnum castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_flags = (Havok.hk2018.hknpConstraint.FlagsEnum)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_desiredConstraintId":
            case "desiredConstraintId":
            {
                if (value is not Havok.hk2018.hknpConstraintId castValue) return false;
                instance.m_desiredConstraintId = castValue;
                return true;
            }
            case "m_constraintGroupId":
            case "constraintGroupId":
            {
                if (value is not Havok.hk2018.hknpConstraintGroupId castValue) return false;
                instance.m_constraintGroupId = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
