// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkxMeshSectionData : HavokData<HKLib.hk2019.hkxMeshSection> 
{
    public hkxMeshSectionData(HavokType type, HKLib.hk2019.hkxMeshSection instance) : base(type, instance) {}

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
            case "m_indexBuffers":
            case "indexBuffers":
            {
                if (instance.m_indexBuffers is not TGet castValue) return false;
                value = castValue;
                return true;
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
            case "m_userChannels":
            case "userChannels":
            {
                if (instance.m_userChannels is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_vertexAnimations":
            case "vertexAnimations":
            {
                if (instance.m_vertexAnimations is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_linearKeyFrameHints":
            case "linearKeyFrameHints":
            {
                if (instance.m_linearKeyFrameHints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneMatrixMap":
            case "boneMatrixMap":
            {
                if (instance.m_boneMatrixMap is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_instances":
            case "instances":
            {
                if (instance.m_instances is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_originalBoundingBoxMin":
            case "originalBoundingBoxMin":
            {
                if (instance.m_originalBoundingBoxMin is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_originalBoundingBoxMax":
            case "originalBoundingBoxMax":
            {
                if (instance.m_originalBoundingBoxMax is not TGet castValue) return false;
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
                if (value is HKLib.hk2019.hkxVertexBuffer castValue)
                {
                    instance.m_vertexBuffer = castValue;
                    return true;
                }
                return false;
            }
            case "m_indexBuffers":
            case "indexBuffers":
            {
                if (value is not List<HKLib.hk2019.hkxIndexBuffer?> castValue) return false;
                instance.m_indexBuffers = castValue;
                return true;
            }
            case "m_material":
            case "material":
            {
                if (value is null)
                {
                    instance.m_material = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkxMaterial castValue)
                {
                    instance.m_material = castValue;
                    return true;
                }
                return false;
            }
            case "m_userChannels":
            case "userChannels":
            {
                if (value is not List<HKLib.hk2019.hkReferencedObject?> castValue) return false;
                instance.m_userChannels = castValue;
                return true;
            }
            case "m_vertexAnimations":
            case "vertexAnimations":
            {
                if (value is not List<HKLib.hk2019.hkxVertexAnimation?> castValue) return false;
                instance.m_vertexAnimations = castValue;
                return true;
            }
            case "m_linearKeyFrameHints":
            case "linearKeyFrameHints":
            {
                if (value is not List<float> castValue) return false;
                instance.m_linearKeyFrameHints = castValue;
                return true;
            }
            case "m_boneMatrixMap":
            case "boneMatrixMap":
            {
                if (value is not List<HKLib.hk2019.hkMeshBoneIndexMapping> castValue) return false;
                instance.m_boneMatrixMap = castValue;
                return true;
            }
            case "m_instances":
            case "instances":
            {
                if (value is not List<HKLib.hk2019.hkxMeshSection.InstanceInfo> castValue) return false;
                instance.m_instances = castValue;
                return true;
            }
            case "m_originalBoundingBoxMin":
            case "originalBoundingBoxMin":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_originalBoundingBoxMin = castValue;
                return true;
            }
            case "m_originalBoundingBoxMax":
            case "originalBoundingBoxMax":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_originalBoundingBoxMax = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
