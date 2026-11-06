// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hclStorageSetupMeshBoneData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hclStorageSetupMesh.Bone>
{
    public hclStorageSetupMeshBoneData(Havok.hk2018.hclStorageSetupMesh.Bone instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_boneFromSkin":
            case "boneFromSkin":
            {
                value = instance.m_boneFromSkin;
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
            case "m_boneFromSkin":
            case "boneFromSkin":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_boneFromSkin = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
