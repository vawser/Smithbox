// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiStreamingSetGraphConnectionData : HavokData<hkaiStreamingSet.GraphConnection> 
{
    public hkaiStreamingSetGraphConnectionData(HavokType type, hkaiStreamingSet.GraphConnection instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_aNodeIndex":
            case "aNodeIndex":
            {
                if (instance.m_aNodeIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bNodeIndex":
            case "bNodeIndex":
            {
                if (instance.m_bNodeIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_aEdgeData":
            case "aEdgeData":
            {
                if (instance.m_aEdgeData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bEdgeData":
            case "bEdgeData":
            {
                if (instance.m_bEdgeData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_aEdgeCost":
            case "aEdgeCost":
            {
                if (instance.m_aEdgeCost is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bEdgeCost":
            case "bEdgeCost":
            {
                if (instance.m_bEdgeCost is not TGet castValue) return false;
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
            case "m_aNodeIndex":
            case "aNodeIndex":
            {
                if (value is not int castValue) return false;
                instance.m_aNodeIndex = castValue;
                return true;
            }
            case "m_bNodeIndex":
            case "bNodeIndex":
            {
                if (value is not int castValue) return false;
                instance.m_bNodeIndex = castValue;
                return true;
            }
            case "m_aEdgeData":
            case "aEdgeData":
            {
                if (value is not uint castValue) return false;
                instance.m_aEdgeData = castValue;
                return true;
            }
            case "m_bEdgeData":
            case "bEdgeData":
            {
                if (value is not uint castValue) return false;
                instance.m_bEdgeData = castValue;
                return true;
            }
            case "m_aEdgeCost":
            case "aEdgeCost":
            {
                if (value is not float castValue) return false;
                instance.m_aEdgeCost = castValue;
                return true;
            }
            case "m_bEdgeCost":
            case "bEdgeCost":
            {
                if (value is not float castValue) return false;
                instance.m_bEdgeCost = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
