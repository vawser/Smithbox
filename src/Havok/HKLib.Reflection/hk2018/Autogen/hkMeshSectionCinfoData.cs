// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkMeshSectionCinfoData : HavokData<hkMeshSectionCinfo> 
{
    public hkMeshSectionCinfoData(HavokType type, hkMeshSectionCinfo instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertexBuffer":
            case "vertexBuffer":
            {
                if (instance.m_vertexBuffer is null)
                {
                    return true;
                }
                if (instance.m_vertexBuffer is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_material":
            case "material":
            {
                if (instance.m_material is null)
                {
                    return true;
                }
                if (instance.m_material is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_boneMatrixMap":
            case "boneMatrixMap":
            {
                if (instance.m_boneMatrixMap is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_primitiveType":
            case "primitiveType":
            {
                if (instance.m_primitiveType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_primitiveType is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_numPrimitives":
            case "numPrimitives":
            {
                if (instance.m_numPrimitives is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_indexType":
            case "indexType":
            {
                if (instance.m_indexType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_indexType is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_vertexStartIndex":
            case "vertexStartIndex":
            {
                if (instance.m_vertexStartIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transformIndex":
            case "transformIndex":
            {
                if (instance.m_transformIndex is not TGet castValue) return false;
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
            case "m_vertexBuffer":
            case "vertexBuffer":
            {
                if (value is null)
                {
                    instance.m_vertexBuffer = default;
                    return true;
                }
                if (value is hkMeshVertexBuffer castValue)
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
                if (value is hkMeshMaterial castValue)
                {
                    instance.m_material = castValue;
                    return true;
                }
                return false;
            }
            case "m_boneMatrixMap":
            case "boneMatrixMap":
            {
                if (value is not hkMeshBoneIndexMapping castValue) return false;
                instance.m_boneMatrixMap = castValue;
                return true;
            }
            case "m_primitiveType":
            case "primitiveType":
            {
                if (value is hkMeshSection.PrimitiveType castValue)
                {
                    instance.m_primitiveType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_primitiveType = (hkMeshSection.PrimitiveType)byteValue;
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
                if (value is hkMeshSection.MeshSectionIndexType castValue)
                {
                    instance.m_indexType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_indexType = (hkMeshSection.MeshSectionIndexType)byteValue;
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
