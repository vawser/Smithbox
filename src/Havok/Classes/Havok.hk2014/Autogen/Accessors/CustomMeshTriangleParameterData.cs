// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class CustomMeshTriangleParameterData : Havok.Formats.HavokFieldAccess<Havok.hk2014.CustomMeshTriangleParameter>
{
    public CustomMeshTriangleParameterData(Havok.hk2014.CustomMeshTriangleParameter instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_version":
            case "version":
            {
                value = instance.m_version;
                return true;
            }
            case "m_vertexDataBuffer":
            case "vertexDataBuffer":
            {
                value = instance.m_vertexDataBuffer;
                return true;
            }
            case "m_vertexDataStride":
            case "vertexDataStride":
            {
                value = instance.m_vertexDataStride;
                return true;
            }
            case "m_primitiveDataBuffer":
            case "primitiveDataBuffer":
            {
                value = instance.m_primitiveDataBuffer;
                return true;
            }
            case "m_materialNameData":
            case "materialNameData":
            {
                value = instance.m_materialNameData;
                return true;
            }
            case "m_triangleDataBuffer":
            case "triangleDataBuffer":
            {
                value = instance.m_triangleDataBuffer;
                return true;
            }
            case "m_triangleDataStride":
            case "triangleDataStride":
            {
                value = instance.m_triangleDataStride;
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
            case "m_version":
            case "version":
            {
                if (value is not uint castValue) return false;
                instance.m_version = castValue;
                return true;
            }
            case "m_vertexDataBuffer":
            case "vertexDataBuffer":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_vertexDataBuffer = castValue;
                return true;
            }
            case "m_vertexDataStride":
            case "vertexDataStride":
            {
                if (value is not int castValue) return false;
                instance.m_vertexDataStride = castValue;
                return true;
            }
            case "m_primitiveDataBuffer":
            case "primitiveDataBuffer":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_primitiveDataBuffer = castValue;
                return true;
            }
            case "m_materialNameData":
            case "materialNameData":
            {
                if (value is not uint castValue) return false;
                instance.m_materialNameData = castValue;
                return true;
            }
            case "m_triangleDataBuffer":
            case "triangleDataBuffer":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_triangleDataBuffer = castValue;
                return true;
            }
            case "m_triangleDataStride":
            case "triangleDataStride":
            {
                if (value is not int castValue) return false;
                instance.m_triangleDataStride = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
