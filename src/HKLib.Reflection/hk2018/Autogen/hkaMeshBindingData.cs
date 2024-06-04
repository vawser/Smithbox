// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaMeshBindingData : HavokData<hkaMeshBinding> 
{
    public hkaMeshBindingData(HavokType type, hkaMeshBinding instance) : base(type, instance) {}

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
            case "m_originalSkeletonName":
            case "originalSkeletonName":
            {
                if (instance.m_originalSkeletonName is null)
                {
                    return true;
                }
                if (instance.m_originalSkeletonName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
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
            case "m_skeleton":
            case "skeleton":
            {
                if (instance.m_skeleton is null)
                {
                    return true;
                }
                if (instance.m_skeleton is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_mappings":
            case "mappings":
            {
                if (instance.m_mappings is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneFromSkinMeshTransforms":
            case "boneFromSkinMeshTransforms":
            {
                if (instance.m_boneFromSkinMeshTransforms is not TGet castValue) return false;
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
            case "m_originalSkeletonName":
            case "originalSkeletonName":
            {
                if (value is null)
                {
                    instance.m_originalSkeletonName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_originalSkeletonName = castValue;
                    return true;
                }
                return false;
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
            case "m_skeleton":
            case "skeleton":
            {
                if (value is null)
                {
                    instance.m_skeleton = default;
                    return true;
                }
                if (value is hkaSkeleton castValue)
                {
                    instance.m_skeleton = castValue;
                    return true;
                }
                return false;
            }
            case "m_mappings":
            case "mappings":
            {
                if (value is not List<hkaMeshBinding.Mapping> castValue) return false;
                instance.m_mappings = castValue;
                return true;
            }
            case "m_boneFromSkinMeshTransforms":
            case "boneFromSkinMeshTransforms":
            {
                if (value is not List<Matrix4x4> castValue) return false;
                instance.m_boneFromSkinMeshTransforms = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
