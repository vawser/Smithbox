// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class fsnpCustomMeshParameterPrimitiveDataData : HavokData<fsnpCustomMeshParameter.PrimitiveData> 
{
    public fsnpCustomMeshParameterPrimitiveDataData(HavokType type, fsnpCustomMeshParameter.PrimitiveData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertexData":
            case "vertexData":
            {
                if (instance.m_vertexData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_triangleData":
            case "triangleData":
            {
                if (instance.m_triangleData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_primitiveData":
            case "primitiveData":
            {
                if (instance.m_primitiveData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_materialNameData":
            case "materialNameData":
            {
                if (instance.m_materialNameData is not TGet castValue) return false;
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
            case "m_vertexData":
            case "vertexData":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_vertexData = castValue;
                return true;
            }
            case "m_triangleData":
            case "triangleData":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_triangleData = castValue;
                return true;
            }
            case "m_primitiveData":
            case "primitiveData":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_primitiveData = castValue;
                return true;
            }
            case "m_materialNameData":
            case "materialNameData":
            {
                if (value is not uint castValue) return false;
                instance.m_materialNameData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
