// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxBlendShapeCollectionChannelFloatCurveData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxBlendShapeCollectionChannel.FloatCurve>
{
    public hkxBlendShapeCollectionChannelFloatCurveData(Havok.hk2019.hkxBlendShapeCollectionChannel.FloatCurve instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_values":
            case "values":
            {
                value = instance.m_values;
                return true;
            }
            case "m_timeStart":
            case "timeStart":
            {
                value = instance.m_timeStart;
                return true;
            }
            case "m_timeEnd":
            case "timeEnd":
            {
                value = instance.m_timeEnd;
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
            case "m_values":
            case "values":
            {
                if (value is not List<Havok.hk2019.hkxBlendShapeCollectionChannel.FloatCurveKey> castValue) return false;
                instance.m_values = castValue;
                return true;
            }
            case "m_timeStart":
            case "timeStart":
            {
                if (value is not long castValue) return false;
                instance.m_timeStart = castValue;
                return true;
            }
            case "m_timeEnd":
            case "timeEnd":
            {
                if (value is not long castValue) return false;
                instance.m_timeEnd = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
