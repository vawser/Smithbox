// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiDynamicUserEdgeSetInfoExternalEdgesData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiDynamicUserEdgeSetInfo.ExternalEdges>
{
    public hkaiDynamicUserEdgeSetInfoExternalEdgesData(Havok.hk2019.hkaiDynamicUserEdgeSetInfo.ExternalEdges instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_sectionIndices":
            case "sectionIndices":
            {
                value = instance.m_sectionIndices;
                return true;
            }
            case "m_edges":
            case "edges":
            {
                value = instance.m_edges;
                return true;
            }
            case "m_clusterGraphEdges":
            case "clusterGraphEdges":
            {
                value = instance.m_clusterGraphEdges;
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
            case "m_sectionIndices":
            case "sectionIndices":
            {
                if (value is not Havok.hk2019.hkaiDynamicUserEdgeSetInfoBase.SectionIdxPair castValue) return false;
                instance.m_sectionIndices = castValue;
                return true;
            }
            case "m_edges":
            case "edges":
            {
                if (value is not Havok.hk2019.hkHashSet<Havok.hk2019.hkaiDynamicUserEdgeSetInfo.UserEdgePair> castValue) return false;
                instance.m_edges = castValue;
                return true;
            }
            case "m_clusterGraphEdges":
            case "clusterGraphEdges":
            {
                if (value is not Havok.hk2019.hkHashMap<Havok.hk2019.hkaiDynamicUserEdgeSetInfoBase.ClusterGraphEdge, int> castValue) return false;
                instance.m_clusterGraphEdges = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
