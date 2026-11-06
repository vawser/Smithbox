// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkbFootIkModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkbFootIkModifier>
{
    public hkbFootIkModifierData(Havok.hk2018.hkbFootIkModifier instance) : base(instance) { }

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
            case "m_gains":
            case "gains":
            {
                value = instance.m_gains;
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
            case "m_errorOut":
            case "errorOut":
            {
                value = instance.m_errorOut;
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
            case "m_useTrackData":
            case "useTrackData":
            {
                value = instance.m_useTrackData;
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
            case "m_keepSourceFootEndAboveGround":
            case "keepSourceFootEndAboveGround":
            {
                value = instance.m_keepSourceFootEndAboveGround;
                return true;
            }
            case "m_alignMode":
            case "alignMode":
            {
                value = instance.m_alignMode;
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
            case "m_gains":
            case "gains":
            {
                if (value is not Havok.hk2018.hkbFootIkGains castValue) return false;
                instance.m_gains = castValue;
                return true;
            }
            case "m_legs":
            case "legs":
            {
                if (value is not List<Havok.hk2018.hkbFootIkModifier.Leg> castValue) return false;
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
            case "m_errorOut":
            case "errorOut":
            {
                if (value is not float castValue) return false;
                instance.m_errorOut = castValue;
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
            case "m_useTrackData":
            case "useTrackData":
            {
                if (value is not bool castValue) return false;
                instance.m_useTrackData = castValue;
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
            case "m_keepSourceFootEndAboveGround":
            case "keepSourceFootEndAboveGround":
            {
                if (value is not bool castValue) return false;
                instance.m_keepSourceFootEndAboveGround = castValue;
                return true;
            }
            case "m_alignMode":
            case "alignMode":
            {
                if (value is Havok.hk2018.hkbFootIkModifier.AlignMode castValue)
                {
                    instance.m_alignMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_alignMode = (Havok.hk2018.hkbFootIkModifier.AlignMode)sbyteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
