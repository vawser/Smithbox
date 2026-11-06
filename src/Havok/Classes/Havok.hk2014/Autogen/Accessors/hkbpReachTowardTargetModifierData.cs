// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbpReachTowardTargetModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbpReachTowardTargetModifier>
{
    public hkbpReachTowardTargetModifierData(Havok.hk2014.hkbpReachTowardTargetModifier instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
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
            case "m_leftHand":
            case "leftHand":
            {
                value = instance.m_leftHand;
                return true;
            }
            case "m_rightHand":
            case "rightHand":
            {
                value = instance.m_rightHand;
                return true;
            }
            case "m_targetIn":
            case "targetIn":
            {
                value = instance.m_targetIn;
                return true;
            }
            case "m_distanceBetweenHands":
            case "distanceBetweenHands":
            {
                value = instance.m_distanceBetweenHands;
                return true;
            }
            case "m_reachDistance":
            case "reachDistance":
            {
                value = instance.m_reachDistance;
                return true;
            }
            case "m_fadeInGainSpeed":
            case "fadeInGainSpeed":
            {
                value = instance.m_fadeInGainSpeed;
                return true;
            }
            case "m_fadeOutGainSpeed":
            case "fadeOutGainSpeed":
            {
                value = instance.m_fadeOutGainSpeed;
                return true;
            }
            case "m_fadeOutDuration":
            case "fadeOutDuration":
            {
                value = instance.m_fadeOutDuration;
                return true;
            }
            case "m_targetChangeSpeed":
            case "targetChangeSpeed":
            {
                value = instance.m_targetChangeSpeed;
                return true;
            }
            case "m_holdTarget":
            case "holdTarget":
            {
                value = instance.m_holdTarget;
                return true;
            }
            case "m_reachPastTarget":
            case "reachPastTarget":
            {
                value = instance.m_reachPastTarget;
                return true;
            }
            case "m_giveUpIfNoTarget":
            case "giveUpIfNoTarget":
            {
                value = instance.m_giveUpIfNoTarget;
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
            case "m_variableBindingSet":
            case "variableBindingSet":
            {
                if (value is null)
                {
                    instance.m_variableBindingSet = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbVariableBindingSet castValue)
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
            case "m_leftHand":
            case "leftHand":
            {
                if (value is not Havok.hk2014.hkbpReachTowardTargetModifierHand castValue) return false;
                instance.m_leftHand = castValue;
                return true;
            }
            case "m_rightHand":
            case "rightHand":
            {
                if (value is not Havok.hk2014.hkbpReachTowardTargetModifierHand castValue) return false;
                instance.m_rightHand = castValue;
                return true;
            }
            case "m_targetIn":
            case "targetIn":
            {
                if (value is null)
                {
                    instance.m_targetIn = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbpTarget castValue)
                {
                    instance.m_targetIn = castValue;
                    return true;
                }
                return false;
            }
            case "m_distanceBetweenHands":
            case "distanceBetweenHands":
            {
                if (value is not float castValue) return false;
                instance.m_distanceBetweenHands = castValue;
                return true;
            }
            case "m_reachDistance":
            case "reachDistance":
            {
                if (value is not float castValue) return false;
                instance.m_reachDistance = castValue;
                return true;
            }
            case "m_fadeInGainSpeed":
            case "fadeInGainSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_fadeInGainSpeed = castValue;
                return true;
            }
            case "m_fadeOutGainSpeed":
            case "fadeOutGainSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_fadeOutGainSpeed = castValue;
                return true;
            }
            case "m_fadeOutDuration":
            case "fadeOutDuration":
            {
                if (value is not float castValue) return false;
                instance.m_fadeOutDuration = castValue;
                return true;
            }
            case "m_targetChangeSpeed":
            case "targetChangeSpeed":
            {
                if (value is not float castValue) return false;
                instance.m_targetChangeSpeed = castValue;
                return true;
            }
            case "m_holdTarget":
            case "holdTarget":
            {
                if (value is not bool castValue) return false;
                instance.m_holdTarget = castValue;
                return true;
            }
            case "m_reachPastTarget":
            case "reachPastTarget":
            {
                if (value is not bool castValue) return false;
                instance.m_reachPastTarget = castValue;
                return true;
            }
            case "m_giveUpIfNoTarget":
            case "giveUpIfNoTarget":
            {
                if (value is not bool castValue) return false;
                instance.m_giveUpIfNoTarget = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
