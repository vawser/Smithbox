// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkMeshSectionCinfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkMeshSectionCinfo>
{
    public hkMeshSectionCinfoData(Havok.hk2014.hkMeshSectionCinfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertexBuffer":
            case "vertexBuffer":
            {
                value = instance.m_vertexBuffer;
                return true;
            }
            case "m_material":
            case "material":
            {
                value = instance.m_material;
                return true;
            }
            case "m_boneMatrixMap":
            case "boneMatrixMap":
            {
                value = instance.m_boneMatrixMap;
                return true;
            }
            case "m_primitiveType":
            case "primitiveType":
            {
                value = instance.m_primitiveType;
                return true;
            }
            case "m_numPrimitives":
            case "numPrimitives":
            {
                value = instance.m_numPrimitives;
                return true;
            }
            case "m_indexType":
            case "indexType":
            {
                value = instance.m_indexType;
                return true;
            }
            case "m_vertexStartIndex":
            case "vertexStartIndex":
            {
                value = instance.m_vertexStartIndex;
                return true;
            }
            case "m_transformIndex":
            case "transformIndex":
            {
                value = instance.m_transformIndex;
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
            case "m_vertexBuffer":
            case "vertexBuffer":
            {
                if (value is null)
                {
                    instance.m_vertexBuffer = default;
                    return true;
                }
                if (value is Havok.hk2014.hkMeshVertexBuffer castValue)
                {
                    instance.m_vertexBuffer = castValue;
                    return true;
                }
                return false;
            }
            case "m_material":
            case "material":
            {
                if (value is null)
                {
                    instance.m_material = default;
                    return true;
                }
                if (value is Havok.hk2014.hkMeshMaterial castValue)
                {
                    instance.m_material = castValue;
                    return true;
                }
                return false;
            }
            case "m_boneMatrixMap":
            case "boneMatrixMap":
            {
                if (value is not Havok.hk2014.hkMeshBoneIndexMapping castValue) return false;
                instance.m_boneMatrixMap = castValue;
                return true;
            }
            case "m_primitiveType":
            case "primitiveType":
            {
                if (value is Havok.hk2014.hkMeshSection.PrimitiveType castValue)
                {
                    instance.m_primitiveType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_primitiveType = (Havok.hk2014.hkMeshSection.PrimitiveType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_numPrimitives":
            case "numPrimitives":
            {
                if (value is not int castValue) return false;
                instance.m_numPrimitives = castValue;
                return true;
            }
            case "m_indexType":
            case "indexType":
            {
                if (value is Havok.hk2014.hkMeshSection.MeshSectionIndexType castValue)
                {
                    instance.m_indexType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_indexType = (Havok.hk2014.hkMeshSection.MeshSectionIndexType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_vertexStartIndex":
            case "vertexStartIndex":
            {
                if (value is not int castValue) return false;
                instance.m_vertexStartIndex = castValue;
                return true;
            }
            case "m_transformIndex":
            case "transformIndex":
            {
                if (value is not int castValue) return false;
                instance.m_transformIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
