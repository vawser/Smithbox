// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclBufferLayoutBufferElementData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclBufferLayout.BufferElement>
{
    public hclBufferLayoutBufferElementData(Havok.hk2018.hclBufferLayout.BufferElement instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vectorConversion":
            case "vectorConversion":
            {
                value = instance.m_vectorConversion;
                return true;
            }
            case "m_vectorSize":
            case "vectorSize":
            {
                value = instance.m_vectorSize;
                return true;
            }
            case "m_slotId":
            case "slotId":
            {
                value = instance.m_slotId;
                return true;
            }
            case "m_slotStart":
            case "slotStart":
            {
                value = instance.m_slotStart;
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
            case "m_vectorConversion":
            case "vectorConversion":
            {
                if (value is Havok.hk2018.hclRuntimeConversionInfo.VectorConversion castValue)
                {
                    instance.m_vectorConversion = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_vectorConversion = (Havok.hk2018.hclRuntimeConversionInfo.VectorConversion)byteValue;
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
