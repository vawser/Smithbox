// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkxVertexBufferVertexDataData : HavokData<hkxVertexBuffer.VertexData> 
{
    public hkxVertexBufferVertexDataData(HavokType type, hkxVertexBuffer.VertexData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vectorData":
            case "vectorData":
            {
                if (instance.m_vectorData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_floatData":
            case "floatData":
            {
                if (instance.m_floatData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_uint32Data":
            case "uint32Data":
            {
                if (instance.m_uint32Data is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_uint16Data":
            case "uint16Data":
            {
                if (instance.m_uint16Data is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_uint8Data":
            case "uint8Data":
            {
                if (instance.m_uint8Data is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numVerts":
            case "numVerts":
            {
                if (instance.m_numVerts is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vectorStride":
            case "vectorStride":
            {
                if (instance.m_vectorStride is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_floatStride":
            case "floatStride":
            {
                if (instance.m_floatStride is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_uint32Stride":
            case "uint32Stride":
            {
                if (instance.m_uint32Stride is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_uint16Stride":
            case "uint16Stride":
            {
                if (instance.m_uint16Stride is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_uint8Stride":
            case "uint8Stride":
            {
                if (instance.m_uint8Stride is not TGet castValue) return false;
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
