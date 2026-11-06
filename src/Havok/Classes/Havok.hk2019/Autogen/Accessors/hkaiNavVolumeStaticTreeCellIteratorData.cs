// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavVolumeStaticTreeCellIteratorData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavVolumeStaticTreeCellIterator>
{
    public hkaiNavVolumeStaticTreeCellIteratorData(Havok.hk2019.hkaiNavVolumeStaticTreeCellIterator instance) : base(instance) { }

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
                if (value is null)
                {
                    instance.m_tree = default;
                    return true;
                }
                if (value is Havok.hk2019.hkcdStaticAabbTree castValue)
                {
                    instance.m_tree = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
