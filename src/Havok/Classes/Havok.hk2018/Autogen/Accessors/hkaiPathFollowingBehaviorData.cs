// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiPathFollowingBehaviorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiPathFollowingBehavior>
{
    public hkaiPathFollowingBehaviorData(Havok.hk2018.hkaiPathFollowingBehavior instance) : base(instance) { }

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
            case "m_pathFollowingProperties":
            case "pathFollowingProperties":
            {
                value = instance.m_pathFollowingProperties;
                return true;
            }
            case "m_currentPath":
            case "currentPath":
            {
                value = instance.m_currentPath;
                return true;
            }
            case "m_currentPathFixed":
            case "currentPathFixed":
            {
                value = instance.m_currentPathFixed;
                return true;
            }
            case "m_currentPathSegment":
            case "currentPathSegment":
            {
                value = instance.m_currentPathSegment;
                return true;
            }
            case "m_previousPathSegment":
            case "previousPathSegment":
            {
                value = instance.m_previousPathSegment;
                return true;
            }
            case "m_newCharacterState":
            case "newCharacterState":
            {
                value = instance.m_newCharacterState;
                return true;
            }
            case "m_changeSegmentDistance":
            case "changeSegmentDistance":
            {
                value = instance.m_changeSegmentDistance;
                return true;
            }
            case "m_tempChangeSegmentDistance":
            case "tempChangeSegmentDistance":
            {
                value = instance.m_tempChangeSegmentDistance;
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
            case "m_characterToPathStartThreshold":
            case "characterToPathStartThreshold":
            {
                value = instance.m_characterToPathStartThreshold;
                return true;
            }
            case "m_useSectionLocalPaths":
            case "useSectionLocalPaths":
            {
                value = instance.m_useSectionLocalPaths;
                return true;
            }
            case "m_pathType":
            case "pathType":
            {
                value = instance.m_pathType;
                return true;
            }
            case "m_lastPointIsGoal":
            case "lastPointIsGoal":
            {
                value = instance.m_lastPointIsGoal;
                return true;
            }
            case "m_needsRepath":
            case "needsRepath":
            {
                value = instance.m_needsRepath;
                return true;
            }
            case "m_passiveAvoidance":
            case "passiveAvoidance":
            {
                value = instance.m_passiveAvoidance;
                return true;
            }
            case "m_savedCharacterState":
            case "savedCharacterState":
            {
                value = instance.m_savedCharacterState;
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
            case "m_currentPath":
            case "currentPath":
            {
                if (value is null)
                {
                    instance.m_currentPath = default;
                    return true;
                }
                if (value is Havok.hk2018.hkaiPath castValue)
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
                if (value is Havok.hk2018.hkaiPath castValue)
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
                if (value is Havok.hk2018.hkaiPathFollowingBehavior.PathType castValue)
                {
                    instance.m_pathType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_pathType = (Havok.hk2018.hkaiPathFollowingBehavior.PathType)byteValue;
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
                if (value is Havok.hk2018.hkaiCharacter.State castValue)
                {
                    instance.m_savedCharacterState = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_savedCharacterState = (Havok.hk2018.hkaiCharacter.State)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
