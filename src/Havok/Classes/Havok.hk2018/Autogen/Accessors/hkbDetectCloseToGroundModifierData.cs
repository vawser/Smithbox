// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbDetectCloseToGroundModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbDetectCloseToGroundModifier>
{
    public hkbDetectCloseToGroundModifierData(Havok.hk2018.hkbDetectCloseToGroundModifier instance) : base(instance) { }

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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                value = instance.m_variableBindingSet;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_enable":
            case "enable":
            {
                value = instance.m_enable;
                return true;
            }
            case "m_closeToGroundEvent":
            case "closeToGroundEvent":
            {
                value = instance.m_closeToGroundEvent;
                return true;
            }
            case "m_closeToGroundHeight":
            case "closeToGroundHeight":
            {
                value = instance.m_closeToGroundHeight;
                return true;
            }
            case "m_raycastDistanceDown":
            case "raycastDistanceDown":
            {
                value = instance.m_raycastDistanceDown;
                return true;
            }
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                value = instance.m_collisionFilterInfo;
                return true;
            }
            case "m_boneIndex":
            case "boneIndex":
            {
                value = instance.m_boneIndex;
                return true;
            }
            case "m_animBoneIndex":
            case "animBoneIndex":
            {
                value = instance.m_animBoneIndex;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is Havok.hk2018.hkbVariableBindingSet castValue)
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
                if (value is not Havok.hk2018.hkbEventProperty castValue) return false;
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
