// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclTransitionConstraintInstanceDataData : HavokData<hclTransitionConstraintInstanceData> 
{
    public hclTransitionConstraintInstanceDataData(HavokType type, hclTransitionConstraintInstanceData instance) : base(type, instance) {}

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
            case "m_state":
            case "state":
            {
                if (instance.m_state is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_state is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_time":
            case "time":
            {
                if (instance.m_time is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_state":
            case "state":
            {
                if (value is hclTransitionConstraintInstanceData.State castValue)
                {
                    instance.m_state = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_state = (hclTransitionConstraintInstanceData.State)intValue;
                    return true;
                }
                return false;
            }
            case "m_time":
            case "time":
            {
                if (value is not float castValue) return false;
                instance.m_time = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
