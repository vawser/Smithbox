// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkxSkinBindingData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkxSkinBinding>
{
    public hkxSkinBindingData(Havok.hk55.hkxSkinBinding instance) : base(instance) { }

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
            case "m_mapping":
            case "mapping":
            {
                value = instance.m_mapping;
                return true;
            }
            case "m_bindPose":
            case "bindPose":
            {
                value = instance.m_bindPose;
                return true;
            }
            case "m_initSkinTransform":
            case "initSkinTransform":
            {
                value = instance.m_initSkinTransform;
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
            case "m_mapping":
            case "mapping":
            {
                if (value is not List<Havok.hk55.hkxNode?> castValue) return false;
                instance.m_mapping = castValue;
                return true;
            }
            case "m_bindPose":
            case "bindPose":
            {
                if (value is not List<Matrix4x4> castValue) return false;
                instance.m_bindPose = castValue;
                return true;
            }
            case "m_initSkinTransform":
            case "initSkinTransform":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_initSkinTransform = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
