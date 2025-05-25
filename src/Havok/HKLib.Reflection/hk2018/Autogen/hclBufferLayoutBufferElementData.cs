// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclBufferLayoutBufferElementData : HavokData<hclBufferLayout.BufferElement> 
{
    public hclBufferLayoutBufferElementData(HavokType type, hclBufferLayout.BufferElement instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vectorConversion":
            case "vectorConversion":
            {
                if (instance.m_vectorConversion is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_vectorConversion is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_vectorSize":
            case "vectorSize":
            {
                if (instance.m_vectorSize is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_slotId":
            case "slotId":
            {
                if (instance.m_slotId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_slotStart":
            case "slotStart":
            {
                if (instance.m_slotStart is not TGet castValue) return false;
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
            case "m_vectorConversion":
            case "vectorConversion":
            {
                if (value is hclRuntimeConversionInfo.VectorConversion castValue)
                {
                    instance.m_vectorConversion = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_vectorConversion = (hclRuntimeConversionInfo.VectorConversion)byteValue;
                    return true;
                }
                return false;
            }
            case "m_vectorSize":
            case "vectorSize":
            {
                if (value is not byte castValue) return false;
                instance.m_vectorSize = castValue;
                return true;
            }
            case "m_slotId":
            case "slotId":
            {
                if (value is not byte castValue) return false;
                instance.m_slotId = castValue;
                return true;
            }
            case "m_slotStart":
            case "slotStart":
            {
                if (value is not byte castValue) return false;
                instance.m_slotStart = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
