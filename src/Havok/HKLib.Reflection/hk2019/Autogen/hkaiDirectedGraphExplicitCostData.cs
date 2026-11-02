// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiDirectedGraphExplicitCostData : HavokData<HKLib.hk2019.hkaiDirectedGraphExplicitCost> 
{
    public hkaiDirectedGraphExplicitCostData(HavokType type, HKLib.hk2019.hkaiDirectedGraphExplicitCost instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_positions":
            case "positions":
            {
                if (instance.m_positions is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nodes":
            case "nodes":
            {
                if (instance.m_nodes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edges":
            case "edges":
            {
                if (instance.m_edges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nodeData":
            case "nodeData":
            {
                if (instance.m_nodeData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeData":
            case "edgeData":
            {
                if (instance.m_edgeData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nodeDataStriding":
            case "nodeDataStriding":
            {
                if (instance.m_nodeDataStriding is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeDataStriding":
            case "edgeDataStriding":
            {
                if (instance.m_edgeDataStriding is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_streamingSets":
            case "streamingSets":
            {
                if (instance.m_streamingSets is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
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
            case "m_positions":
            case "positions":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_positions = castValue;
                return true;
            }
            case "m_nodes":
            case "nodes":
            {
                if (value is not List<HKLib.hk2019.hkaiDirectedGraphExplicitCost.Node> castValue) return false;
                instance.m_nodes = castValue;
                return true;
            }
            case "m_edges":
            case "edges":
            {
                if (value is not List<HKLib.hk2019.hkaiDirectedGraphExplicitCost.Edge> castValue) return false;
                instance.m_edges = castValue;
                return true;
            }
            case "m_nodeData":
            case "nodeData":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_nodeData = castValue;
                return true;
            }
            case "m_edgeData":
            case "edgeData":
            {
                if (value is not List<uint> castValue) return false;
                instance.m_edgeData = castValue;
                return true;
            }
            case "m_nodeDataStriding":
            case "nodeDataStriding":
            {
                if (value is not int castValue) return false;
                instance.m_nodeDataStriding = castValue;
                return true;
            }
            case "m_edgeDataStriding":
            case "edgeDataStriding":
            {
                if (value is not int castValue) return false;
                instance.m_edgeDataStriding = castValue;
                return true;
            }
            case "m_streamingSets":
            case "streamingSets":
            {
                if (value is not List<HKLib.hk2019.hkaiAnnotatedStreamingSet> castValue) return false;
                instance.m_streamingSets = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
