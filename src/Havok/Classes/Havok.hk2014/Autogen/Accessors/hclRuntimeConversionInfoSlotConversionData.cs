// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclRuntimeConversionInfoSlotConversionData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclRuntimeConversionInfo.SlotConversion>
{
    private static readonly System.Reflection.FieldInfo _elementsInfo = typeof(Havok.hk2014.hclRuntimeConversionInfo.SlotConversion).GetField("m_elements")!;
    public hclRuntimeConversionInfoSlotConversionData(Havok.hk2014.hclRuntimeConversionInfo.SlotConversion instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_elements":
            case "elements":
            {
                value = instance.m_elements;
                return true;
            }
            case "m_numElements":
            case "numElements":
            {
                value = instance.m_numElements;
                return true;
            }
            case "m_index":
            case "index":
            {
                value = instance.m_index;
                return true;
            }
            case "m_partialWrite":
            case "partialWrite":
            {
                value = instance.m_partialWrite;
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
            case "m_elements":
            case "elements":
            {
                if (value is not byte[] castValue || castValue.Length != 4) return false;
                try
                {
                    _elementsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_numElements":
            case "numElements":
            {
                if (value is not byte castValue) return false;
                instance.m_numElements = castValue;
                return true;
            }
            case "m_index":
            case "index":
            {
                if (value is not byte castValue) return false;
                instance.m_index = castValue;
                return true;
            }
            case "m_partialWrite":
            case "partialWrite":
            {
                if (value is not bool castValue) return false;
                instance.m_partialWrite = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
