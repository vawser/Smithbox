// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkSkinnedMeshShapePartData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkSkinnedMeshShape.Part>
{
    public hkSkinnedMeshShapePartData(Havok.hk2018.hkSkinnedMeshShape.Part instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_startVertex":
            case "startVertex":
            {
                value = instance.m_startVertex;
                return true;
            }
            case "m_numVertices":
            case "numVertices":
            {
                value = instance.m_numVertices;
                return true;
            }
            case "m_startIndex":
            case "startIndex":
            {
                value = instance.m_startIndex;
                return true;
            }
            case "m_numIndices":
            case "numIndices":
            {
                value = instance.m_numIndices;
                return true;
            }
            case "m_boneSetId":
            case "boneSetId":
            {
                value = instance.m_boneSetId;
                return true;
            }
            case "m_meshSectionIndex":
            case "meshSectionIndex":
            {
                value = instance.m_meshSectionIndex;
                return true;
            }
            case "m_boundingSphere":
            case "boundingSphere":
            {
                value = instance.m_boundingSphere;
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
