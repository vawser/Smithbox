// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclStaticShadowBufferDefinitionData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclStaticShadowBufferDefinition>
{
    public hclStaticShadowBufferDefinitionData(Havok.hk2014.hclStaticShadowBufferDefinition instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_subType":
            case "subType":
            {
                value = instance.m_subType;
                return true;
            }
            case "m_numVertices":
            case "numVertices":
            {
                value = instance.m_numVertices;
                return true;
            }
            case "m_numTriangles":
            case "numTriangles":
            {
                value = instance.m_numTriangles;
                return true;
            }
            case "m_bufferLayout":
            case "bufferLayout":
            {
                value = instance.m_bufferLayout;
                return true;
            }
            case "m_staticPositions":
            case "staticPositions":
            {
                value = instance.m_staticPositions;
                return true;
            }
            case "m_staticNormals":
            case "staticNormals":
            {
                value = instance.m_staticNormals;
                return true;
            }
            case "m_staticTangents":
            case "staticTangents":
            {
                value = instance.m_staticTangents;
                return true;
            }
            case "m_staticBiTangents":
            case "staticBiTangents":
            {
                value = instance.m_staticBiTangents;
                return true;
            }
            case "m_triangleIndices":
            case "triangleIndices":
            {
                value = instance.m_triangleIndices;
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
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_type":
            case "type":
            {
                if (value is not int castValue) return false;
                instance.m_type = castValue;
                return true;
            }
            case "m_subType":
            case "subType":
            {
                if (value is not int castValue) return false;
                instance.m_subType = castValue;
                return true;
            }
            case "m_numVertices":
            case "numVertices":
            {
                if (value is not uint castValue) return false;
                instance.m_numVertices = castValue;
                return true;
            }
            case "m_numTriangles":
            case "numTriangles":
            {
                if (value is not uint castValue) return false;
                instance.m_numTriangles = castValue;
                return true;
            }
            case "m_bufferLayout":
            case "bufferLayout":
            {
                if (value is not Havok.hk2014.hclBufferLayout castValue) return false;
                instance.m_bufferLayout = castValue;
                return true;
            }
            case "m_staticPositions":
            case "staticPositions":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_staticPositions = castValue;
                return true;
            }
            case "m_staticNormals":
            case "staticNormals":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_staticNormals = castValue;
                return true;
            }
            case "m_staticTangents":
            case "staticTangents":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_staticTangents = castValue;
                return true;
            }
            case "m_staticBiTangents":
            case "staticBiTangents":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_staticBiTangents = castValue;
                return true;
            }
            case "m_triangleIndices":
            case "triangleIndices":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_triangleIndices = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
