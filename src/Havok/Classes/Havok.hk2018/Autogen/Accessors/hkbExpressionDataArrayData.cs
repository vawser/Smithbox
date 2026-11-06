// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbExpressionDataArrayData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbExpressionDataArray>
{
    public hkbExpressionDataArrayData(Havok.hk2018.hkbExpressionDataArray instance) : base(instance) { }

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
            case "m_expressionsData":
            case "expressionsData":
            {
                value = instance.m_expressionsData;
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
            case "m_expressionsData":
            case "expressionsData":
            {
                if (value is not List<Havok.hk2018.hkbExpressionData> castValue) return false;
                instance.m_expressionsData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
