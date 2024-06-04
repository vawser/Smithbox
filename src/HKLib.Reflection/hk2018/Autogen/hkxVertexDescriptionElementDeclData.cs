// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkxVertexDescriptionElementDeclData : HavokData<hkxVertexDescription.ElementDecl> 
{
    public hkxVertexDescriptionElementDeclData(HavokType type, hkxVertexDescription.ElementDecl instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_byteOffset":
            case "byteOffset":
            {
                if (instance.m_byteOffset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((ushort)instance.m_type is TGet ushortValue)
                {
                    value = ushortValue;
                    return true;
                }
                return false;
            }
            case "m_usage":
            case "usage":
            {
                if (instance.m_usage is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((ushort)instance.m_usage is TGet ushortValue)
                {
                    value = ushortValue;
                    return true;
                }
                return false;
            }
            case "m_byteStride":
            case "byteStride":
            {
                if (instance.m_byteStride is not TGet castValue) return false;
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
            case "m_channelID":
            case "channelID":
            {
                if (instance.m_channelID is null)
                {
                    return true;
                }
                if (instance.m_channelID is TGet castValue)
                {
                    value = castValue;
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
            case "m_byteOffset":
            case "byteOffset":
            {
                if (value is not uint castValue) return false;
                instance.m_byteOffset = castValue;
                return true;
            }
            case "m_type":
            case "type":
            {
                if (value is hkxVertexDescription.DataType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (hkxVertexDescription.DataType)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_usage":
            case "usage":
            {
                if (value is hkxVertexDescription.DataUsage castValue)
                {
                    instance.m_usage = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_usage = (hkxVertexDescription.DataUsage)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_byteStride":
            case "byteStride":
            {
                if (value is not uint castValue) return false;
                instance.m_byteStride = castValue;
                return true;
            }
            case "m_numElements":
            case "numElements":
            {
                if (value is not byte castValue) return false;
                instance.m_numElements = castValue;
                return true;
            }
            case "m_channelID":
            case "channelID":
            {
                if (value is null)
                {
                    instance.m_channelID = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_channelID = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
