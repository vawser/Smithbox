// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiVolumeNavigatorNavigatorSettingsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiVolumeNavigator.NavigatorSettings>
{
    public hkaiVolumeNavigatorNavigatorSettingsData(Havok.hk2019.hkaiVolumeNavigator.NavigatorSettings instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_up":
            case "up":
            {
                value = instance.m_up;
                return true;
            }
            case "m_iterationLimit":
            case "iterationLimit":
            {
                value = instance.m_iterationLimit;
                return true;
            }
            case "m_layerIndex":
            case "layerIndex":
            {
                value = instance.m_layerIndex;
                return true;
            }
            case "m_searchRadius":
            case "searchRadius":
            {
                value = instance.m_searchRadius;
                return true;
            }
            case "m_goalReachedDistance":
            case "goalReachedDistance":
            {
                value = instance.m_goalReachedDistance;
                return true;
            }
            case "m_userEdgeEnteredDistance":
            case "userEdgeEnteredDistance":
            {
                value = instance.m_userEdgeEnteredDistance;
                return true;
            }
            case "m_agentTraversalInfo":
            case "agentTraversalInfo":
            {
                value = instance.m_agentTraversalInfo;
                return true;
            }
            case "m_searchParameters":
            case "searchParameters":
            {
                value = instance.m_searchParameters;
                return true;
            }
            case "m_edgeFilter":
            case "edgeFilter":
            {
                value = instance.m_edgeFilter;
                return true;
            }
            case "m_costModifier":
            case "costModifier":
            {
                value = instance.m_costModifier;
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
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            case "m_iterationLimit":
            case "iterationLimit":
            {
                if (value is not int castValue) return false;
                instance.m_iterationLimit = castValue;
                return true;
            }
            case "m_layerIndex":
            case "layerIndex":
            {
                if (value is not int castValue) return false;
                instance.m_layerIndex = castValue;
                return true;
            }
            case "m_searchRadius":
            case "searchRadius":
            {
                if (value is not float castValue) return false;
                instance.m_searchRadius = castValue;
                return true;
            }
            case "m_goalReachedDistance":
            case "goalReachedDistance":
            {
                if (value is not float castValue) return false;
                instance.m_goalReachedDistance = castValue;
                return true;
            }
            case "m_userEdgeEnteredDistance":
            case "userEdgeEnteredDistance":
            {
                if (value is not float castValue) return false;
                instance.m_userEdgeEnteredDistance = castValue;
                return true;
            }
            case "m_agentTraversalInfo":
            case "agentTraversalInfo":
            {
                if (value is not Havok.hk2019.hkaiAgentTraversalInfo castValue) return false;
                instance.m_agentTraversalInfo = castValue;
                return true;
            }
            case "m_searchParameters":
            case "searchParameters":
            {
                if (value is not Havok.hk2019.hkaiNavVolumePathSearchParameters castValue) return false;
                instance.m_searchParameters = castValue;
                return true;
            }
            case "m_edgeFilter":
            case "edgeFilter":
            {
                if (value is null)
                {
                    instance.m_edgeFilter = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiAstarEdgeFilter castValue)
                {
                    instance.m_edgeFilter = castValue;
                    return true;
                }
                return false;
            }
            case "m_costModifier":
            case "costModifier":
            {
                if (value is null)
                {
                    instance.m_costModifier = default;
                    return true;
                }
                if (value is Havok.hk2019.hkaiAstarCostModifier castValue)
                {
                    instance.m_costModifier = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
