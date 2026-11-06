// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbAssetBundleStringDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbAssetBundleStringData>
{
    public hkbAssetBundleStringDataData(Havok.hk2018.hkbAssetBundleStringData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bundleName":
            case "bundleName":
            {
                value = instance.m_bundleName;
                return true;
            }
            case "m_assetNames":
            case "assetNames":
            {
                value = instance.m_assetNames;
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
            case "m_bundleName":
            case "bundleName":
            {
                if (value is null)
                {
                    instance.m_bundleName = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_bundleName = castValue;
                    return true;
                }
                return false;
            }
            case "m_assetNames":
            case "assetNames":
            {
                if (value is not List<string?> castValue) return false;
                instance.m_assetNames = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
