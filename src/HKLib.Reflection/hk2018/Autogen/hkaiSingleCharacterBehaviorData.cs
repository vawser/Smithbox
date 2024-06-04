// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiSingleCharacterBehaviorData : HavokData<hkaiSingleCharacterBehavior> 
{
    public hkaiSingleCharacterBehaviorData(HavokType type, hkaiSingleCharacterBehavior instance) : base(type, instance) {}

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
            default:
            return false;
        }
    }

}
