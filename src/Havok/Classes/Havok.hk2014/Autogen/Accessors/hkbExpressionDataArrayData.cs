// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbExpressionDataArrayData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbExpressionDataArray>
{
    public hkbExpressionDataArrayData(Havok.hk2014.hkbExpressionDataArray instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_expressionsData":
            case "expressionsData":
            {
                if (value is not List<Havok.hk2014.hkbExpressionData> castValue) return false;
                instance.m_expressionsData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
