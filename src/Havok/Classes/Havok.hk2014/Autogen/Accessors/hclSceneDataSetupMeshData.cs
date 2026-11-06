// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclSceneDataSetupMeshData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclSceneDataSetupMesh>
{
    public hclSceneDataSetupMeshData(Havok.hk2014.hclSceneDataSetupMesh instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_node":
            case "node":
            {
                value = instance.m_node;
                return true;
            }
            case "m_worldFromMesh":
            case "worldFromMesh":
            {
                value = instance.m_worldFromMesh;
                return true;
            }
            case "m_mesh":
            case "mesh":
            {
                value = instance.m_mesh;
                return true;
            }
            case "m_skinBinding":
            case "skinBinding":
            {
                value = instance.m_skinBinding;
                return true;
            }
            case "m_vertexChannels":
            case "vertexChannels":
            {
                value = instance.m_vertexChannels;
                return true;
            }
            case "m_triangleChannels":
            case "triangleChannels":
            {
                value = instance.m_triangleChannels;
                return true;
            }
            case "m_edgeChannels":
            case "edgeChannels":
            {
                value = instance.m_edgeChannels;
                return true;
            }
            case "m_meshBufferInterfaces":
            case "meshBufferInterfaces":
            {
                value = instance.m_meshBufferInterfaces;
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
            case "m_node":
            case "node":
            {
                if (value is null)
                {
                    instance.m_node = default;
                    return true;
                }
                if (value is Havok.hk2014.hkxNode castValue)
                {
                    instance.m_node = castValue;
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
            case "m_mesh":
            case "mesh":
            {
                if (value is null)
                {
                    instance.m_mesh = default;
                    return true;
                }
                if (value is Havok.hk2014.hkxMesh castValue)
                {
                    instance.m_mesh = castValue;
                    return true;
                }
                return false;
            }
            case "m_skinBinding":
            case "skinBinding":
            {
                if (value is null)
                {
                    instance.m_skinBinding = default;
                    return true;
                }
                if (value is Havok.hk2014.hkxSkinBinding castValue)
                {
                    instance.m_skinBinding = castValue;
                    return true;
                }
                return false;
            }
            case "m_vertexChannels":
            case "vertexChannels":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_vertexChannels = castValue;
                return true;
            }
            case "m_triangleChannels":
            case "triangleChannels":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_triangleChannels = castValue;
                return true;
            }
            case "m_edgeChannels":
            case "edgeChannels":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_edgeChannels = castValue;
                return true;
            }
            case "m_meshBufferInterfaces":
            case "meshBufferInterfaces":
            {
                if (value is not List<Havok.hk2014.hclSceneDataSetupMeshSection?> castValue) return false;
                instance.m_meshBufferInterfaces = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
