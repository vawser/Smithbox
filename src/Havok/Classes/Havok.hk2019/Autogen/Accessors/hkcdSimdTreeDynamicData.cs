// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkcdSimdTreeDynamicData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkcdSimdTree.Dynamic>
{
    public hkcdSimdTreeDynamicData(Havok.hk2019.hkcdSimdTree.Dynamic instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_nodeData":
            case "nodeData":
            {
                value = instance.m_nodeData;
                return true;
            }
            case "m_leaves":
            case "leaves":
            {
                value = instance.m_leaves;
                return true;
            }
            case "m_firstFreeNode":
            case "firstFreeNode":
            {
                value = instance.m_firstFreeNode;
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
            case "m_nodeData":
            case "nodeData":
            {
                if (value is not List<Havok.hk2019.hkcdSimdTree.Dynamic.NodeData> castValue) return false;
                instance.m_nodeData = castValue;
                return true;
            }
            case "m_leaves":
            case "leaves":
            {
                if (value is not List<Havok.hk2019.hkTuple2<uint, uint>> castValue) return false;
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
