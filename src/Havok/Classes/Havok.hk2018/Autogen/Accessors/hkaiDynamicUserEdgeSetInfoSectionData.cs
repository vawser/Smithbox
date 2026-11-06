// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiDynamicUserEdgeSetInfoSectionData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiDynamicUserEdgeSetInfo.Section>
{
    public hkaiDynamicUserEdgeSetInfoSectionData(Havok.hk2018.hkaiDynamicUserEdgeSetInfo.Section instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_faceIndices":
            case "faceIndices":
            {
                value = instance.m_faceIndices;
                return true;
            }
            case "m_clusterGraphEdges":
            case "clusterGraphEdges":
            {
                value = instance.m_clusterGraphEdges;
                return true;
            }
            case "m_externalEdges":
            case "externalEdges":
            {
                value = instance.m_externalEdges;
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
            case "m_faceIndices":
            case "faceIndices":
            {
                if (value is not Havok.hk2018.hkHashSet<int> castValue) return false;
                instance.m_faceIndices = castValue;
                return true;
            }
            case "m_clusterGraphEdges":
            case "clusterGraphEdges":
            {
                if (value is not Havok.hk2018.hkHashMap<Havok.hk2018.hkaiDynamicUserEdgeSetInfoBase.ClusterGraphEdge, int> castValue) return false;
                instance.m_clusterGraphEdges = castValue;
                return true;
            }
            case "m_externalEdges":
            case "externalEdges":
            {
                if (value is not Havok.hk2018.hkHashMap<int, Havok.hk2018.hkaiDynamicUserEdgeSetInfo.ExternalEdges?> castValue) return false;
                instance.m_externalEdges = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
