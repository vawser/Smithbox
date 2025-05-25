// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclRuntimeConversionInfoSlotConversionData : HavokData<hclRuntimeConversionInfo.SlotConversion> 
{
    private static readonly System.Reflection.FieldInfo _elementsInfo = typeof(hclRuntimeConversionInfo.SlotConversion).GetField("m_elements")!;
    public hclRuntimeConversionInfoSlotConversionData(HavokType type, hclRuntimeConversionInfo.SlotConversion instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_elements":
            case "elements":
            {
                if (instance.m_elements is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numElements":
            case "numElements":
            {
                if (instance.m_numElements is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_index":
            case "index":
            {
                if (instance.m_index is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_partialWrite":
            case "partialWrite":
            {
                if (instance.m_partialWrite is not TGet castValue) return false;
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
