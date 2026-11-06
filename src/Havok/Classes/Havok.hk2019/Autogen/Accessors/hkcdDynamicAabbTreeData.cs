// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkcdDynamicAabbTreeData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkcdDynamicAabbTree>
{
    public hkcdDynamicAabbTreeData(Havok.hk2019.hkcdDynamicAabbTree instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_treePtr":
            case "treePtr":
            {
                value = instance.m_treePtr;
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
            case "m_treePtr":
            case "treePtr":
            {
                if (value is null)
                {
                    instance.m_treePtr = default;
                    return true;
                }
                if (value is Havok.hk2019.hkcdDynamicAabbTree.Impl castValue)
                {
                    instance.m_treePtr = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
