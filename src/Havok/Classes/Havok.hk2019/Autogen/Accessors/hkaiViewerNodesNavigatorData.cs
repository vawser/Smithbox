// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiViewerNodesNavigatorData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiViewerNodes.Navigator>
{
    public hkaiViewerNodesNavigatorData(Havok.hk2019.hkaiViewerNodes.Navigator instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_index":
            case "index":
            {
                value = instance.m_index;
                return true;
            }
            case "m_characterRadius":
            case "characterRadius":
            {
                value = instance.m_characterRadius;
                return true;
            }
            case "m_hasEdgeFilter":
            case "hasEdgeFilter":
            {
                value = instance.m_hasEdgeFilter;
                return true;
            }
            case "m_hasCostModifier":
            case "hasCostModifier":
            {
                value = instance.m_hasCostModifier;
                return true;
            }
            case "m_filterInfo":
            case "filterInfo":
            {
                value = instance.m_filterInfo;
                return true;
            }
            case "m_layerIndex":
            case "layerIndex":
            {
                value = instance.m_layerIndex;
                return true;
            }
            case "m_clearanceCacheIdentifier":
            case "clearanceCacheIdentifier":
            {
                value = instance.m_clearanceCacheIdentifier;
                return true;
            }
            case "m_hasCapsuleLimit":
            case "hasCapsuleLimit":
            {
                value = instance.m_hasCapsuleLimit;
                return true;
            }
            case "m_hasLengthLimit":
            case "hasLengthLimit":
            {
                value = instance.m_hasLengthLimit;
                return true;
            }
            case "m_iterationLimit":
            case "iterationLimit":
            {
                value = instance.m_iterationLimit;
                return true;
            }
            case "m_searchRadius":
            case "searchRadius":
            {
                value = instance.m_searchRadius;
                return true;
            }
            case "m_horizontalTolerance":
            case "horizontalTolerance":
            {
                value = instance.m_horizontalTolerance;
                return true;
            }
            case "m_verticalTolerance":
            case "verticalTolerance":
            {
                value = instance.m_verticalTolerance;
                return true;
            }
            case "m_usesHierarchicalHeuristic":
            case "usesHierarchicalHeuristic":
            {
                value = instance.m_usesHierarchicalHeuristic;
                return true;
            }
            case "m_heuristicWeight":
            case "heuristicWeight":
            {
                value = instance.m_heuristicWeight;
                return true;
            }
            case "m_internalVertexPullingEnabled":
            case "internalVertexPullingEnabled":
            {
                value = instance.m_internalVertexPullingEnabled;
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
            case "m_leftTurningRadiusMultiplier":
            case "leftTurningRadiusMultiplier":
            {
                value = instance.m_leftTurningRadiusMultiplier;
                return true;
            }
            case "m_rightTurningRadiusMultiplier":
            case "rightTurningRadiusMultiplier":
            {
                value = instance.m_rightTurningRadiusMultiplier;
                return true;
            }
            case "m_goals":
            case "goals":
            {
                value = instance.m_goals;
                return true;
            }
            case "m_position":
            case "position":
            {
                value = instance.m_position;
                return true;
            }
            case "m_navigationOutput":
            case "navigationOutput":
            {
                value = instance.m_navigationOutput;
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
            case "m_index":
            case "index":
            {
                if (value is not int castValue) return false;
                instance.m_index = castValue;
                return true;
            }
            case "m_characterRadius":
            case "characterRadius":
            {
                if (value is not float castValue) return false;
                instance.m_characterRadius = castValue;
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
            case "m_clearanceCacheIdentifier":
            case "clearanceCacheIdentifier":
            {
                if (value is not Havok.hk2019.hkHandle<byte> castValue) return false;
                instance.m_clearanceCacheIdentifier = castValue;
                return true;
            }
            case "m_hasCapsuleLimit":
            case "hasCapsuleLimit":
            {
                if (value is not bool castValue) return false;
                instance.m_hasCapsuleLimit = castValue;
                return true;
            }
            case "m_hasLengthLimit":
            case "hasLengthLimit":
            {
                if (value is not bool castValue) return false;
                instance.m_hasLengthLimit = castValue;
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
            case "m_horizontalTolerance":
            case "horizontalTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_horizontalTolerance = castValue;
                return true;
            }
            case "m_verticalTolerance":
            case "verticalTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_verticalTolerance = castValue;
                return true;
            }
            case "m_usesHierarchicalHeuristic":
            case "usesHierarchicalHeuristic":
            {
                if (value is not bool castValue) return false;
                instance.m_usesHierarchicalHeuristic = castValue;
                return true;
            }
            case "m_heuristicWeight":
            case "heuristicWeight":
            {
                if (value is not float castValue) return false;
                instance.m_heuristicWeight = castValue;
                return true;
            }
            case "m_internalVertexPullingEnabled":
            case "internalVertexPullingEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_internalVertexPullingEnabled = castValue;
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
            case "m_leftTurningRadiusMultiplier":
            case "leftTurningRadiusMultiplier":
            {
                if (value is not float castValue) return false;
                instance.m_leftTurningRadiusMultiplier = castValue;
                return true;
            }
            case "m_rightTurningRadiusMultiplier":
            case "rightTurningRadiusMultiplier":
            {
                if (value is not float castValue) return false;
                instance.m_rightTurningRadiusMultiplier = castValue;
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
                if (value is not Havok.hk2019.hkaiNavigator.NavigationOutput castValue) return false;
                instance.m_navigationOutput = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
