// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxIndexBufferData : HavokData<HKLib.hk2019.hkxIndexBuffer> 
{
    public hkxIndexBufferData(HavokType type, HKLib.hk2019.hkxIndexBuffer instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_indexType":
            case "indexType":
            {
                if (instance.m_indexType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((sbyte)instance.m_indexType is TGet sbyteValue)
                {
                    value = sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_indices16":
            case "indices16":
            {
                if (instance.m_indices16 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_indices32":
            case "indices32":
            {
                if (instance.m_indices32 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vertexBaseOffset":
            case "vertexBaseOffset":
            {
                if (instance.m_vertexBaseOffset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_length":
            case "length":
            {
                if (instance.m_length is not TGet castValue) return false;
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
            case "m_indexType":
            case "indexType":
            {
                if (value is HKLib.hk2019.hkxIndexBuffer.IndexType castValue)
                {
                    instance.m_indexType = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_indexType = (HKLib.hk2019.hkxIndexBuffer.IndexType)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_indices16":
            case "indices16":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_indices16 = castValue;
                return true;
            }
            case "m_indices32":
            case "indices32":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_indices32 = castValue;
                return true;
            }
            case "m_vertexBaseOffset":
            case "vertexBaseOffset":
            {
                if (value is not uint castValue) return false;
                instance.m_vertexBaseOffset = castValue;
                return true;
            }
            case "m_length":
            case "length":
            {
                if (value is not uint castValue) return false;
                instance.m_length = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
