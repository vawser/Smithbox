// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkSkinnedMeshShapePartData : HavokData<hkSkinnedMeshShape.Part> 
{
    public hkSkinnedMeshShapePartData(HavokType type, hkSkinnedMeshShape.Part instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_startVertex":
            case "startVertex":
            {
                if (instance.m_startVertex is not TGet castValue) return false;
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
            case "m_startIndex":
            case "startIndex":
            {
                if (instance.m_startIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numIndices":
            case "numIndices":
            {
                if (instance.m_numIndices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneSetId":
            case "boneSetId":
            {
                if (instance.m_boneSetId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_meshSectionIndex":
            case "meshSectionIndex":
            {
                if (instance.m_meshSectionIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boundingSphere":
            case "boundingSphere":
            {
                if (instance.m_boundingSphere is not TGet castValue) return false;
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
            case "m_startVertex":
            case "startVertex":
            {
                if (value is not int castValue) return false;
                instance.m_startVertex = castValue;
                return true;
            }
            case "m_numVertices":
            case "numVertices":
            {
                if (value is not int castValue) return false;
                instance.m_numVertices = castValue;
                return true;
            }
            case "m_startIndex":
            case "startIndex":
            {
                if (value is not int castValue) return false;
                instance.m_startIndex = castValue;
                return true;
            }
            case "m_numIndices":
            case "numIndices":
            {
                if (value is not int castValue) return false;
                instance.m_numIndices = castValue;
                return true;
            }
            case "m_boneSetId":
            case "boneSetId":
            {
                if (value is not ushort castValue) return false;
                instance.m_boneSetId = castValue;
                return true;
            }
            case "m_meshSectionIndex":
            case "meshSectionIndex":
            {
                if (value is not ushort castValue) return false;
                instance.m_meshSectionIndex = castValue;
                return true;
            }
            case "m_boundingSphere":
            case "boundingSphere":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_boundingSphere = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
