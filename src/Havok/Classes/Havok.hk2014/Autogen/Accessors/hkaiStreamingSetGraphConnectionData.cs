// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkaiStreamingSetGraphConnectionData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkaiStreamingSet.GraphConnection>
{
    public hkaiStreamingSetGraphConnectionData(Havok.hk2014.hkaiStreamingSet.GraphConnection instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_nodeIndex":
            case "nodeIndex":
            {
                value = instance.m_nodeIndex;
                return true;
            }
            case "m_oppositeNodeIndex":
            case "oppositeNodeIndex":
            {
                value = instance.m_oppositeNodeIndex;
                return true;
            }
            case "m_edgeData":
            case "edgeData":
            {
                value = instance.m_edgeData;
                return true;
            }
            case "m_edgeCost":
            case "edgeCost":
            {
                value = instance.m_edgeCost;
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
            case "m_nodeIndex":
            case "nodeIndex":
            {
                if (value is not int castValue) return false;
                instance.m_nodeIndex = castValue;
                return true;
            }
            case "m_oppositeNodeIndex":
            case "oppositeNodeIndex":
            {
                if (value is not int castValue) return false;
                instance.m_oppositeNodeIndex = castValue;
                return true;
            }
            case "m_edgeData":
            case "edgeData":
            {
                if (value is not uint castValue) return false;
                instance.m_edgeData = castValue;
                return true;
            }
            case "m_edgeCost":
            case "edgeCost":
            {
                if (value is not float castValue) return false;
                instance.m_edgeCost = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
