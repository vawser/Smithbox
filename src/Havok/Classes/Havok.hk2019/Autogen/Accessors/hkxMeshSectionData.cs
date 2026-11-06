// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkxMeshSectionData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkxMeshSection>
{
    public hkxMeshSectionData(Havok.hk2019.hkxMeshSection instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_vertexBuffer":
            case "vertexBuffer":
            {
                value = instance.m_vertexBuffer;
                return true;
            }
            case "m_indexBuffers":
            case "indexBuffers":
            {
                value = instance.m_indexBuffers;
                return true;
            }
            case "m_material":
            case "material":
            {
                value = instance.m_material;
                return true;
            }
            case "m_userChannels":
            case "userChannels":
            {
                value = instance.m_userChannels;
                return true;
            }
            case "m_vertexAnimations":
            case "vertexAnimations":
            {
                value = instance.m_vertexAnimations;
                return true;
            }
            case "m_linearKeyFrameHints":
            case "linearKeyFrameHints":
            {
                value = instance.m_linearKeyFrameHints;
                return true;
            }
            case "m_boneMatrixMap":
            case "boneMatrixMap":
            {
                value = instance.m_boneMatrixMap;
                return true;
            }
            case "m_instances":
            case "instances":
            {
                value = instance.m_instances;
                return true;
            }
            case "m_originalBoundingBoxMin":
            case "originalBoundingBoxMin":
            {
                value = instance.m_originalBoundingBoxMin;
                return true;
            }
            case "m_originalBoundingBoxMax":
            case "originalBoundingBoxMax":
            {
                value = instance.m_originalBoundingBoxMax;
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
            case "m_vertexBuffer":
            case "vertexBuffer":
            {
                if (value is null)
                {
                    instance.m_vertexBuffer = default;
                    return true;
                }
                if (value is Havok.hk2019.hkxVertexBuffer castValue)
                {
                    instance.m_vertexBuffer = castValue;
                    return true;
                }
                return false;
            }
            case "m_indexBuffers":
            case "indexBuffers":
            {
                if (value is not List<Havok.hk2019.hkxIndexBuffer?> castValue) return false;
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
                if (value is Havok.hk2019.hkxMaterial castValue)
                {
                    instance.m_material = castValue;
                    return true;
                }
                return false;
            }
            case "m_userChannels":
            case "userChannels":
            {
                if (value is not List<Havok.hk2019.hkReferencedObject?> castValue) return false;
                instance.m_userChannels = castValue;
                return true;
            }
            case "m_vertexAnimations":
            case "vertexAnimations":
            {
                if (value is not List<Havok.hk2019.hkxVertexAnimation?> castValue) return false;
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
                if (value is not List<Havok.hk2019.hkMeshBoneIndexMapping> castValue) return false;
                instance.m_boneMatrixMap = castValue;
                return true;
            }
            case "m_instances":
            case "instances":
            {
                if (value is not List<Havok.hk2019.hkxMeshSection.InstanceInfo> castValue) return false;
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
