// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkxIndexBufferData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkxIndexBuffer>
{
    public hkxIndexBufferData(Havok.hk2016.hkxIndexBuffer instance) : base(instance) { }

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
            case "m_indexType":
            case "indexType":
            {
                value = instance.m_indexType;
                return true;
            }
            case "m_indices16":
            case "indices16":
            {
                value = instance.m_indices16;
                return true;
            }
            case "m_indices32":
            case "indices32":
            {
                value = instance.m_indices32;
                return true;
            }
            case "m_vertexBaseOffset":
            case "vertexBaseOffset":
            {
                value = instance.m_vertexBaseOffset;
                return true;
            }
            case "m_length":
            case "length":
            {
                value = instance.m_length;
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
                if (value is not List<Havok.hk2016.hkDefaultPropertyBag> castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_indexType":
            case "indexType":
            {
                if (value is Havok.hk2016.hkxIndexBuffer.IndexType castValue)
                {
                    instance.m_indexType = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_indexType = (Havok.hk2016.hkxIndexBuffer.IndexType)sbyteValue;
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
