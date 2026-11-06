// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hclClothSetupContainerData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hclClothSetupContainer>
{
    public hclClothSetupContainerData(Havok.hk2014.hclClothSetupContainer instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_clothSetupDatas":
            case "clothSetupDatas":
            {
                value = instance.m_clothSetupDatas;
                return true;
            }
            case "m_namedSetupMeshWrappers":
            case "namedSetupMeshWrappers":
            {
                value = instance.m_namedSetupMeshWrappers;
                return true;
            }
            case "m_namedTransformSetWrappers":
            case "namedTransformSetWrappers":
            {
                value = instance.m_namedTransformSetWrappers;
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
            case "m_clothSetupDatas":
            case "clothSetupDatas":
            {
                if (value is not List<Havok.hk2014.hclClothSetupObject?> castValue) return false;
                instance.m_clothSetupDatas = castValue;
                return true;
            }
            case "m_namedSetupMeshWrappers":
            case "namedSetupMeshWrappers":
            {
                if (value is not List<Havok.hk2014.hclNamedSetupMesh?> castValue) return false;
                instance.m_namedSetupMeshWrappers = castValue;
                return true;
            }
            case "m_namedTransformSetWrappers":
            case "namedTransformSetWrappers":
            {
                if (value is not List<Havok.hk2014.hclNamedTransformSetSetupObject?> castValue) return false;
                instance.m_namedTransformSetWrappers = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
