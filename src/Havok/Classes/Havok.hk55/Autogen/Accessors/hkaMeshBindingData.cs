// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkaMeshBindingData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkaMeshBinding>
{
    public hkaMeshBindingData(Havok.hk55.hkaMeshBinding instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_mesh":
            case "mesh":
            {
                value = instance.m_mesh;
                return true;
            }
            case "m_skeleton":
            case "skeleton":
            {
                value = instance.m_skeleton;
                return true;
            }
            case "m_mappings":
            case "mappings":
            {
                value = instance.m_mappings;
                return true;
            }
            case "m_boneFromSkinMeshTransforms":
            case "boneFromSkinMeshTransforms":
            {
                value = instance.m_boneFromSkinMeshTransforms;
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
            case "m_mesh":
            case "mesh":
            {
                if (value is null)
                {
                    instance.m_mesh = default;
                    return true;
                }
                if (value is Havok.hk55.hkxMesh castValue)
                {
                    instance.m_mesh = castValue;
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
                if (value is Havok.hk55.hkaSkeleton castValue)
                {
                    instance.m_skeleton = castValue;
                    return true;
                }
                return false;
            }
            case "m_mappings":
            case "mappings":
            {
                if (value is not List<Havok.hk55.hkaMeshBinding.Mapping> castValue) return false;
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
