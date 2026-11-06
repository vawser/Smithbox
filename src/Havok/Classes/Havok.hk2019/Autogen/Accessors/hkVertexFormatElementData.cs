// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkVertexFormatElementData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkVertexFormat.Element>
{
    private static readonly System.Reflection.FieldInfo _padInfo = typeof(Havok.hk2019.hkVertexFormat.Element).GetField("m_pad")!;
    public hkVertexFormatElementData(Havok.hk2019.hkVertexFormat.Element instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_dataType":
            case "dataType":
            {
                value = instance.m_dataType;
                return true;
            }
            case "m_numValues":
            case "numValues":
            {
                value = instance.m_numValues;
                return true;
            }
            case "m_usage":
            case "usage":
            {
                value = instance.m_usage;
                return true;
            }
            case "m_subUsage":
            case "subUsage":
            {
                value = instance.m_subUsage;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_pad":
            case "pad":
            {
                value = instance.m_pad;
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
            case "m_dataType":
            case "dataType":
            {
                if (value is Havok.hk2019.hkVertexFormat.ComponentType castValue)
                {
                    instance.m_dataType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_dataType = (Havok.hk2019.hkVertexFormat.ComponentType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_numValues":
            case "numValues":
            {
                if (value is not byte castValue) return false;
                instance.m_numValues = castValue;
                return true;
            }
            case "m_usage":
            case "usage":
            {
                if (value is Havok.hk2019.hkVertexFormat.ComponentUsage castValue)
                {
                    instance.m_usage = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_usage = (Havok.hk2019.hkVertexFormat.ComponentUsage)byteValue;
                    return true;
                }
                return false;
            }
            case "m_subUsage":
            case "subUsage":
            {
                if (value is not byte castValue) return false;
                instance.m_subUsage = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is Havok.hk2019.hkVertexFormat.HintFlags castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_flags = (Havok.hk2019.hkVertexFormat.HintFlags)byteValue;
                    return true;
                }
                return false;
            }
            case "m_pad":
            case "pad":
            {
                if (value is not byte[] castValue || castValue.Length != 3) return false;
                try
                {
                    _padInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            default:
            return false;
        }
    }

}
