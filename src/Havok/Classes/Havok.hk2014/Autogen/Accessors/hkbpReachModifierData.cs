// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbpReachModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbpReachModifier>
{
    public hkbpReachModifierData(Havok.hk2014.hkbpReachModifier instance) : base(instance) { }

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
            case "m_hands":
            case "hands":
            {
                value = instance.m_hands;
                return true;
            }
            case "m_newTargetGain":
            case "newTargetGain":
            {
                value = instance.m_newTargetGain;
                return true;
            }
            case "m_noTargetGain":
            case "noTargetGain":
            {
                value = instance.m_noTargetGain;
                return true;
            }
            case "m_targetGain":
            case "targetGain":
            {
                value = instance.m_targetGain;
                return true;
            }
            case "m_fadeOutDuration":
            case "fadeOutDuration":
            {
                value = instance.m_fadeOutDuration;
                return true;
            }
            case "m_raycastLayer":
            case "raycastLayer":
            {
                value = instance.m_raycastLayer;
                return true;
            }
            case "m_sensingPropertyKey":
            case "sensingPropertyKey":
            {
                value = instance.m_sensingPropertyKey;
                return true;
            }
            case "m_reachMode":
            case "reachMode":
            {
                value = instance.m_reachMode;
                return true;
            }
            case "m_ignoreMySystemGroup":
            case "ignoreMySystemGroup":
            {
                value = instance.m_ignoreMySystemGroup;
                return true;
            }
            case "m_extrapolate":
            case "extrapolate":
            {
                value = instance.m_extrapolate;
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
            case "m_hands":
            case "hands":
            {
                if (value is not List<Havok.hk2014.hkbpReachModifierHand> castValue) return false;
                instance.m_hands = castValue;
                return true;
            }
            case "m_newTargetGain":
            case "newTargetGain":
            {
                if (value is not float castValue) return false;
                instance.m_newTargetGain = castValue;
                return true;
            }
            case "m_noTargetGain":
            case "noTargetGain":
            {
                if (value is not float castValue) return false;
                instance.m_noTargetGain = castValue;
                return true;
            }
            case "m_targetGain":
            case "targetGain":
            {
                if (value is not float castValue) return false;
                instance.m_targetGain = castValue;
                return true;
            }
            case "m_fadeOutDuration":
            case "fadeOutDuration":
            {
                if (value is not float castValue) return false;
                instance.m_fadeOutDuration = castValue;
                return true;
            }
            case "m_raycastLayer":
            case "raycastLayer":
            {
                if (value is not int castValue) return false;
                instance.m_raycastLayer = castValue;
                return true;
            }
            case "m_sensingPropertyKey":
            case "sensingPropertyKey":
            {
                if (value is not uint castValue) return false;
                instance.m_sensingPropertyKey = castValue;
                return true;
            }
            case "m_reachMode":
            case "reachMode":
            {
                if (value is Havok.hk2014.hkbpReachModifier.ReachMode castValue)
                {
                    instance.m_reachMode = castValue;
                    return true;
                }
                if (value is sbyte sbyteValue)
                {
                    instance.m_reachMode = (Havok.hk2014.hkbpReachModifier.ReachMode)sbyteValue;
                    return true;
                }
                return false;
            }
            case "m_ignoreMySystemGroup":
            case "ignoreMySystemGroup":
            {
                if (value is not bool castValue) return false;
                instance.m_ignoreMySystemGroup = castValue;
                return true;
            }
            case "m_extrapolate":
            case "extrapolate":
            {
                if (value is not float castValue) return false;
                instance.m_extrapolate = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
