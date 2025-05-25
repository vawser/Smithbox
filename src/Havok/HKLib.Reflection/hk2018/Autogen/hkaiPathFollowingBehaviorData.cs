// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiPathFollowingBehaviorData : HavokData<hkaiPathFollowingBehavior> 
{
    public hkaiPathFollowingBehaviorData(HavokType type, hkaiPathFollowingBehavior instance) : base(type, instance) {}

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
            case "m_currentPath":
            case "currentPath":
            {
                if (instance.m_currentPath is null)
                {
                    return true;
                }
                if (instance.m_currentPath is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_currentPathFixed":
            case "currentPathFixed":
            {
                if (instance.m_currentPathFixed is null)
                {
                    return true;
                }
                if (instance.m_currentPathFixed is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_currentPathSegment":
            case "currentPathSegment":
            {
                if (instance.m_currentPathSegment is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_previousPathSegment":
            case "previousPathSegment":
            {
                if (instance.m_previousPathSegment is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_newCharacterState":
            case "newCharacterState":
            {
                if (instance.m_newCharacterState is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_changeSegmentDistance":
            case "changeSegmentDistance":
            {
                if (instance.m_changeSegmentDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_tempChangeSegmentDistance":
            case "tempChangeSegmentDistance":
            {
                if (instance.m_tempChangeSegmentDistance is not TGet castValue) return false;
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
            case "m_characterToPathStartThreshold":
            case "characterToPathStartThreshold":
            {
                if (instance.m_characterToPathStartThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_useSectionLocalPaths":
            case "useSectionLocalPaths":
            {
                if (instance.m_useSectionLocalPaths is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pathType":
            case "pathType":
            {
                if (instance.m_pathType is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_pathType is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_lastPointIsGoal":
            case "lastPointIsGoal":
            {
                if (instance.m_lastPointIsGoal is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_needsRepath":
            case "needsRepath":
            {
                if (instance.m_needsRepath is not TGet castValue) return false;
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
            case "m_savedCharacterState":
            case "savedCharacterState":
            {
                if (instance.m_savedCharacterState is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_savedCharacterState is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
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
            case "m_currentPath":
            case "currentPath":
            {
                if (value is null)
                {
                    instance.m_currentPath = default;
                    return true;
                }
                if (value is hkaiPath castValue)
                {
                    instance.m_currentPath = castValue;
                    return true;
                }
                return false;
            }
            case "m_currentPathFixed":
            case "currentPathFixed":
            {
                if (value is null)
                {
                    instance.m_currentPathFixed = default;
                    return true;
                }
                if (value is hkaiPath castValue)
                {
                    instance.m_currentPathFixed = castValue;
                    return true;
                }
                return false;
            }
            case "m_currentPathSegment":
            case "currentPathSegment":
            {
                if (value is not int castValue) return false;
                instance.m_currentPathSegment = castValue;
                return true;
            }
            case "m_previousPathSegment":
            case "previousPathSegment":
            {
                if (value is not int castValue) return false;
                instance.m_previousPathSegment = castValue;
                return true;
            }
            case "m_newCharacterState":
            case "newCharacterState":
            {
                if (value is not int castValue) return false;
                instance.m_newCharacterState = castValue;
                return true;
            }
            case "m_changeSegmentDistance":
            case "changeSegmentDistance":
            {
                if (value is not float castValue) return false;
                instance.m_changeSegmentDistance = castValue;
                return true;
            }
            case "m_tempChangeSegmentDistance":
            case "tempChangeSegmentDistance":
            {
                if (value is not float castValue) return false;
                instance.m_tempChangeSegmentDistance = castValue;
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
            case "m_characterToPathStartThreshold":
            case "characterToPathStartThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_characterToPathStartThreshold = castValue;
                return true;
            }
            case "m_useSectionLocalPaths":
            case "useSectionLocalPaths":
            {
                if (value is not bool castValue) return false;
                instance.m_useSectionLocalPaths = castValue;
                return true;
            }
            case "m_pathType":
            case "pathType":
            {
                if (value is hkaiPathFollowingBehavior.PathType castValue)
                {
                    instance.m_pathType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_pathType = (hkaiPathFollowingBehavior.PathType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_lastPointIsGoal":
            case "lastPointIsGoal":
            {
                if (value is not bool castValue) return false;
                instance.m_lastPointIsGoal = castValue;
                return true;
            }
            case "m_needsRepath":
            case "needsRepath":
            {
                if (value is not bool castValue) return false;
                instance.m_needsRepath = castValue;
                return true;
            }
            case "m_passiveAvoidance":
            case "passiveAvoidance":
            {
                if (value is not bool castValue) return false;
                instance.m_passiveAvoidance = castValue;
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
                if (value is byte byteValue)
                {
                    instance.m_savedCharacterState = (hkaiCharacter.State)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
