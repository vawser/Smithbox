// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbAssetBundleStringDataData : HavokData<hkbAssetBundleStringData> 
{
    public hkbAssetBundleStringDataData(HavokType type, hkbAssetBundleStringData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bundleName":
            case "bundleName":
            {
                if (instance.m_bundleName is null)
                {
                    return true;
                }
                if (instance.m_bundleName is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_assetNames":
            case "assetNames":
            {
                if (instance.m_assetNames is not TGet castValue) return false;
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
