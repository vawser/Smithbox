// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbScriptConditionData : HavokData<hkbScriptCondition> 
{
    public hkbScriptConditionData(HavokType type, hkbScriptCondition instance) : base(type, instance) {}

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
            case "m_conditionScript":
            case "conditionScript":
            {
                if (instance.m_conditionScript is null)
                {
                    return true;
                }
                if (instance.m_conditionScript is TGet castValue)
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
            case "m_conditionScript":
            case "conditionScript":
            {
                if (value is null)
                {
                    instance.m_conditionScript = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_conditionScript = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
