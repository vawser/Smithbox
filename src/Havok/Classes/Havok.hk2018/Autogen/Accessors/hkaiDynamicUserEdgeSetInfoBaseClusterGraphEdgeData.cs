// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiDynamicUserEdgeSetInfoBaseClusterGraphEdgeData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiDynamicUserEdgeSetInfoBase.ClusterGraphEdge>
{
    public hkaiDynamicUserEdgeSetInfoBaseClusterGraphEdgeData(Havok.hk2018.hkaiDynamicUserEdgeSetInfoBase.ClusterGraphEdge instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_startClusterKey":
            case "startClusterKey":
            {
                value = instance.m_startClusterKey;
                return true;
            }
            case "m_endClusterKey":
            case "endClusterKey":
            {
                value = instance.m_endClusterKey;
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
