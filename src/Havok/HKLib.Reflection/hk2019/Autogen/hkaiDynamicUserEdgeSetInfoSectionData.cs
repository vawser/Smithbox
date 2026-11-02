// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiDynamicUserEdgeSetInfoSectionData : HavokData<HKLib.hk2019.hkaiDynamicUserEdgeSetInfo.Section> 
{
    public hkaiDynamicUserEdgeSetInfoSectionData(HavokType type, HKLib.hk2019.hkaiDynamicUserEdgeSetInfo.Section instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_faceIndices":
            case "faceIndices":
            {
                if (instance.m_faceIndices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_clusterGraphEdges":
            case "clusterGraphEdges":
            {
                if (instance.m_clusterGraphEdges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_externalEdges":
            case "externalEdges":
            {
                if (instance.m_externalEdges is not TGet castValue) return false;
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
            case "m_faceIndices":
            case "faceIndices":
            {
                if (value is not HKLib.hk2019.hkHashSet<int> castValue) return false;
                instance.m_faceIndices = castValue;
                return true;
            }
            case "m_clusterGraphEdges":
            case "clusterGraphEdges":
            {
                if (value is not HKLib.hk2019.hkHashMap<HKLib.hk2019.hkaiDynamicUserEdgeSetInfoBase.ClusterGraphEdge, int> castValue) return false;
                instance.m_clusterGraphEdges = castValue;
                return true;
            }
            case "m_externalEdges":
            case "externalEdges":
            {
                if (value is not HKLib.hk2019.hkHashMap<int, HKLib.hk2019.hkaiDynamicUserEdgeSetInfo.ExternalEdges?> castValue) return false;
                instance.m_externalEdges = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
