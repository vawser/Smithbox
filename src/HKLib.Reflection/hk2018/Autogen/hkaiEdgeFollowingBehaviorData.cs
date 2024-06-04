// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiEdgeFollowingBehaviorData : HavokData<hkaiEdgeFollowingBehavior> 
{
    public hkaiEdgeFollowingBehaviorData(HavokType type, hkaiEdgeFollowingBehavior instance) : base(type, instance) {}

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
            case "m_world":
            case "world":
            {
                if (instance.m_world is null)
                {
                    return true;
                }
                if (instance.m_world is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_character":
            case "character":
            {
                if (instance.m_character is null)
                {
                    return true;
                }
                if (instance.m_character is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_callbackType":
            case "callbackType":
            {
                if (instance.m_callbackType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_callbackType is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_immediateNavMeshRequest":
            case "immediateNavMeshRequest":
            {
                if (instance.m_immediateNavMeshRequest is null)
                {
                    return true;
                }
                if (instance.m_immediateNavMeshRequest is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_immediateNavVolumeRequest":
            case "immediateNavVolumeRequest":
            {
                if (instance.m_immediateNavVolumeRequest is null)
                {
                    return true;
                }
                if (instance.m_immediateNavVolumeRequest is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_requestedGoalPoints":
            case "requestedGoalPoints":
            {
                if (instance.m_requestedGoalPoints is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_currentGoalIndex":
            case "currentGoalIndex":
            {
                if (instance.m_currentGoalIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_updateQuerySize":
            case "updateQuerySize":
            {
                if (instance.m_updateQuerySize is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_characterRadiusMultiplier":
            case "characterRadiusMultiplier":
            {
                if (instance.m_characterRadiusMultiplier is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxIgnoredHeight":
            case "maxIgnoredHeight":
            {
                if (instance.m_maxIgnoredHeight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgePath":
            case "edgePath":
            {
                if (instance.m_edgePath is null)
                {
                    return true;
                }
                if (instance.m_edgePath is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_traversalState":
            case "traversalState":
            {
                if (instance.m_traversalState is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_newCharacterState":
            case "newCharacterState":
            {
                if (instance.m_newCharacterState is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_newCharacterState is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_pathFollowingProperties":
            case "pathFollowingProperties":
            {
                if (instance.m_pathFollowingProperties is null)
                {
                    return true;
                }
                if (instance.m_pathFollowingProperties is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_highestUserEdgeNotified":
            case "highestUserEdgeNotified":
            {
                if (instance.m_highestUserEdgeNotified is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userEdgeFakePathPoint":
            case "userEdgeFakePathPoint":
            {
                if (instance.m_userEdgeFakePathPoint is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_savedCharacterState":
            case "savedCharacterState":
            {
                if (instance.m_savedCharacterState is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_savedCharacterState is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_cornerPredictorInitInfo":
            case "cornerPredictorInitInfo":
            {
                if (instance.m_cornerPredictorInitInfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_passiveAvoidance":
            case "passiveAvoidance":
            {
                if (instance.m_passiveAvoidance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_resubmitEarlyIterationTerminations":
            case "resubmitEarlyIterationTerminations":
            {
                if (instance.m_resubmitEarlyIterationTerminations is not TGet castValue) return false;
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
            case "m_world":
            case "world":
            {
                if (value is null)
                {
                    instance.m_world = default;
                    return true;
                }
                if (value is hkaiWorld castValue)
                {
                    instance.m_world = castValue;
                    return true;
                }
                return false;
            }
            case "m_character":
            case "character":
            {
                if (value is null)
                {
                    instance.m_character = default;
                    return true;
                }
                if (value is hkaiCharacter castValue)
                {
                    instance.m_character = castValue;
                    return true;
                }
                return false;
            }
            case "m_callbackType":
            case "callbackType":
            {
                if (value is hkaiCharacterUtil.CallbackType castValue)
                {
                    instance.m_callbackType = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_callbackType = (hkaiCharacterUtil.CallbackType)intValue;
                    return true;
                }
                return false;
            }
            case "m_immediateNavMeshRequest":
            case "immediateNavMeshRequest":
            {
                if (value is null)
                {
                    instance.m_immediateNavMeshRequest = default;
                    return true;
                }
                if (value is hkaiNavMeshPathRequestInfo castValue)
                {
                    instance.m_immediateNavMeshRequest = castValue;
                    return true;
                }
                return false;
            }
            case "m_immediateNavVolumeRequest":
            case "immediateNavVolumeRequest":
            {
                if (value is null)
                {
                    instance.m_immediateNavVolumeRequest = default;
                    return true;
                }
                if (value is hkaiNavVolumePathRequestInfo castValue)
                {
                    instance.m_immediateNavVolumeRequest = castValue;
                    return true;
                }
                return false;
            }
            case "m_requestedGoalPoints":
            case "requestedGoalPoints":
            {
                if (value is not List<hkaiSingleCharacterBehavior.RequestedGoalPoint> castValue) return false;
                instance.m_requestedGoalPoints = castValue;
                return true;
            }
            case "m_currentGoalIndex":
            case "currentGoalIndex":
            {
                if (value is not int castValue) return false;
                instance.m_currentGoalIndex = castValue;
                return true;
            }
            case "m_updateQuerySize":
            case "updateQuerySize":
            {
                if (value is not float castValue) return false;
                instance.m_updateQuerySize = castValue;
                return true;
            }
            case "m_characterRadiusMultiplier":
            case "characterRadiusMultiplier":
            {
                if (value is not float castValue) return false;
                instance.m_characterRadiusMultiplier = castValue;
                return true;
            }
            case "m_maxIgnoredHeight":
            case "maxIgnoredHeight":
            {
                if (value is not float castValue) return false;
                instance.m_maxIgnoredHeight = castValue;
                return true;
            }
            case "m_edgePath":
            case "edgePath":
            {
                if (value is null)
                {
                    instance.m_edgePath = default;
                    return true;
                }
                if (value is hkaiEdgePath castValue)
                {
                    instance.m_edgePath = castValue;
                    return true;
                }
                return false;
            }
            case "m_traversalState":
            case "traversalState":
            {
                if (value is not hkaiEdgePath.TraversalState castValue) return false;
                instance.m_traversalState = castValue;
                return true;
            }
            case "m_newCharacterState":
            case "newCharacterState":
            {
                if (value is hkaiCharacter.State castValue)
                {
                    instance.m_newCharacterState = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_newCharacterState = (hkaiCharacter.State)intValue;
                    return true;
                }
                return false;
            }
            case "m_pathFollowingProperties":
            case "pathFollowingProperties":
            {
                if (value is null)
                {
                    instance.m_pathFollowingProperties = default;
                    return true;
                }
                if (value is hkaiPathFollowingProperties castValue)
                {
                    instance.m_pathFollowingProperties = castValue;
                    return true;
                }
                return false;
            }
            case "m_highestUserEdgeNotified":
            case "highestUserEdgeNotified":
            {
                if (value is not int castValue) return false;
                instance.m_highestUserEdgeNotified = castValue;
                return true;
            }
            case "m_userEdgeFakePathPoint":
            case "userEdgeFakePathPoint":
            {
                if (value is not hkaiPath.PathPoint castValue) return false;
                instance.m_userEdgeFakePathPoint = castValue;
                return true;
            }
            case "m_savedCharacterState":
            case "savedCharacterState":
            {
                if (value is hkaiCharacter.State castValue)
                {
                    instance.m_savedCharacterState = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_savedCharacterState = (hkaiCharacter.State)intValue;
                    return true;
                }
                return false;
            }
            case "m_cornerPredictorInitInfo":
            case "cornerPredictorInitInfo":
            {
                if (value is not hkaiEdgeFollowingBehavior.CornerPredictorInitInfo castValue) return false;
                instance.m_cornerPredictorInitInfo = castValue;
                return true;
            }
            case "m_passiveAvoidance":
            case "passiveAvoidance":
            {
                if (value is not bool castValue) return false;
                instance.m_passiveAvoidance = castValue;
                return true;
            }
            case "m_resubmitEarlyIterationTerminations":
            case "resubmitEarlyIterationTerminations":
            {
                if (value is not bool castValue) return false;
                instance.m_resubmitEarlyIterationTerminations = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
