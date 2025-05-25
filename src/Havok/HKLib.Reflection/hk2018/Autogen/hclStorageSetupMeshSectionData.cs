// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclStorageSetupMeshSectionData : HavokData<hclStorageSetupMeshSection> 
{
    public hclStorageSetupMeshSectionData(HavokType type, hclStorageSetupMeshSection instance) : base(type, instance) {}

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
            case "m_sectionVertexChannels":
            case "sectionVertexChannels":
            {
                if (instance.m_sectionVertexChannels is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sectionEdgeChannels":
            case "sectionEdgeChannels":
            {
                if (instance.m_sectionEdgeChannels is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sectionTriangleChannels":
            case "sectionTriangleChannels":
            {
                if (instance.m_sectionTriangleChannels is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneInfluences":
            case "boneInfluences":
            {
                if (instance.m_boneInfluences is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_parentSetupMesh":
            case "parentSetupMesh":
            {
                if (instance.m_parentSetupMesh is null)
                {
                    return true;
                }
                if (instance.m_parentSetupMesh is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_vertices":
            case "vertices":
            {
                if (instance.m_vertices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_normals":
            case "normals":
            {
                if (instance.m_normals is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_tangents":
            case "tangents":
            {
                if (instance.m_tangents is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bitangents":
            case "bitangents":
            {
                if (instance.m_bitangents is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_triangles":
            case "triangles":
            {
                if (instance.m_triangles is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_normalIDs":
            case "normalIDs":
            {
                if (instance.m_normalIDs is not TGet castValue) return false;
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
            case "m_sectionVertexChannels":
            case "sectionVertexChannels":
            {
                if (value is not List<hclStorageSetupMeshSection.SectionVertexChannel?> castValue) return false;
                instance.m_sectionVertexChannels = castValue;
                return true;
            }
            case "m_sectionEdgeChannels":
            case "sectionEdgeChannels":
            {
                if (value is not List<hclStorageSetupMeshSection.SectionEdgeSelectionChannel?> castValue) return false;
                instance.m_sectionEdgeChannels = castValue;
                return true;
            }
            case "m_sectionTriangleChannels":
            case "sectionTriangleChannels":
            {
                if (value is not List<hclStorageSetupMeshSection.SectionTriangleSelectionChannel?> castValue) return false;
                instance.m_sectionTriangleChannels = castValue;
                return true;
            }
            case "m_boneInfluences":
            case "boneInfluences":
            {
                if (value is not List<hclStorageSetupMeshSection.BoneInfluences?> castValue) return false;
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
                if (value is hclSetupMesh castValue)
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
                if (value is not List<hclSetupMeshSection.Triangle> castValue) return false;
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
