// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkcdStaticAabbTreeImplData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkcdStaticAabbTree.Impl>
{
    public hkcdStaticAabbTreeImplData(Havok.hk2019.hkcdStaticAabbTree.Impl instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_tree":
            case "tree":
            {
                if (value is not Havok.hk2019.hkcdStaticTree.Aabb6BytesTree castValue) return false;
                instance.m_tree = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
