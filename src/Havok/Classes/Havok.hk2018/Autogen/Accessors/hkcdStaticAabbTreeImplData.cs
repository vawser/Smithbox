// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkcdStaticAabbTreeImplData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkcdStaticAabbTree.Impl>
{
    public hkcdStaticAabbTreeImplData(Havok.hk2018.hkcdStaticAabbTree.Impl instance) : base(instance) { }

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
            case "m_tree":
            case "tree":
            {
                value = instance.m_tree;
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
            case "m_tree":
            case "tree":
            {
                if (value is not Havok.hk2018.hkcdStaticTree.Aabb6BytesTree castValue) return false;
                instance.m_tree = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
