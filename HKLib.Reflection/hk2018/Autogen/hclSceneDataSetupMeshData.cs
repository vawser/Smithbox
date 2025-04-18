// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hclSceneDataSetupMeshData : HavokData<hclSceneDataSetupMesh> 
{
    public hclSceneDataSetupMeshData(HavokType type, hclSceneDataSetupMesh instance) : base(type, instance) {}

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
            case "m_node":
            case "node":
            {
                if (instance.m_node is null)
                {
                    return true;
                }
                if (instance.m_node is TGet castValue)
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
            case "m_mesh":
            case "mesh":
            {
                if (instance.m_mesh is null)
                {
                    return true;
                }
                if (instance.m_mesh is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_skinBinding":
            case "skinBinding":
            {
                if (instance.m_skinBinding is null)
                {
                    return true;
                }
                if (instance.m_skinBinding is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_vertexChannels":
            case "vertexChannels":
            {
                if (instance.m_vertexChannels is not TGet castValue) return false;
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
            case "m_edgeChannels":
            case "edgeChannels":
            {
                if (instance.m_edgeChannels is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_meshBufferInterfaces":
            case "meshBufferInterfaces":
            {
                if (instance.m_meshBufferInterfaces is not TGet castValue) return false;
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
            case "m_node":
            case "node":
            {
                if (value is null)
                {
                    instance.m_node = default;
                    return true;
                }
                if (value is hkxNode castValue)
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
                if (value is hkxMesh castValue)
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
                if (value is hkxSkinBinding castValue)
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
                if (value is not List<hclSceneDataSetupMeshSection?> castValue) return false;
                instance.m_meshBufferInterfaces = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
