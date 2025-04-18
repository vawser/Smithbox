// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkaiHierarchyUtils;

namespace HKLib.Reflection.hk2018;

internal class hkaiHierarchyUtilsClusterSettingsData : HavokData<ClusterSettings> 
{
    public hkaiHierarchyUtilsClusterSettingsData(HavokType type, ClusterSettings instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_desiredFacesPerCluster":
            case "desiredFacesPerCluster":
            {
                if (instance.m_desiredFacesPerCluster is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_searchParameters":
            case "searchParameters":
            {
                if (instance.m_searchParameters is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_agentInfo":
            case "agentInfo":
            {
                if (instance.m_agentInfo is not TGet castValue) return false;
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
            case "m_desiredFacesPerCluster":
            case "desiredFacesPerCluster":
            {
                if (value is not int castValue) return false;
                instance.m_desiredFacesPerCluster = castValue;
                return true;
            }
            case "m_searchParameters":
            case "searchParameters":
            {
                if (value is not hkaiNavMeshPathSearchParameters castValue) return false;
                instance.m_searchParameters = castValue;
                return true;
            }
            case "m_agentInfo":
            case "agentInfo":
            {
                if (value is not hkaiAgentTraversalInfo castValue) return false;
                instance.m_agentInfo = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
