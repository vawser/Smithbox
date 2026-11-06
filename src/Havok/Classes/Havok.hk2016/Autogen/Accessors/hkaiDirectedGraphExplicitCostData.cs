// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkaiDirectedGraphExplicitCostData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkaiDirectedGraphExplicitCost>
{
    public hkaiDirectedGraphExplicitCostData(Havok.hk2016.hkaiDirectedGraphExplicitCost instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_positions":
            case "positions":
            {
                value = instance.m_positions;
                return true;
            }
            case "m_nodes":
            case "nodes":
            {
                value = instance.m_nodes;
                return true;
            }
            case "m_edges":
            case "edges":
            {
                value = instance.m_edges;
                return true;
            }
            case "m_nodeData":
            case "nodeData":
            {
                value = instance.m_nodeData;
                return true;
            }
            case "m_edgeData":
            case "edgeData":
            {
                value = instance.m_edgeData;
                return true;
            }
            case "m_nodeDataStriding":
            case "nodeDataStriding":
            {
                value = instance.m_nodeDataStriding;
                return true;
            }
            case "m_edgeDataStriding":
            case "edgeDataStriding":
            {
                value = instance.m_edgeDataStriding;
                return true;
            }
            case "m_streamingSets":
            case "streamingSets":
            {
                value = instance.m_streamingSets;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not List<Havok.hk2016.hkDefaultPropertyBag> castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
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
                if (value is not List<Havok.hk2016.hkaiDirectedGraphExplicitCost.Node> castValue) return false;
                instance.m_nodes = castValue;
                return true;
            }
            case "m_edges":
            case "edges":
            {
                if (value is not List<Havok.hk2016.hkaiDirectedGraphExplicitCost.Edge> castValue) return false;
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
                if (value is not List<Havok.hk2016.hkaiAnnotatedStreamingSet> castValue) return false;
                instance.m_streamingSets = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
