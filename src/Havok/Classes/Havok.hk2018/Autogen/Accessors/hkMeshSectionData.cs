// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkMeshSectionData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkMeshSection>
{
    public hkMeshSectionData(Havok.hk2018.hkMeshSection instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_numIndices":
            case "numIndices":
            {
                value = instance.m_numIndices;
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
            case "m_indexType":
            case "indexType":
            {
                value = instance.m_indexType;
                return true;
            }
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
            case "m_sectionIndex":
            case "sectionIndex":
            {
                value = instance.m_sectionIndex;
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
            case "m_primitiveType":
            case "primitiveType":
            {
                if (value is Havok.hk2018.hkMeshSection.PrimitiveType castValue)
                {
                    instance.m_primitiveType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_primitiveType = (Havok.hk2018.hkMeshSection.PrimitiveType)byteValue;
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
            case "m_numIndices":
            case "numIndices":
            {
                if (value is not int castValue) return false;
                instance.m_numIndices = castValue;
                return true;
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
            case "m_indexType":
            case "indexType":
            {
                if (value is Havok.hk2018.hkMeshSection.MeshSectionIndexType castValue)
                {
                    instance.m_indexType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_indexType = (Havok.hk2018.hkMeshSection.MeshSectionIndexType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_vertexBuffer":
            case "vertexBuffer":
            {
                if (value is null)
                {
                    instance.m_vertexBuffer = default;
                    return true;
                }
                if (value is Havok.hk2018.hkMeshVertexBuffer castValue)
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
                if (value is Havok.hk2018.hkMeshMaterial castValue)
                {
                    instance.m_material = castValue;
                    return true;
                }
                return false;
            }
            case "m_boneMatrixMap":
            case "boneMatrixMap":
            {
                if (value is null)
                {
                    instance.m_boneMatrixMap = default;
                    return true;
                }
                if (value is Havok.hk2018.hkMeshBoneIndexMapping castValue)
                {
                    instance.m_boneMatrixMap = castValue;
                    return true;
                }
                return false;
            }
            case "m_sectionIndex":
            case "sectionIndex":
            {
                if (value is not int castValue) return false;
                instance.m_sectionIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
