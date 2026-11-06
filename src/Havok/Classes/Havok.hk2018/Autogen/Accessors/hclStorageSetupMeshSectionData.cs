// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclStorageSetupMeshSectionData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclStorageSetupMeshSection>
{
    public hclStorageSetupMeshSectionData(Havok.hk2018.hclStorageSetupMeshSection instance) : base(instance) { }

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
            case "m_sectionVertexChannels":
            case "sectionVertexChannels":
            {
                value = instance.m_sectionVertexChannels;
                return true;
            }
            case "m_sectionEdgeChannels":
            case "sectionEdgeChannels":
            {
                value = instance.m_sectionEdgeChannels;
                return true;
            }
            case "m_sectionTriangleChannels":
            case "sectionTriangleChannels":
            {
                value = instance.m_sectionTriangleChannels;
                return true;
            }
            case "m_boneInfluences":
            case "boneInfluences":
            {
                value = instance.m_boneInfluences;
                return true;
            }
            case "m_parentSetupMesh":
            case "parentSetupMesh":
            {
                value = instance.m_parentSetupMesh;
                return true;
            }
            case "m_vertices":
            case "vertices":
            {
                value = instance.m_vertices;
                return true;
            }
            case "m_normals":
            case "normals":
            {
                value = instance.m_normals;
                return true;
            }
            case "m_tangents":
            case "tangents":
            {
                value = instance.m_tangents;
                return true;
            }
            case "m_bitangents":
            case "bitangents":
            {
                value = instance.m_bitangents;
                return true;
            }
            case "m_triangles":
            case "triangles":
            {
                value = instance.m_triangles;
                return true;
            }
            case "m_normalIDs":
            case "normalIDs":
            {
                value = instance.m_normalIDs;
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
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_sectionVertexChannels":
            case "sectionVertexChannels":
            {
                if (value is not List<Havok.hk2018.hclStorageSetupMeshSection.SectionVertexChannel?> castValue) return false;
                instance.m_sectionVertexChannels = castValue;
                return true;
            }
            case "m_sectionEdgeChannels":
            case "sectionEdgeChannels":
            {
                if (value is not List<Havok.hk2018.hclStorageSetupMeshSection.SectionEdgeSelectionChannel?> castValue) return false;
                instance.m_sectionEdgeChannels = castValue;
                return true;
            }
            case "m_sectionTriangleChannels":
            case "sectionTriangleChannels":
            {
                if (value is not List<Havok.hk2018.hclStorageSetupMeshSection.SectionTriangleSelectionChannel?> castValue) return false;
                instance.m_sectionTriangleChannels = castValue;
                return true;
            }
            case "m_boneInfluences":
            case "boneInfluences":
            {
                if (value is not List<Havok.hk2018.hclStorageSetupMeshSection.BoneInfluences?> castValue) return false;
                instance.m_boneInfluences = castValue;
                return true;
            }
            case "m_parentSetupMesh":
            case "parentSetupMesh":
            {
                if (value is null)
                {
                    instance.m_parentSetupMesh = default;
                    return true;
                }
                if (value is Havok.hk2018.hclSetupMesh castValue)
                {
                    instance.m_parentSetupMesh = castValue;
                    return true;
                }
                return false;
            }
            case "m_vertices":
            case "vertices":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_vertices = castValue;
                return true;
            }
            case "m_normals":
            case "normals":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_normals = castValue;
                return true;
            }
            case "m_tangents":
            case "tangents":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_tangents = castValue;
                return true;
            }
            case "m_bitangents":
            case "bitangents":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_bitangents = castValue;
                return true;
            }
            case "m_triangles":
            case "triangles":
            {
                if (value is not List<Havok.hk2018.hclSetupMeshSection.Triangle> castValue) return false;
                instance.m_triangles = castValue;
                return true;
            }
            case "m_normalIDs":
            case "normalIDs":
            {
                if (value is not List<ushort> castValue) return false;
                instance.m_normalIDs = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
