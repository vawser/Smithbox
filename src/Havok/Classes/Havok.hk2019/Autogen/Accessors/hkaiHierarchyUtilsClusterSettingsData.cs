// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiHierarchyUtilsClusterSettingsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiHierarchyUtils.ClusterSettings>
{
    public hkaiHierarchyUtilsClusterSettingsData(Havok.hk2019.hkaiHierarchyUtils.ClusterSettings instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_desiredFacesPerCluster":
            case "desiredFacesPerCluster":
            {
                value = instance.m_desiredFacesPerCluster;
                return true;
            }
            case "m_searchParameters":
            case "searchParameters":
            {
                value = instance.m_searchParameters;
                return true;
            }
            case "m_agentInfo":
            case "agentInfo":
            {
                value = instance.m_agentInfo;
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
                if (value is not Havok.hk2019.hkaiNavMeshPathSearchParameters castValue) return false;
                instance.m_searchParameters = castValue;
                return true;
            }
            case "m_agentInfo":
            case "agentInfo":
            {
                if (value is not Havok.hk2019.hkaiAgentTraversalInfo castValue) return false;
                instance.m_agentInfo = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
