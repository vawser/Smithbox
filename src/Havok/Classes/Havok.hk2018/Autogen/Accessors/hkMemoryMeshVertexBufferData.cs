// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkMemoryMeshVertexBufferData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkMemoryMeshVertexBuffer>
{
    private static readonly System.Reflection.FieldInfo _elementOffsetsInfo = typeof(Havok.hk2018.hkMemoryMeshVertexBuffer).GetField("m_elementOffsets")!;
    public hkMemoryMeshVertexBufferData(Havok.hk2018.hkMemoryMeshVertexBuffer instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_format":
            case "format":
            {
                value = instance.m_format;
                return true;
            }
            case "m_elementOffsets":
            case "elementOffsets":
            {
                value = instance.m_elementOffsets;
                return true;
            }
            case "m_memory":
            case "memory":
            {
                value = instance.m_memory;
                return true;
            }
            case "m_vertexStride":
            case "vertexStride":
            {
                value = instance.m_vertexStride;
                return true;
            }
            case "m_numVertices":
            case "numVertices":
            {
                value = instance.m_numVertices;
                return true;
            }
            case "m_isBigEndian":
            case "isBigEndian":
            {
                value = instance.m_isBigEndian;
                return true;
            }
            case "m_isSharable":
            case "isSharable":
            {
                value = instance.m_isSharable;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_format":
            case "format":
            {
                if (value is not Havok.hk2018.hkVertexFormat castValue) return false;
                instance.m_format = castValue;
                return true;
            }
            case "m_elementOffsets":
            case "elementOffsets":
            {
                if (value is not int[] castValue || castValue.Length != 32) return false;
                try
                {
                    _elementOffsetsInfo.SetValue(instance, value);
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            case "m_memory":
            case "memory":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_memory = castValue;
                return true;
            }
            case "m_vertexStride":
            case "vertexStride":
            {
                if (value is not int castValue) return false;
                instance.m_vertexStride = castValue;
                return true;
            }
            case "m_numVertices":
            case "numVertices":
            {
                if (value is not int castValue) return false;
                instance.m_numVertices = castValue;
                return true;
            }
            case "m_isBigEndian":
            case "isBigEndian":
            {
                if (value is not bool castValue) return false;
                instance.m_isBigEndian = castValue;
                return true;
            }
            case "m_isSharable":
            case "isSharable":
            {
                if (value is not bool castValue) return false;
                instance.m_isSharable = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
