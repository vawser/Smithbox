// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavigatorNavigatorSettingsData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavigator.NavigatorSettings>
{
    public hkaiNavigatorNavigatorSettingsData(Havok.hk2019.hkaiNavigator.NavigatorSettings instance) : base(instance) { }

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
            case "m_layerIndex":
            case "layerIndex":
            {
                value = instance.m_layerIndex;
                return true;
            }
            case "m_capsuleLimit":
            case "capsuleLimit":
            {
                value = instance.m_capsuleLimit;
                return true;
            }
            case "m_lengthLimit":
            case "lengthLimit":
            {
                value = instance.m_lengthLimit;
                return true;
            }
            case "m_iterationLimit":
            case "iterationLimit":
            {
                value = instance.m_iterationLimit;
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
            case "m_internalVertexPullingEnabled":
            case "internalVertexPullingEnabled":
            {
                value = instance.m_internalVertexPullingEnabled;
                return true;
            }
            case "m_vertexPullingGapTolerance":
            case "vertexPullingGapTolerance":
            {
                value = instance.m_vertexPullingGapTolerance;
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
            case "m_layerIndex":
            case "layerIndex":
            {
                if (value is not int castValue) return false;
                instance.m_layerIndex = castValue;
                return true;
            }
            case "m_capsuleLimit":
            case "capsuleLimit":
            {
                if (value is not Havok.hk2019.hkaiNavigatorCapsuleLimit castValue) return false;
                instance.m_capsuleLimit = castValue;
                return true;
            }
            case "m_lengthLimit":
            case "lengthLimit":
            {
                if (value is not Havok.hk2019.hkaiNavigatorLengthLimit castValue) return false;
                instance.m_lengthLimit = castValue;
                return true;
            }
            case "m_iterationLimit":
            case "iterationLimit":
            {
                if (value is not int castValue) return false;
                instance.m_iterationLimit = castValue;
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
            case "m_internalVertexPullingEnabled":
            case "internalVertexPullingEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_internalVertexPullingEnabled = castValue;
                return true;
            }
            case "m_vertexPullingGapTolerance":
            case "vertexPullingGapTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_vertexPullingGapTolerance = castValue;
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
                if (value is not Havok.hk2019.hkaiNavMeshPathSearchParameters castValue) return false;
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
            default:
            return false;
        }
    }

}
