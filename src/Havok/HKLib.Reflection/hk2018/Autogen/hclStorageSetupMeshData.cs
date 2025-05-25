// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclStorageSetupMeshData : HavokData<hclStorageSetupMesh> 
{
    public hclStorageSetupMeshData(HavokType type, hclStorageSetupMesh instance) : base(type, instance) {}

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
            case "m_worldFromMesh":
            case "worldFromMesh":
            {
                if (instance.m_worldFromMesh is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sections":
            case "sections":
            {
                if (instance.m_sections is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vertexChannels":
            case "vertexChannels":
            {
                if (instance.m_vertexChannels is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeChannels":
            case "edgeChannels":
            {
                if (instance.m_edgeChannels is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_triangleChannels":
            case "triangleChannels":
            {
                if (instance.m_triangleChannels is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bones":
            case "bones":
            {
                if (instance.m_bones is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isSkinned":
            case "isSkinned":
            {
                if (instance.m_isSkinned is not TGet castValue) return false;
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
            case "m_worldFromMesh":
            case "worldFromMesh":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_worldFromMesh = castValue;
                return true;
            }
            case "m_sections":
            case "sections":
            {
                if (value is not List<hclStorageSetupMeshSection?> castValue) return false;
                instance.m_sections = castValue;
                return true;
            }
            case "m_vertexChannels":
            case "vertexChannels":
            {
                if (value is not List<hclStorageSetupMesh.VertexChannel> castValue) return false;
                instance.m_vertexChannels = castValue;
                return true;
            }
            case "m_edgeChannels":
            case "edgeChannels":
            {
                if (value is not List<hclStorageSetupMesh.EdgeChannel> castValue) return false;
                instance.m_edgeChannels = castValue;
                return true;
            }
            case "m_triangleChannels":
            case "triangleChannels":
            {
                if (value is not List<hclStorageSetupMesh.TriangleChannel> castValue) return false;
                instance.m_triangleChannels = castValue;
                return true;
            }
            case "m_bones":
            case "bones":
            {
                if (value is not List<hclStorageSetupMesh.Bone> castValue) return false;
                instance.m_bones = castValue;
                return true;
            }
            case "m_isSkinned":
            case "isSkinned":
            {
                if (value is not bool castValue) return false;
                instance.m_isSkinned = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
