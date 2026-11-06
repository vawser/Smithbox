// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hclShadowBufferDefinitionData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hclShadowBufferDefinition>
{
    public hclShadowBufferDefinitionData(Havok.hk2016.hclShadowBufferDefinition instance) : base(instance) { }

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
            case "m_triangleIndices":
            case "triangleIndices":
            {
                value = instance.m_triangleIndices;
                return true;
            }
            case "m_shadowPositions":
            case "shadowPositions":
            {
                value = instance.m_shadowPositions;
                return true;
            }
            case "m_shadowNormals":
            case "shadowNormals":
            {
                value = instance.m_shadowNormals;
                return true;
            }
            case "m_shadowTangents":
            case "shadowTangents":
            {
                value = instance.m_shadowTangents;
                return true;
            }
            case "m_shadowBiTangents":
            case "shadowBiTangents":
            {
                value = instance.m_shadowBiTangents;
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
                if (value is not Havok.hk2016.hclBufferLayout castValue) return false;
                instance.m_bufferLayout = castValue;
                return true;
            }
            case "m_triangleIndices":
            case "triangleIndices":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_triangleIndices = castValue;
                return true;
            }
            case "m_shadowPositions":
            case "shadowPositions":
            {
                if (value is not bool castValue) return false;
                instance.m_shadowPositions = castValue;
                return true;
            }
            case "m_shadowNormals":
            case "shadowNormals":
            {
                if (value is not bool castValue) return false;
                instance.m_shadowNormals = castValue;
                return true;
            }
            case "m_shadowTangents":
            case "shadowTangents":
            {
                if (value is not bool castValue) return false;
                instance.m_shadowTangents = castValue;
                return true;
            }
            case "m_shadowBiTangents":
            case "shadowBiTangents":
            {
                if (value is not bool castValue) return false;
                instance.m_shadowBiTangents = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
