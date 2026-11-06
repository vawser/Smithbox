// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxVertexBufferVertexDataData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxVertexBuffer.VertexData>
{
    public hkxVertexBufferVertexDataData(Havok.hk2019.hkxVertexBuffer.VertexData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vectorData":
            case "vectorData":
            {
                value = instance.m_vectorData;
                return true;
            }
            case "m_floatData":
            case "floatData":
            {
                value = instance.m_floatData;
                return true;
            }
            case "m_uint32Data":
            case "uint32Data":
            {
                value = instance.m_uint32Data;
                return true;
            }
            case "m_uint16Data":
            case "uint16Data":
            {
                value = instance.m_uint16Data;
                return true;
            }
            case "m_uint8Data":
            case "uint8Data":
            {
                value = instance.m_uint8Data;
                return true;
            }
            case "m_numVerts":
            case "numVerts":
            {
                value = instance.m_numVerts;
                return true;
            }
            case "m_vectorStride":
            case "vectorStride":
            {
                value = instance.m_vectorStride;
                return true;
            }
            case "m_floatStride":
            case "floatStride":
            {
                value = instance.m_floatStride;
                return true;
            }
            case "m_uint32Stride":
            case "uint32Stride":
            {
                value = instance.m_uint32Stride;
                return true;
            }
            case "m_uint16Stride":
            case "uint16Stride":
            {
                value = instance.m_uint16Stride;
                return true;
            }
            case "m_uint8Stride":
            case "uint8Stride":
            {
                value = instance.m_uint8Stride;
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
            case "m_vectorData":
            case "vectorData":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_vectorData = castValue;
                return true;
            }
            case "m_floatData":
            case "floatData":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_floatData = castValue;
                return true;
            }
            case "m_uint32Data":
            case "uint32Data":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_uint32Data = castValue;
                return true;
            }
            case "m_uint16Data":
            case "uint16Data":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_uint16Data = castValue;
                return true;
            }
            case "m_uint8Data":
            case "uint8Data":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_uint8Data = castValue;
                return true;
            }
            case "m_numVerts":
            case "numVerts":
            {
                if (value is not uint castValue) return false;
                instance.m_numVerts = castValue;
                return true;
            }
            case "m_vectorStride":
            case "vectorStride":
            {
                if (value is not uint castValue) return false;
                instance.m_vectorStride = castValue;
                return true;
            }
            case "m_floatStride":
            case "floatStride":
            {
                if (value is not uint castValue) return false;
                instance.m_floatStride = castValue;
                return true;
            }
            case "m_uint32Stride":
            case "uint32Stride":
            {
                if (value is not uint castValue) return false;
                instance.m_uint32Stride = castValue;
                return true;
            }
            case "m_uint16Stride":
            case "uint16Stride":
            {
                if (value is not uint castValue) return false;
                instance.m_uint16Stride = castValue;
                return true;
            }
            case "m_uint8Stride":
            case "uint8Stride":
            {
                if (value is not uint castValue) return false;
                instance.m_uint8Stride = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
