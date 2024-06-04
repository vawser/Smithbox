// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpConstraintCinfoData : HavokData<hknpConstraintCinfo> 
{
    public hknpConstraintCinfoData(HavokType type, hknpConstraintCinfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_constraintData":
            case "constraintData":
            {
                if (instance.m_constraintData is null)
                {
                    return true;
                }
                if (instance.m_constraintData is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_bodyA":
            case "bodyA":
            {
                if (instance.m_bodyA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bodyB":
            case "bodyB":
            {
                if (instance.m_bodyB is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((ushort)instance.m_flags is TGet ushortValue)
                {
                    value = ushortValue;
                    return true;
                }
                return false;
            }
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_desiredConstraintId":
            case "desiredConstraintId":
            {
                if (instance.m_desiredConstraintId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_constraintGroupId":
            case "constraintGroupId":
            {
                if (instance.m_constraintGroupId is not TGet castValue) return false;
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
            case "m_constraintData":
            case "constraintData":
            {
                if (value is null)
                {
                    instance.m_constraintData = default;
                    return true;
                }
                if (value is hkpConstraintData castValue)
                {
                    instance.m_constraintData = castValue;
                    return true;
                }
                return false;
            }
            case "m_bodyA":
            case "bodyA":
            {
                if (value is not hknpBodyId castValue) return false;
                instance.m_bodyA = castValue;
                return true;
            }
            case "m_bodyB":
            case "bodyB":
            {
                if (value is not hknpBodyId castValue) return false;
                instance.m_bodyB = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is hknpConstraint.FlagsEnum castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_flags = (hknpConstraint.FlagsEnum)ushortValue;
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
                if (value is not hknpConstraintId castValue) return false;
                instance.m_desiredConstraintId = castValue;
                return true;
            }
            case "m_constraintGroupId":
            case "constraintGroupId":
            {
                if (value is not hknpConstraintGroupId castValue) return false;
                instance.m_constraintGroupId = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
