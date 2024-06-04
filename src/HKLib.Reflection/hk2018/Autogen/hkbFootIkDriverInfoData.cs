// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbFootIkDriverInfoData : HavokData<hkbFootIkDriverInfo> 
{
    public hkbFootIkDriverInfoData(HavokType type, hkbFootIkDriverInfo instance) : base(type, instance) {}

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
            case "m_legs":
            case "legs":
            {
                if (instance.m_legs is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_raycastDistanceUp":
            case "raycastDistanceUp":
            {
                if (instance.m_raycastDistanceUp is not TGet castValue) return false;
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
            case "m_originalGroundHeightMS":
            case "originalGroundHeightMS":
            {
                if (instance.m_originalGroundHeightMS is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_verticalOffset":
            case "verticalOffset":
            {
                if (instance.m_verticalOffset is not TGet castValue) return false;
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
            case "m_forwardAlignFraction":
            case "forwardAlignFraction":
            {
                if (instance.m_forwardAlignFraction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sidewaysAlignFraction":
            case "sidewaysAlignFraction":
            {
                if (instance.m_sidewaysAlignFraction is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sidewaysSampleWidth":
            case "sidewaysSampleWidth":
            {
                if (instance.m_sidewaysSampleWidth is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lockFeetWhenPlanted":
            case "lockFeetWhenPlanted":
            {
                if (instance.m_lockFeetWhenPlanted is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_useCharacterUpVector":
            case "useCharacterUpVector":
            {
                if (instance.m_useCharacterUpVector is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isQuadrupedNarrow":
            case "isQuadrupedNarrow":
            {
                if (instance.m_isQuadrupedNarrow is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_keepSourceFootEndAboveGround":
            case "keepSourceFootEndAboveGround":
            {
                if (instance.m_keepSourceFootEndAboveGround is not TGet castValue) return false;
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
            case "m_legs":
            case "legs":
            {
                if (value is not List<hkbFootIkDriverInfo.Leg> castValue) return false;
                instance.m_legs = castValue;
                return true;
            }
            case "m_raycastDistanceUp":
            case "raycastDistanceUp":
            {
                if (value is not float castValue) return false;
                instance.m_raycastDistanceUp = castValue;
                return true;
            }
            case "m_raycastDistanceDown":
            case "raycastDistanceDown":
            {
                if (value is not float castValue) return false;
                instance.m_raycastDistanceDown = castValue;
                return true;
            }
            case "m_originalGroundHeightMS":
            case "originalGroundHeightMS":
            {
                if (value is not float castValue) return false;
                instance.m_originalGroundHeightMS = castValue;
                return true;
            }
            case "m_verticalOffset":
            case "verticalOffset":
            {
                if (value is not float castValue) return false;
                instance.m_verticalOffset = castValue;
                return true;
            }
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                if (value is not uint castValue) return false;
                instance.m_collisionFilterInfo = castValue;
                return true;
            }
            case "m_forwardAlignFraction":
            case "forwardAlignFraction":
            {
                if (value is not float castValue) return false;
                instance.m_forwardAlignFraction = castValue;
                return true;
            }
            case "m_sidewaysAlignFraction":
            case "sidewaysAlignFraction":
            {
                if (value is not float castValue) return false;
                instance.m_sidewaysAlignFraction = castValue;
                return true;
            }
            case "m_sidewaysSampleWidth":
            case "sidewaysSampleWidth":
            {
                if (value is not float castValue) return false;
                instance.m_sidewaysSampleWidth = castValue;
                return true;
            }
            case "m_lockFeetWhenPlanted":
            case "lockFeetWhenPlanted":
            {
                if (value is not bool castValue) return false;
                instance.m_lockFeetWhenPlanted = castValue;
                return true;
            }
            case "m_useCharacterUpVector":
            case "useCharacterUpVector":
            {
                if (value is not bool castValue) return false;
                instance.m_useCharacterUpVector = castValue;
                return true;
            }
            case "m_isQuadrupedNarrow":
            case "isQuadrupedNarrow":
            {
                if (value is not bool castValue) return false;
                instance.m_isQuadrupedNarrow = castValue;
                return true;
            }
            case "m_keepSourceFootEndAboveGround":
            case "keepSourceFootEndAboveGround":
            {
                if (value is not bool castValue) return false;
                instance.m_keepSourceFootEndAboveGround = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
