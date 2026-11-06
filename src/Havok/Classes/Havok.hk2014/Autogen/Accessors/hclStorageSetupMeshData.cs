// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclStorageSetupMeshData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclStorageSetupMesh>
{
    public hclStorageSetupMeshData(Havok.hk2014.hclStorageSetupMesh instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_worldFromMesh":
            case "worldFromMesh":
            {
                value = instance.m_worldFromMesh;
                return true;
            }
            case "m_sections":
            case "sections":
            {
                value = instance.m_sections;
                return true;
            }
            case "m_vertexChannels":
            case "vertexChannels":
            {
                value = instance.m_vertexChannels;
                return true;
            }
            case "m_edgeChannels":
            case "edgeChannels":
            {
                value = instance.m_edgeChannels;
                return true;
            }
            case "m_triangleChannels":
            case "triangleChannels":
            {
                value = instance.m_triangleChannels;
                return true;
            }
            case "m_bones":
            case "bones":
            {
                value = instance.m_bones;
                return true;
            }
            case "m_isSkinned":
            case "isSkinned":
            {
                value = instance.m_isSkinned;
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
                if (value is not List<Havok.hk2014.hclStorageSetupMeshSection?> castValue) return false;
                instance.m_sections = castValue;
                return true;
            }
            case "m_vertexChannels":
            case "vertexChannels":
            {
                if (value is not List<Havok.hk2014.hclStorageSetupMesh.VertexChannel> castValue) return false;
                instance.m_vertexChannels = castValue;
                return true;
            }
            case "m_edgeChannels":
            case "edgeChannels":
            {
                if (value is not List<Havok.hk2014.hclStorageSetupMesh.EdgeChannel> castValue) return false;
                instance.m_edgeChannels = castValue;
                return true;
            }
            case "m_triangleChannels":
            case "triangleChannels":
            {
                if (value is not List<Havok.hk2014.hclStorageSetupMesh.TriangleChannel> castValue) return false;
                instance.m_triangleChannels = castValue;
                return true;
            }
            case "m_bones":
            case "bones":
            {
                if (value is not List<Havok.hk2014.hclStorageSetupMesh.Bone> castValue) return false;
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
