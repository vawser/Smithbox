// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkcdSimdTreeDynamicNodeDataData : HavokData<HKLib.hk2019.hkcdSimdTree.Dynamic.NodeData> 
{
    public hkcdSimdTreeDynamicNodeDataData(HavokType type, HKLib.hk2019.hkcdSimdTree.Dynamic.NodeData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_parent":
            case "parent":
            {
                if (instance.m_parent is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dirty":
            case "dirty":
            {
                if (instance.m_dirty is not TGet castValue) return false;
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
