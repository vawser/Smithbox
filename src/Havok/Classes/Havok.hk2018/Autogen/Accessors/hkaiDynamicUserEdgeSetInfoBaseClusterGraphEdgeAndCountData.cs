// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiDynamicUserEdgeSetInfoBaseClusterGraphEdgeAndCountData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiDynamicUserEdgeSetInfoBase.ClusterGraphEdgeAndCount>
{
    public hkaiDynamicUserEdgeSetInfoBaseClusterGraphEdgeAndCountData(Havok.hk2018.hkaiDynamicUserEdgeSetInfoBase.ClusterGraphEdgeAndCount instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_edge":
            case "edge":
            {
                value = instance.m_edge;
                return true;
            }
            case "m_userEdgeCount":
            case "userEdgeCount":
            {
                value = instance.m_userEdgeCount;
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
            case "m_edge":
            case "edge":
            {
                if (value is not Havok.hk2018.hkaiDynamicUserEdgeSetInfoBase.ClusterGraphEdge castValue) return false;
                instance.m_edge = castValue;
                return true;
            }
            case "m_userEdgeCount":
            case "userEdgeCount":
            {
                if (value is not int castValue) return false;
                instance.m_userEdgeCount = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
