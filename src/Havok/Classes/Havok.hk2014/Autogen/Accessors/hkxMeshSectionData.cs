// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkxMeshSectionData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkxMeshSection>
{
    public hkxMeshSectionData(Havok.hk2014.hkxMeshSection instance) : base(instance) { }

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
                if (value is Havok.hk2014.hkxVertexBuffer castValue)
                {
                    instance.m_vertexBuffer = castValue;
                    return true;
                }
                return false;
            }
            case "m_indexBuffers":
            case "indexBuffers":
            {
                if (value is not List<Havok.hk2014.hkxIndexBuffer?> castValue) return false;
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
                if (value is Havok.hk2014.hkxMaterial castValue)
                {
                    instance.m_material = castValue;
                    return true;
                }
                return false;
            }
            case "m_userChannels":
            case "userChannels":
            {
                if (value is not List<Havok.hk2014.hkReferencedObject?> castValue) return false;
                instance.m_userChannels = castValue;
                return true;
            }
            case "m_vertexAnimations":
            case "vertexAnimations":
            {
                if (value is not List<Havok.hk2014.hkxVertexAnimation?> castValue) return false;
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
                if (value is not List<Havok.hk2014.hkMeshBoneIndexMapping> castValue) return false;
                instance.m_boneMatrixMap = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
