// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hknpConstraintGroupData : HavokData<HKLib.hk2019.hknpConstraintGroup> 
{
    public hknpConstraintGroupData(HavokType type, HKLib.hk2019.hknpConstraintGroup instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_microStepMultiplier":
            case "microStepMultiplier":
            {
                if (instance.m_microStepMultiplier is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_id":
            case "id":
            {
                if (instance.m_id is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_firstConstraintId":
            case "firstConstraintId":
            {
                if (instance.m_firstConstraintId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numConstraintIds":
            case "numConstraintIds":
            {
                if (instance.m_numConstraintIds is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
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
            case "m_microStepMultiplier":
            case "microStepMultiplier":
            {
                if (value is not byte castValue) return false;
                instance.m_microStepMultiplier = castValue;
                return true;
            }
            case "m_id":
            case "id":
            {
                if (value is not HKLib.hk2019.hknpConstraintGroupId castValue) return false;
                instance.m_id = castValue;
                return true;
            }
            case "m_firstConstraintId":
            case "firstConstraintId":
            {
                if (value is not HKLib.hk2019.hknpConstraintId castValue) return false;
                instance.m_firstConstraintId = castValue;
                return true;
            }
            case "m_numConstraintIds":
            case "numConstraintIds":
            {
                if (value is not uint castValue) return false;
                instance.m_numConstraintIds = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
