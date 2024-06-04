// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclVdbConfigConstraintSetData : HavokData<hclVdbConfigConstraintSet> 
{
    public hclVdbConfigConstraintSetData(HavokType type, hclVdbConfigConstraintSet instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_id":
            case "id":
            {
                if (instance.m_id is not TGet castValue) return false;
                value = castValue;
                return true;
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
            case "m_type":
            case "type":
            {
                if (instance.m_type is null)
                {
                    return true;
                }
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_subStepIndex":
            case "subStepIndex":
            {
                if (instance.m_subStepIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                if (instance.m_stiffness is not TGet castValue) return false;
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
            case "m_id":
            case "id":
            {
                if (value is not hkHandle<uint> castValue) return false;
                instance.m_id = castValue;
                return true;
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
            case "m_type":
            case "type":
            {
                if (value is null)
                {
                    instance.m_type = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                return false;
            }
            case "m_subStepIndex":
            case "subStepIndex":
            {
                if (value is not int castValue) return false;
                instance.m_subStepIndex = castValue;
                return true;
            }
            case "m_stiffness":
            case "stiffness":
            {
                if (value is not float castValue) return false;
                instance.m_stiffness = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
