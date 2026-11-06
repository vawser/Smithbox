// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbpBalanceModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbpBalanceModifier>
{
    public hkbpBalanceModifierData(Havok.hk2014.hkbpBalanceModifier instance) : base(instance) { }

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
            case "m_giveUp":
            case "giveUp":
            {
                value = instance.m_giveUp;
                return true;
            }
            case "m_comDistThreshold":
            case "comDistThreshold":
            {
                value = instance.m_comDistThreshold;
                return true;
            }
            case "m_passThrough":
            case "passThrough":
            {
                value = instance.m_passThrough;
                return true;
            }
            case "m_ragdollLeftFootBoneIndex":
            case "ragdollLeftFootBoneIndex":
            {
                value = instance.m_ragdollLeftFootBoneIndex;
                return true;
            }
            case "m_ragdollRightFootBoneIndex":
            case "ragdollRightFootBoneIndex":
            {
                value = instance.m_ragdollRightFootBoneIndex;
                return true;
            }
            case "m_balanceOnAnklesFraction":
            case "balanceOnAnklesFraction":
            {
                value = instance.m_balanceOnAnklesFraction;
                return true;
            }
            case "m_upAxis":
            case "upAxis":
            {
                value = instance.m_upAxis;
                return true;
            }
            case "m_fadeInTime":
            case "fadeInTime":
            {
                value = instance.m_fadeInTime;
                return true;
            }
            case "m_comBiasX":
            case "comBiasX":
            {
                value = instance.m_comBiasX;
                return true;
            }
            case "m_stepInfo":
            case "stepInfo":
            {
                value = instance.m_stepInfo;
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
            case "m_giveUp":
            case "giveUp":
            {
                if (value is not bool castValue) return false;
                instance.m_giveUp = castValue;
                return true;
            }
            case "m_comDistThreshold":
            case "comDistThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_comDistThreshold = castValue;
                return true;
            }
            case "m_passThrough":
            case "passThrough":
            {
                if (value is not bool castValue) return false;
                instance.m_passThrough = castValue;
                return true;
            }
            case "m_ragdollLeftFootBoneIndex":
            case "ragdollLeftFootBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_ragdollLeftFootBoneIndex = castValue;
                return true;
            }
            case "m_ragdollRightFootBoneIndex":
            case "ragdollRightFootBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_ragdollRightFootBoneIndex = castValue;
                return true;
            }
            case "m_balanceOnAnklesFraction":
            case "balanceOnAnklesFraction":
            {
                if (value is not float castValue) return false;
                instance.m_balanceOnAnklesFraction = castValue;
                return true;
            }
            case "m_upAxis":
            case "upAxis":
            {
                if (value is not int castValue) return false;
                instance.m_upAxis = castValue;
                return true;
            }
            case "m_fadeInTime":
            case "fadeInTime":
            {
                if (value is not float castValue) return false;
                instance.m_fadeInTime = castValue;
                return true;
            }
            case "m_comBiasX":
            case "comBiasX":
            {
                if (value is not float castValue) return false;
                instance.m_comBiasX = castValue;
                return true;
            }
            case "m_stepInfo":
            case "stepInfo":
            {
                if (value is not List<Havok.hk2014.hkbpBalanceModifierStepInfo> castValue) return false;
                instance.m_stepInfo = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
