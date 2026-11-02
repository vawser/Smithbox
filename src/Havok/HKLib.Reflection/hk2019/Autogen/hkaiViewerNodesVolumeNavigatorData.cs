// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiViewerNodesVolumeNavigatorData : HavokData<HKLib.hk2019.hkaiViewerNodes.VolumeNavigator> 
{
    public hkaiViewerNodesVolumeNavigatorData(HavokType type, HKLib.hk2019.hkaiViewerNodes.VolumeNavigator instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_index":
            case "index":
            {
                if (instance.m_index is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hasEdgeFilter":
            case "hasEdgeFilter":
            {
                if (instance.m_hasEdgeFilter is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hasCostModifier":
            case "hasCostModifier":
            {
                if (instance.m_hasCostModifier is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_filterInfo":
            case "filterInfo":
            {
                if (instance.m_filterInfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_layerIndex":
            case "layerIndex":
            {
                if (instance.m_layerIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_iterationLimit":
            case "iterationLimit":
            {
                if (instance.m_iterationLimit is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_searchRadius":
            case "searchRadius":
            {
                if (instance.m_searchRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_goalReachedDistance":
            case "goalReachedDistance":
            {
                if (instance.m_goalReachedDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userEdgeEnteredDistance":
            case "userEdgeEnteredDistance":
            {
                if (instance.m_userEdgeEnteredDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_goals":
            case "goals":
            {
                if (instance.m_goals is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_position":
            case "position":
            {
                if (instance.m_position is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_navigationOutput":
            case "navigationOutput":
            {
                if (instance.m_navigationOutput is not TGet castValue) return false;
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
            case "m_index":
            case "index":
            {
                if (value is not int castValue) return false;
                instance.m_index = castValue;
                return true;
            }
            case "m_hasEdgeFilter":
            case "hasEdgeFilter":
            {
                if (value is not bool castValue) return false;
                instance.m_hasEdgeFilter = castValue;
                return true;
            }
            case "m_hasCostModifier":
            case "hasCostModifier":
            {
                if (value is not bool castValue) return false;
                instance.m_hasCostModifier = castValue;
                return true;
            }
            case "m_filterInfo":
            case "filterInfo":
            {
                if (value is not uint castValue) return false;
                instance.m_filterInfo = castValue;
                return true;
            }
            case "m_layerIndex":
            case "layerIndex":
            {
                if (value is not int castValue) return false;
                instance.m_layerIndex = castValue;
                return true;
            }
            case "m_iterationLimit":
            case "iterationLimit":
            {
                if (value is not int castValue) return false;
                instance.m_iterationLimit = castValue;
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
            case "m_goals":
            case "goals":
            {
                if (value is not List<Vector4> castValue) return false;
                instance.m_goals = castValue;
                return true;
            }
            case "m_position":
            case "position":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_position = castValue;
                return true;
            }
            case "m_navigationOutput":
            case "navigationOutput":
            {
                if (value is not HKLib.hk2019.hkaiVolumeNavigator.NavigationOutput castValue) return false;
                instance.m_navigationOutput = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
