// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclRuntimeConversionInfoElementConversionData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclRuntimeConversionInfo.ElementConversion>
{
    public hclRuntimeConversionInfoElementConversionData(Havok.hk2018.hclRuntimeConversionInfo.ElementConversion instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_index":
            case "index":
            {
                value = instance.m_index;
                return true;
            }
            case "m_offset":
            case "offset":
            {
                value = instance.m_offset;
                return true;
            }
            case "m_conversion":
            case "conversion":
            {
                value = instance.m_conversion;
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
                if (value is Havok.hk2018.hclRuntimeConversionInfo.VectorConversion castValue)
                {
                    instance.m_conversion = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_conversion = (Havok.hk2018.hclRuntimeConversionInfo.VectorConversion)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
