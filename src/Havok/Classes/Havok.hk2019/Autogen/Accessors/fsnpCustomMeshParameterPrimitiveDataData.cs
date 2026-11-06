// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class fsnpCustomMeshParameterPrimitiveDataData : Havok.Formats.HavokFieldAccess<Havok.hk2019.fsnpCustomMeshParameter.PrimitiveData>
{
    public fsnpCustomMeshParameterPrimitiveDataData(Havok.hk2019.fsnpCustomMeshParameter.PrimitiveData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertexData":
            case "vertexData":
            {
                value = instance.m_vertexData;
                return true;
            }
            case "m_triangleData":
            case "triangleData":
            {
                value = instance.m_triangleData;
                return true;
            }
            case "m_primitiveData":
            case "primitiveData":
            {
                value = instance.m_primitiveData;
                return true;
            }
            case "m_materialNameData":
            case "materialNameData":
            {
                value = instance.m_materialNameData;
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
