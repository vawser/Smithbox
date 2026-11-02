// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkMemoryMeshVertexBufferData : HavokData<HKLib.hk2019.hkMemoryMeshVertexBuffer> 
{
    private static readonly System.Reflection.FieldInfo _elementOffsetsInfo = typeof(HKLib.hk2019.hkMemoryMeshVertexBuffer).GetField("m_elementOffsets")!;
    public hkMemoryMeshVertexBufferData(HavokType type, HKLib.hk2019.hkMemoryMeshVertexBuffer instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_format":
            case "format":
            {
                if (instance.m_format is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_elementOffsets":
            case "elementOffsets":
            {
                if (instance.m_elementOffsets is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_memory":
            case "memory":
            {
                if (instance.m_memory is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vertexStride":
            case "vertexStride":
            {
                if (instance.m_vertexStride is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numVertices":
            case "numVertices":
            {
                if (instance.m_numVertices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isBigEndian":
            case "isBigEndian":
            {
                if (instance.m_isBigEndian is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isSharable":
            case "isSharable":
            {
                if (instance.m_isSharable is not TGet castValue) return false;
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
            case "m_format":
            case "format":
            {
                if (value is not HKLib.hk2019.hkVertexFormat castValue) return false;
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
