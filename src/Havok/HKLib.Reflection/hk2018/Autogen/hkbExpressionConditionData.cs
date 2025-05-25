// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbExpressionConditionData : HavokData<hkbExpressionCondition> 
{
    public hkbExpressionConditionData(HavokType type, hkbExpressionCondition instance) : base(type, instance) {}

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
            case "m_expression":
            case "expression":
            {
                if (instance.m_expression is null)
                {
                    return true;
                }
                if (instance.m_expression is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
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
            case "m_expression":
            case "expression":
            {
                if (value is null)
                {
                    instance.m_expression = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_expression = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
