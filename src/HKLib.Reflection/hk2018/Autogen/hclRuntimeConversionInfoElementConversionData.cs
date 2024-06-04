// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclRuntimeConversionInfoElementConversionData : HavokData<hclRuntimeConversionInfo.ElementConversion> 
{
    public hclRuntimeConversionInfoElementConversionData(HavokType type, hclRuntimeConversionInfo.ElementConversion instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_index":
            case "index":
            {
                if (instance.m_index is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_offset":
            case "offset":
            {
                if (instance.m_offset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_conversion":
            case "conversion":
            {
                if (instance.m_conversion is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_conversion is TGet byteValue)
                {
                    value = byteValue;
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
            case "m_index":
            case "index":
            {
                if (value is not byte castValue) return false;
                instance.m_index = castValue;
                return true;
            }
            case "m_offset":
            case "offset":
            {
                if (value is not byte castValue) return false;
                instance.m_offset = castValue;
                return true;
            }
            case "m_conversion":
            case "conversion":
            {
                if (value is hclRuntimeConversionInfo.VectorConversion castValue)
                {
                    instance.m_conversion = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_conversion = (hclRuntimeConversionInfo.VectorConversion)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
