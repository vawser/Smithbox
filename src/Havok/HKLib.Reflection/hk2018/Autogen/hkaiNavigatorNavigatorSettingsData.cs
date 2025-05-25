// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavigatorNavigatorSettingsData : HavokData<hkaiNavigator.NavigatorSettings> 
{
    public hkaiNavigatorNavigatorSettingsData(HavokType type, hkaiNavigator.NavigatorSettings instance) : base(type, instance) {}

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
            case "m_up":
            case "up":
            {
                if (instance.m_up is not TGet castValue) return false;
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
            case "m_capsuleLimit":
            case "capsuleLimit":
            {
                if (instance.m_capsuleLimit is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lengthLimit":
            case "lengthLimit":
            {
                if (instance.m_lengthLimit is not TGet castValue) return false;
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
            case "m_hasCapsuleLimit":
            case "hasCapsuleLimit":
            {
                if (instance.m_hasCapsuleLimit is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hasLengthLimit":
            case "hasLengthLimit":
            {
                if (instance.m_hasLengthLimit is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_internalVertexPullingEnabled":
            case "internalVertexPullingEnabled":
            {
                if (instance.m_internalVertexPullingEnabled is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_agentTraversalInfo":
            case "agentTraversalInfo":
            {
                if (instance.m_agentTraversalInfo is not TGet castValue) return false;
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
            case "m_edgeFilter":
            case "edgeFilter":
            {
                if (instance.m_edgeFilter is null)
                {
                    return true;
                }
                if (instance.m_edgeFilter is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_costModifier":
            case "costModifier":
            {
                if (instance.m_costModifier is null)
                {
                    return true;
                }
                if (instance.m_costModifier is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_searchRadius":
            case "searchRadius":
            {
                if (instance.m_searchRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_horizontalTolerance":
            case "horizontalTolerance":
            {
                if (instance.m_horizontalTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_verticalTolerance":
            case "verticalTolerance":
            {
                if (instance.m_verticalTolerance is not TGet castValue) return false;
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
            case "m_leftTurningRadiusMultiplier":
            case "leftTurningRadiusMultiplier":
            {
                if (instance.m_leftTurningRadiusMultiplier is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_rightTurningRadiusMultiplier":
            case "rightTurningRadiusMultiplier":
            {
                if (instance.m_rightTurningRadiusMultiplier is not TGet castValue) return false;
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
                if (value is not hkaiNavigatorCapsuleLimit castValue) return false;
                instance.m_capsuleLimit = castValue;
                return true;
            }
            case "m_lengthLimit":
            case "lengthLimit":
            {
                if (value is not hkaiNavigatorLengthLimit castValue) return false;
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
            case "m_agentTraversalInfo":
            case "agentTraversalInfo":
            {
                if (value is not hkaiAgentTraversalInfo castValue) return false;
                instance.m_agentTraversalInfo = castValue;
                return true;
            }
            case "m_searchParameters":
            case "searchParameters":
            {
                if (value is not hkaiNavMeshPathSearchParameters castValue) return false;
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
                if (value is hkaiAstarEdgeFilter castValue)
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
                if (value is hkaiAstarCostModifier castValue)
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
