// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiDirectedGraphExplicitCostNodeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiDirectedGraphExplicitCost.Node>
{
    public hkaiDirectedGraphExplicitCostNodeData(Havok.hk2018.hkaiDirectedGraphExplicitCost.Node instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_startEdgeIndex":
            case "startEdgeIndex":
            {
                value = instance.m_startEdgeIndex;
                return true;
            }
            case "m_numEdges":
            case "numEdges":
            {
                value = instance.m_numEdges;
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
            case "m_startEdgeIndex":
            case "startEdgeIndex":
            {
                if (value is not int castValue) return false;
                instance.m_startEdgeIndex = castValue;
                return true;
            }
            case "m_numEdges":
            case "numEdges":
            {
                if (value is not int castValue) return false;
                instance.m_numEdges = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
