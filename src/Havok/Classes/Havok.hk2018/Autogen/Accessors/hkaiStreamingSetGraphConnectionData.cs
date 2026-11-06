// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiStreamingSetGraphConnectionData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiStreamingSet.GraphConnection>
{
    public hkaiStreamingSetGraphConnectionData(Havok.hk2018.hkaiStreamingSet.GraphConnection instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_aNodeIndex":
            case "aNodeIndex":
            {
                value = instance.m_aNodeIndex;
                return true;
            }
            case "m_bNodeIndex":
            case "bNodeIndex":
            {
                value = instance.m_bNodeIndex;
                return true;
            }
            case "m_aEdgeData":
            case "aEdgeData":
            {
                value = instance.m_aEdgeData;
                return true;
            }
            case "m_bEdgeData":
            case "bEdgeData":
            {
                value = instance.m_bEdgeData;
                return true;
            }
            case "m_aEdgeCost":
            case "aEdgeCost":
            {
                value = instance.m_aEdgeCost;
                return true;
            }
            case "m_bEdgeCost":
            case "bEdgeCost":
            {
                value = instance.m_bEdgeCost;
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
