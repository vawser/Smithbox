// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbLayerData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbLayer>
{
    public hkbLayerData(Havok.hk2014.hkbLayer instance) : base(instance) { }

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
            case "m_generator":
            case "generator":
            {
                value = instance.m_generator;
                return true;
            }
            case "m_weight":
            case "weight":
            {
                value = instance.m_weight;
                return true;
            }
            case "m_boneWeights":
            case "boneWeights":
            {
                value = instance.m_boneWeights;
                return true;
            }
            case "m_fadeInDuration":
            case "fadeInDuration":
            {
                value = instance.m_fadeInDuration;
                return true;
            }
            case "m_fadeOutDuration":
            case "fadeOutDuration":
            {
                value = instance.m_fadeOutDuration;
                return true;
            }
            case "m_onEventId":
            case "onEventId":
            {
                value = instance.m_onEventId;
                return true;
            }
            case "m_offEventId":
            case "offEventId":
            {
                value = instance.m_offEventId;
                return true;
            }
            case "m_onByDefault":
            case "onByDefault":
            {
                value = instance.m_onByDefault;
                return true;
            }
            case "m_useMotion":
            case "useMotion":
            {
                value = instance.m_useMotion;
                return true;
            }
            case "m_forceFullFadeDurations":
            case "forceFullFadeDurations":
            {
                value = instance.m_forceFullFadeDurations;
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
            case "m_generator":
            case "generator":
            {
                if (value is null)
                {
                    instance.m_generator = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbGenerator castValue)
                {
                    instance.m_generator = castValue;
                    return true;
                }
                return false;
            }
            case "m_weight":
            case "weight":
            {
                if (value is not float castValue) return false;
                instance.m_weight = castValue;
                return true;
            }
            case "m_boneWeights":
            case "boneWeights":
            {
                if (value is null)
                {
                    instance.m_boneWeights = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbBoneWeightArray castValue)
                {
                    instance.m_boneWeights = castValue;
                    return true;
                }
                return false;
            }
            case "m_fadeInDuration":
            case "fadeInDuration":
            {
                if (value is not float castValue) return false;
                instance.m_fadeInDuration = castValue;
                return true;
            }
            case "m_fadeOutDuration":
            case "fadeOutDuration":
            {
                if (value is not float castValue) return false;
                instance.m_fadeOutDuration = castValue;
                return true;
            }
            case "m_onEventId":
            case "onEventId":
            {
                if (value is not int castValue) return false;
                instance.m_onEventId = castValue;
                return true;
            }
            case "m_offEventId":
            case "offEventId":
            {
                if (value is not int castValue) return false;
                instance.m_offEventId = castValue;
                return true;
            }
            case "m_onByDefault":
            case "onByDefault":
            {
                if (value is not bool castValue) return false;
                instance.m_onByDefault = castValue;
                return true;
            }
            case "m_useMotion":
            case "useMotion":
            {
                if (value is not bool castValue) return false;
                instance.m_useMotion = castValue;
                return true;
            }
            case "m_forceFullFadeDurations":
            case "forceFullFadeDurations":
            {
                if (value is not bool castValue) return false;
                instance.m_forceFullFadeDurations = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
