// Automatically Generated

#nullable enable

namespace Havok.hk2016.Accessors;

internal sealed class hkbFootIkDriverInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2016.hkbFootIkDriverInfo>
{
    public hkbFootIkDriverInfoData(Havok.hk2016.hkbFootIkDriverInfo instance) : base(instance) { }

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
            case "m_legs":
            case "legs":
            {
                value = instance.m_legs;
                return true;
            }
            case "m_raycastDistanceUp":
            case "raycastDistanceUp":
            {
                value = instance.m_raycastDistanceUp;
                return true;
            }
            case "m_raycastDistanceDown":
            case "raycastDistanceDown":
            {
                value = instance.m_raycastDistanceDown;
                return true;
            }
            case "m_originalGroundHeightMS":
            case "originalGroundHeightMS":
            {
                value = instance.m_originalGroundHeightMS;
                return true;
            }
            case "m_verticalOffset":
            case "verticalOffset":
            {
                value = instance.m_verticalOffset;
                return true;
            }
            case "m_collisionFilterInfo":
            case "collisionFilterInfo":
            {
                value = instance.m_collisionFilterInfo;
                return true;
            }
            case "m_forwardAlignFraction":
            case "forwardAlignFraction":
            {
                value = instance.m_forwardAlignFraction;
                return true;
            }
            case "m_sidewaysAlignFraction":
            case "sidewaysAlignFraction":
            {
                value = instance.m_sidewaysAlignFraction;
                return true;
            }
            case "m_sidewaysSampleWidth":
            case "sidewaysSampleWidth":
            {
                value = instance.m_sidewaysSampleWidth;
                return true;
            }
            case "m_lockFeetWhenPlanted":
            case "lockFeetWhenPlanted":
            {
                value = instance.m_lockFeetWhenPlanted;
                return true;
            }
            case "m_useCharacterUpVector":
            case "useCharacterUpVector":
            {
                value = instance.m_useCharacterUpVector;
                return true;
            }
            case "m_isQuadrupedNarrow":
            case "isQuadrupedNarrow":
            {
                value = instance.m_isQuadrupedNarrow;
                return true;
            }
            case "m_keepSourceFootEndAboveGround":
            case "keepSourceFootEndAboveGround":
            {
                value = instance.m_keepSourceFootEndAboveGround;
                return true;
            }
            case "m_usefootEndToOnlyHeelRay":
            case "usefootEndToOnlyHeelRay":
            {
                value = instance.m_usefootEndToOnlyHeelRay;
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
                if (value is not Havok.hk2016.hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_legs":
            case "legs":
            {
                if (value is not List<Havok.hk2016.hkbFootIkDriverInfo.Leg> castValue) return false;
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
            case "m_usefootEndToOnlyHeelRay":
            case "usefootEndToOnlyHeelRay":
            {
                if (value is not bool castValue) return false;
                instance.m_usefootEndToOnlyHeelRay = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
