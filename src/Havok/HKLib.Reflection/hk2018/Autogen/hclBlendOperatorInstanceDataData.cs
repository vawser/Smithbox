// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclBlendOperatorInstanceDataData : HavokData<hclBlendOperatorInstanceData> 
{
    public hclBlendOperatorInstanceDataData(HavokType type, hclBlendOperatorInstanceData instance) : base(type, instance) {}

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
            case "m_blendWeightType":
            case "blendWeightType":
            {
                if (instance.m_blendWeightType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_blendWeightType is TGet intValue)
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
            case "m_blendWeight":
            case "blendWeight":
            {
                if (instance.m_blendWeight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_customBlendWeight":
            case "customBlendWeight":
            {
                if (instance.m_customBlendWeight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_blendPeriod":
            case "blendPeriod":
            {
                if (instance.m_blendPeriod is not TGet castValue) return false;
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
            case "m_blendWeightType":
            case "blendWeightType":
            {
                if (value is hclBlendSomeVerticesOperator.BlendWeightType castValue)
                {
                    instance.m_blendWeightType = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_blendWeightType = (hclBlendSomeVerticesOperator.BlendWeightType)intValue;
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
            case "m_blendWeight":
            case "blendWeight":
            {
                if (value is not float castValue) return false;
                instance.m_blendWeight = castValue;
                return true;
            }
            case "m_customBlendWeight":
            case "customBlendWeight":
            {
                if (value is not float castValue) return false;
                instance.m_customBlendWeight = castValue;
                return true;
            }
            case "m_blendPeriod":
            case "blendPeriod":
            {
                if (value is not float castValue) return false;
                instance.m_blendPeriod = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
