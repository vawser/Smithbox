// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbAssetBundleData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbAssetBundle>
{
    public hkbAssetBundleData(Havok.hk2018.hkbAssetBundle instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_assets":
            case "assets":
            {
                value = instance.m_assets;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_type":
            case "type":
            {
                value = instance.m_type;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_assets":
            case "assets":
            {
                if (value is not List<Havok.hk2018.hkReferencedObject?> castValue) return false;
                instance.m_assets = castValue;
                return true;
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
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2018.hkbAssetBundle.BundleType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_type = (Havok.hk2018.hkbAssetBundle.BundleType)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
