// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkcdSimdTreeDynamicData : HavokData<hkcdSimdTree.Dynamic> 
{
    public hkcdSimdTreeDynamicData(HavokType type, hkcdSimdTree.Dynamic instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_nodeData":
            case "nodeData":
            {
                if (instance.m_nodeData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_leaves":
            case "leaves":
            {
                if (instance.m_leaves is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_firstFreeNode":
            case "firstFreeNode":
            {
                if (instance.m_firstFreeNode is not TGet castValue) return false;
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
            case "m_nodeData":
            case "nodeData":
            {
                if (value is not List<hkcdSimdTree.Dynamic.NodeData> castValue) return false;
                instance.m_nodeData = castValue;
                return true;
            }
            case "m_leaves":
            case "leaves":
            {
                if (value is not List<hkTuple2<uint, uint>> castValue) return false;
                instance.m_leaves = castValue;
                return true;
            }
            case "m_firstFreeNode":
            case "firstFreeNode":
            {
                if (value is not int castValue) return false;
                instance.m_firstFreeNode = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
