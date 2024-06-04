// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbStringConditionData : HavokData<hkbStringCondition> 
{
    public hkbStringConditionData(HavokType type, hkbStringCondition instance) : base(type, instance) {}

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
            case "m_conditionString":
            case "conditionString":
            {
                if (instance.m_conditionString is null)
                {
                    return true;
                }
                if (instance.m_conditionString is TGet castValue)
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
            case "m_conditionString":
            case "conditionString":
            {
                if (value is null)
                {
                    instance.m_conditionString = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_conditionString = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
