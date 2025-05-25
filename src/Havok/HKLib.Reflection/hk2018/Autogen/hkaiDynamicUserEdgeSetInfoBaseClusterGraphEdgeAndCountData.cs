// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiDynamicUserEdgeSetInfoBaseClusterGraphEdgeAndCountData : HavokData<hkaiDynamicUserEdgeSetInfoBase.ClusterGraphEdgeAndCount> 
{
    public hkaiDynamicUserEdgeSetInfoBaseClusterGraphEdgeAndCountData(HavokType type, hkaiDynamicUserEdgeSetInfoBase.ClusterGraphEdgeAndCount instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_edge":
            case "edge":
            {
                if (instance.m_edge is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userEdgeCount":
            case "userEdgeCount":
            {
                if (instance.m_userEdgeCount is not TGet castValue) return false;
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
            case "m_edge":
            case "edge":
            {
                if (value is not hkaiDynamicUserEdgeSetInfoBase.ClusterGraphEdge castValue) return false;
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
