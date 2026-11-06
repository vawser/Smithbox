// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkSkinBindingData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkSkinBinding>
{
    public hkSkinBindingData(Havok.hk2014.hkSkinBinding instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_skin":
            case "skin":
            {
                value = instance.m_skin;
                return true;
            }
            case "m_worldFromBoneTransforms":
            case "worldFromBoneTransforms":
            {
                value = instance.m_worldFromBoneTransforms;
                return true;
            }
            case "m_boneNames":
            case "boneNames":
            {
                value = instance.m_boneNames;
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
            case "m_skin":
            case "skin":
            {
                if (value is null)
                {
                    instance.m_skin = default;
                    return true;
                }
                if (value is Havok.hk2014.hkMeshShape castValue)
                {
                    instance.m_skin = castValue;
                    return true;
                }
                return false;
            }
            case "m_worldFromBoneTransforms":
            case "worldFromBoneTransforms":
            {
                if (value is not List<Matrix4x4> castValue) return false;
                instance.m_worldFromBoneTransforms = castValue;
                return true;
            }
            case "m_boneNames":
            case "boneNames":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_boneNames = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
