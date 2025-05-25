// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbExpressionDataArrayData : HavokData<hkbExpressionDataArray> 
{
    public hkbExpressionDataArrayData(HavokType type, hkbExpressionDataArray instance) : base(type, instance) {}

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
            case "m_expressionsData":
            case "expressionsData":
            {
                if (instance.m_expressionsData is not TGet castValue) return false;
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
            case "m_expressionsData":
            case "expressionsData":
            {
                if (value is not List<hkbExpressionData> castValue) return false;
                instance.m_expressionsData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
