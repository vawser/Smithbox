// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkVertexFormatElementData : HavokData<hkVertexFormat.Element> 
{
    private static readonly System.Reflection.FieldInfo _padInfo = typeof(hkVertexFormat.Element).GetField("m_pad")!;
    public hkVertexFormatElementData(HavokType type, hkVertexFormat.Element instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_dataType":
            case "dataType":
            {
                if (instance.m_dataType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_dataType is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_numValues":
            case "numValues":
            {
                if (instance.m_numValues is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_usage":
            case "usage":
            {
                if (instance.m_usage is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_usage is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_subUsage":
            case "subUsage":
            {
                if (instance.m_subUsage is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_flags is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_pad":
            case "pad":
            {
                if (instance.m_pad is not TGet castValue) return false;
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
            case "m_dataType":
            case "dataType":
            {
                if (value is hkVertexFormat.ComponentType castValue)
                {
                    instance.m_dataType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_dataType = (hkVertexFormat.ComponentType)byteValue;
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
                if (value is hkVertexFormat.ComponentUsage castValue)
                {
                    instance.m_usage = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_usage = (hkVertexFormat.ComponentUsage)byteValue;
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
                if (value is hkVertexFormat.HintFlags castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_flags = (hkVertexFormat.HintFlags)byteValue;
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
