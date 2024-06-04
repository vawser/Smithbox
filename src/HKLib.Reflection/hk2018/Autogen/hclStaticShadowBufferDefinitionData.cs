// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclStaticShadowBufferDefinitionData : HavokData<hclStaticShadowBufferDefinition> 
{
    public hclStaticShadowBufferDefinitionData(HavokType type, hclStaticShadowBufferDefinition instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_type":
            case "type":
            {
                if (instance.m_type is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_subType":
            case "subType":
            {
                if (instance.m_subType is not TGet castValue) return false;
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
            case "m_numTriangles":
            case "numTriangles":
            {
                if (instance.m_numTriangles is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bufferLayout":
            case "bufferLayout":
            {
                if (instance.m_bufferLayout is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_staticPositions":
            case "staticPositions":
            {
                if (instance.m_staticPositions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_staticNormals":
            case "staticNormals":
            {
                if (instance.m_staticNormals is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_staticTangents":
            case "staticTangents":
            {
                if (instance.m_staticTangents is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_staticBiTangents":
            case "staticBiTangents":
            {
                if (instance.m_staticBiTangents is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_triangleIndices":
            case "triangleIndices":
            {
                if (instance.m_triangleIndices is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
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
                if (value is not hclBufferLayout castValue) return false;
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
