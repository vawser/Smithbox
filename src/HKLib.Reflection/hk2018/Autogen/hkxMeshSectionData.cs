// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkxMeshSectionData : HavokData<hkxMeshSection> 
{
    public hkxMeshSectionData(HavokType type, hkxMeshSection instance) : base(type, instance) {}

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
            case "m_vertexBuffer":
            case "vertexBuffer":
            {
                if (value is null)
                {
                    instance.m_vertexBuffer = default;
                    return true;
                }
                if (value is hkxVertexBuffer castValue)
                {
                    instance.m_vertexBuffer = castValue;
                    return true;
                }
                return false;
            }
            case "m_indexBuffers":
            case "indexBuffers":
            {
                if (value is not List<hkxIndexBuffer?> castValue) return false;
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
                if (value is hkxMaterial castValue)
                {
                    instance.m_material = castValue;
                    return true;
                }
                return false;
            }
            case "m_userChannels":
            case "userChannels":
            {
                if (value is not List<hkReferencedObject?> castValue) return false;
                instance.m_userChannels = castValue;
                return true;
            }
            case "m_vertexAnimations":
            case "vertexAnimations":
            {
                if (value is not List<hkxVertexAnimation?> castValue) return false;
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
                if (value is not List<hkMeshBoneIndexMapping> castValue) return false;
                instance.m_boneMatrixMap = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
