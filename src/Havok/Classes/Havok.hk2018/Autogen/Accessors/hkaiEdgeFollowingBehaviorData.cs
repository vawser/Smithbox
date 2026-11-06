// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiEdgeFollowingBehaviorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiEdgeFollowingBehavior>
{
    public hkaiEdgeFollowingBehaviorData(Havok.hk2018.hkaiEdgeFollowingBehavior instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_world":
            case "world":
            {
                value = instance.m_world;
                return true;
            }
            case "m_character":
            case "character":
            {
                value = instance.m_character;
                return true;
            }
            case "m_callbackType":
            case "callbackType":
            {
                value = instance.m_callbackType;
                return true;
            }
            case "m_immediateNavMeshRequest":
            case "immediateNavMeshRequest":
            {
                value = instance.m_immediateNavMeshRequest;
                return true;
            }
            case "m_immediateNavVolumeRequest":
            case "immediateNavVolumeRequest":
            {
                value = instance.m_immediateNavVolumeRequest;
                return true;
            }
            case "m_requestedGoalPoints":
            case "requestedGoalPoints":
            {
                value = instance.m_requestedGoalPoints;
                return true;
            }
            case "m_currentGoalIndex":
            case "currentGoalIndex":
            {
                value = instance.m_currentGoalIndex;
                return true;
            }
            case "m_updateQuerySize":
            case "updateQuerySize":
            {
                value = instance.m_updateQuerySize;
                return true;
            }
            case "m_characterRadiusMultiplier":
            case "characterRadiusMultiplier":
            {
                value = instance.m_characterRadiusMultiplier;
                return true;
            }
            case "m_maxIgnoredHeight":
            case "maxIgnoredHeight":
            {
                value = instance.m_maxIgnoredHeight;
                return true;
            }
            case "m_edgePath":
            case "edgePath":
            {
                value = instance.m_edgePath;
                return true;
            }
            case "m_traversalState":
            case "traversalState":
            {
                value = instance.m_traversalState;
                return true;
            }
            case "m_newCharacterState":
            case "newCharacterState":
            {
                value = instance.m_newCharacterState;
                return true;
            }
            case "m_pathFollowingProperties":
            case "pathFollowingProperties":
            {
                value = instance.m_pathFollowingProperties;
                return true;
            }
            case "m_highestUserEdgeNotified":
            case "highestUserEdgeNotified":
            {
                value = instance.m_highestUserEdgeNotified;
                return true;
            }
            case "m_userEdgeFakePathPoint":
            case "userEdgeFakePathPoint":
            {
                value = instance.m_userEdgeFakePathPoint;
                return true;
            }
            case "m_savedCharacterState":
            case "savedCharacterState":
            {
                value = instance.m_savedCharacterState;
                return true;
            }
            case "m_cornerPredictorInitInfo":
            case "cornerPredictorInitInfo":
            {
                value = instance.m_cornerPredictorInitInfo;
                return true;
            }
            case "m_passiveAvoidance":
            case "passiveAvoidance":
            {
                value = instance.m_passiveAvoidance;
                return true;
            }
            case "m_resubmitEarlyIterationTerminations":
            case "resubmitEarlyIterationTerminations":
            {
                value = instance.m_resubmitEarlyIterationTerminations;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
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
                if (value is Havok.hk2018.hkaiWorld castValue)
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
                if (value is Havok.hk2018.hkaiCharacter castValue)
                {
                    instance.m_character = castValue;
                    return true;
                }
                return false;
            }
            case "m_callbackType":
            case "callbackType":
            {
                if (value is Havok.hk2018.hkaiCharacterUtil.CallbackType castValue)
                {
                    instance.m_callbackType = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_callbackType = (Havok.hk2018.hkaiCharacterUtil.CallbackType)intValue;
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
                if (value is Havok.hk2018.hkaiNavMeshPathRequestInfo castValue)
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
                if (value is Havok.hk2018.hkaiNavVolumePathRequestInfo castValue)
                {
                    instance.m_immediateNavVolumeRequest = castValue;
                    return true;
                }
                return false;
            }
            case "m_requestedGoalPoints":
            case "requestedGoalPoints":
            {
                if (value is not List<Havok.hk2018.hkaiSingleCharacterBehavior.RequestedGoalPoint> castValue) return false;
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
                if (value is Havok.hk2018.hkaiEdgePath castValue)
                {
                    instance.m_edgePath = castValue;
                    return true;
                }
                return false;
            }
            case "m_traversalState":
            case "traversalState":
            {
                if (value is not Havok.hk2018.hkaiEdgePath.TraversalState castValue) return false;
                instance.m_traversalState = castValue;
                return true;
            }
            case "m_newCharacterState":
            case "newCharacterState":
            {
                if (value is Havok.hk2018.hkaiCharacter.State castValue)
                {
                    instance.m_newCharacterState = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_newCharacterState = (Havok.hk2018.hkaiCharacter.State)intValue;
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
                if (value is Havok.hk2018.hkaiPathFollowingProperties castValue)
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
                if (value is not Havok.hk2018.hkaiPath.PathPoint castValue) return false;
                instance.m_userEdgeFakePathPoint = castValue;
                return true;
            }
            case "m_savedCharacterState":
            case "savedCharacterState":
            {
                if (value is Havok.hk2018.hkaiCharacter.State castValue)
                {
                    instance.m_savedCharacterState = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_savedCharacterState = (Havok.hk2018.hkaiCharacter.State)intValue;
                    return true;
                }
                return false;
            }
            case "m_cornerPredictorInitInfo":
            case "cornerPredictorInitInfo":
            {
                if (value is not Havok.hk2018.hkaiEdgeFollowingBehavior.CornerPredictorInitInfo castValue) return false;
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
