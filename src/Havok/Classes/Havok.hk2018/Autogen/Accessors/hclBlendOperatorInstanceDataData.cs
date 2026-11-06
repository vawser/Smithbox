// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclBlendOperatorInstanceDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclBlendOperatorInstanceData>
{
    public hclBlendOperatorInstanceDataData(Havok.hk2018.hclBlendOperatorInstanceData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_blendWeightType":
            case "blendWeightType":
            {
                value = instance.m_blendWeightType;
                return true;
            }
            case "m_time":
            case "time":
            {
                value = instance.m_time;
                return true;
            }
            case "m_blendWeight":
            case "blendWeight":
            {
                value = instance.m_blendWeight;
                return true;
            }
            case "m_customBlendWeight":
            case "customBlendWeight":
            {
                value = instance.m_customBlendWeight;
                return true;
            }
            case "m_blendPeriod":
            case "blendPeriod":
            {
                value = instance.m_blendPeriod;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_blendWeightType":
            case "blendWeightType":
            {
                if (value is Havok.hk2018.hclBlendSomeVerticesOperator.BlendWeightType castValue)
                {
                    instance.m_blendWeightType = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_blendWeightType = (Havok.hk2018.hclBlendSomeVerticesOperator.BlendWeightType)intValue;
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
