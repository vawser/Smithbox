// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiDynamicUserEdgeSetInfoBaseClusterGraphEdgeData : HavokData<hkaiDynamicUserEdgeSetInfoBase.ClusterGraphEdge> 
{
    public hkaiDynamicUserEdgeSetInfoBaseClusterGraphEdgeData(HavokType type, hkaiDynamicUserEdgeSetInfoBase.ClusterGraphEdge instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_startClusterKey":
            case "startClusterKey":
            {
                if (instance.m_startClusterKey is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_endClusterKey":
            case "endClusterKey":
            {
                if (instance.m_endClusterKey is not TGet castValue) return false;
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
            case "m_startClusterKey":
            case "startClusterKey":
            {
                if (value is not uint castValue) return false;
                instance.m_startClusterKey = castValue;
                return true;
            }
            case "m_endClusterKey":
            case "endClusterKey":
            {
                if (value is not uint castValue) return false;
                instance.m_endClusterKey = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
