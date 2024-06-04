// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkxBlendShapeCollectionChannelFloatCurveData : HavokData<hkxBlendShapeCollectionChannel.FloatCurve> 
{
    public hkxBlendShapeCollectionChannelFloatCurveData(HavokType type, hkxBlendShapeCollectionChannel.FloatCurve instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_values":
            case "values":
            {
                if (instance.m_values is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_timeStart":
            case "timeStart":
            {
                if (instance.m_timeStart is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_timeEnd":
            case "timeEnd":
            {
                if (instance.m_timeEnd is not TGet castValue) return false;
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
                if (value is not List<hkxBlendShapeCollectionChannel.FloatCurveKey> castValue) return false;
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
