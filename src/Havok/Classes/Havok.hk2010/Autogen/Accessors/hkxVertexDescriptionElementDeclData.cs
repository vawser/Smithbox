// Automatically Generated

#nullable enable

namespace Havok.hk2010.Accessors;

internal sealed class hkxVertexDescriptionElementDeclData : Havok.Formats.HavokFieldAccess<Havok.hk2010.hkxVertexDescription.ElementDecl>
{
    public hkxVertexDescriptionElementDeclData(Havok.hk2010.hkxVertexDescription.ElementDecl instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_byteOffset":
            case "byteOffset":
            {
                value = instance.m_byteOffset;
                return true;
            }
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_usage":
            case "usage":
            {
                value = instance.m_usage;
                return true;
            }
            case "m_byteStride":
            case "byteStride":
            {
                value = instance.m_byteStride;
                return true;
            }
            case "m_numElements":
            case "numElements":
            {
                value = instance.m_numElements;
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
                if (value is Havok.hk2010.hkxVertexDescription.DataType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (Havok.hk2010.hkxVertexDescription.DataType)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_usage":
            case "usage":
            {
                if (value is Havok.hk2010.hkxVertexDescription.DataUsage castValue)
                {
                    instance.m_usage = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_usage = (Havok.hk2010.hkxVertexDescription.DataUsage)ushortValue;
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
            default:
            return false;
        }
    }

}
