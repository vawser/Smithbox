// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkcdStaticTreeAabbTreeData<NODE> : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkcdStaticTree.AabbTree<NODE>>
{
    public hkcdStaticTreeAabbTreeData(Havok.hk2018.hkcdStaticTree.AabbTree<NODE> instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_nodes":
            case "nodes":
            {
                value = instance.m_nodes;
                return true;
            }
            case "m_domain":
            case "domain":
            {
                value = instance.m_domain;
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
            case "m_nodes":
            case "nodes":
            {
                if (value is not List<NODE> castValue) return false;
                instance.m_nodes = castValue;
                return true;
            }
            case "m_domain":
            case "domain":
            {
                if (value is not Havok.hk2018.hkAabb castValue) return false;
                instance.m_domain = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
