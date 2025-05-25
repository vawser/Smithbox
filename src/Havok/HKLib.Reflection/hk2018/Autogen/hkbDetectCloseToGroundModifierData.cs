// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbDetectCloseToGroundModifierData : HavokData<hkbDetectCloseToGroundModifier> 
{
    public hkbDetectCloseToGroundModifierData(HavokType type, hkbDetectCloseToGroundModifier instance) : base(type, instance) {}

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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (instance.m_variableBindingSet is null)
                {
                    return true;
                }
                if (instance.m_variableBindingSet is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (instance.m_name is null)
                {
                    return true;
                }
                if (instance.m_name is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_enable":
            case "enable":
            {
                if (instance.m_enable is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_closeToGroundEvent":
            case "closeToGroundEvent":
            {
                if (instance.m_closeToGroundEvent is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_closeToGroundHeight":
            case "closeToGroundHeight":
            {
                if (instance.m_closeToGroundHeight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_raycastDistanceDown":
            case "raycastDistanceDown":
            {
                if (instance.m_raycastDistanceDown is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                if (instance.m_collisionFilterInfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_boneIndex":
            case "boneIndex":
            {
                if (instance.m_boneIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_animBoneIndex":
            case "animBoneIndex":
            {
                if (instance.m_animBoneIndex is not TGet castValue) return false;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is hkbVariableBindingSet castValue)
                {
                    instance.m_variableBindingSet = castValue;
                    return true;
                }
                return false;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_name":
            case "name":
            {
                if (value is null)
                {
                    instance.m_name = default;
                    return true;
                }
                if (value is string castValue)
                {
                    instance.m_name = castValue;
                    return true;
                }
                return false;
            }
            case "m_enable":
            case "enable":
            {
                if (value is not bool castValue) return false;
                instance.m_enable = castValue;
                return true;
            }
            case "m_closeToGroundEvent":
            case "closeToGroundEvent":
            {
                if (value is not hkbEventProperty castValue) return false;
                instance.m_closeToGroundEvent = castValue;
                return true;
            }
            case "m_closeToGroundHeight":
            case "closeToGroundHeight":
            {
                if (value is not float castValue) return false;
                instance.m_closeToGroundHeight = castValue;
                return true;
            }
            case "m_raycastDistanceDown":
            case "raycastDistanceDown":
            {
                if (value is not float castValue) return false;
                instance.m_raycastDistanceDown = castValue;
                return true;
            }
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                if (value is not uint castValue) return false;
                instance.m_collisionFilterInfo = castValue;
                return true;
            }
            case "m_boneIndex":
            case "boneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_boneIndex = castValue;
                return true;
            }
            case "m_animBoneIndex":
            case "animBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_animBoneIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
