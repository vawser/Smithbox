// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkcdSimdTreeDynamicNodeDataData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkcdSimdTree.Dynamic.NodeData>
{
    public hkcdSimdTreeDynamicNodeDataData(Havok.hk2018.hkcdSimdTree.Dynamic.NodeData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_parent":
            case "parent":
            {
                value = instance.m_parent;
                return true;
            }
            case "m_dirty":
            case "dirty":
            {
                value = instance.m_dirty;
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
            case "m_parent":
            case "parent":
            {
                if (value is not uint castValue) return false;
                instance.m_parent = castValue;
                return true;
            }
            case "m_dirty":
            case "dirty":
            {
                if (value is not uint castValue) return false;
                instance.m_dirty = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
