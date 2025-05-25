// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiDynamicUserEdgeSetInfoExternalEdgesData : HavokData<hkaiDynamicUserEdgeSetInfo.ExternalEdges> 
{
    public hkaiDynamicUserEdgeSetInfoExternalEdgesData(HavokType type, hkaiDynamicUserEdgeSetInfo.ExternalEdges instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sectionIndices":
            case "sectionIndices":
            {
                if (instance.m_sectionIndices is not TGet castValue) return false;
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
            case "m_clusterGraphEdges":
            case "clusterGraphEdges":
            {
                if (instance.m_clusterGraphEdges is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_sectionIndices":
            case "sectionIndices":
            {
                if (value is not hkaiDynamicUserEdgeSetInfoBase.SectionIdxPair castValue) return false;
                instance.m_sectionIndices = castValue;
                return true;
            }
            case "m_edges":
            case "edges":
            {
                if (value is not hkHashSet<hkaiDynamicUserEdgeSetInfo.UserEdgePair> castValue) return false;
                instance.m_edges = castValue;
                return true;
            }
            case "m_clusterGraphEdges":
            case "clusterGraphEdges":
            {
                if (value is not hkHashMap<hkaiDynamicUserEdgeSetInfoBase.ClusterGraphEdge, int> castValue) return false;
                instance.m_clusterGraphEdges = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
