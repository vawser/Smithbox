// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiSingleCharacterBehaviorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiSingleCharacterBehavior>
{
    public hkaiSingleCharacterBehaviorData(Havok.hk2018.hkaiSingleCharacterBehavior instance) : base(instance) { }

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
            default:
            return false;
        }
    }

}
