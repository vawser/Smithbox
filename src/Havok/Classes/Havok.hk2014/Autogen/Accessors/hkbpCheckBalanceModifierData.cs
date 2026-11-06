// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbpCheckBalanceModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbpCheckBalanceModifier>
{
    public hkbpCheckBalanceModifierData(Havok.hk2014.hkbpCheckBalanceModifier instance) : base(instance) { }

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
            case "m_eventToSendWhenOffBalance":
            case "eventToSendWhenOffBalance":
            {
                value = instance.m_eventToSendWhenOffBalance;
                return true;
            }
            case "m_offBalanceEventThreshold":
            case "offBalanceEventThreshold":
            {
                value = instance.m_offBalanceEventThreshold;
                return true;
            }
            case "m_worldUpAxisIndex":
            case "worldUpAxisIndex":
            {
                value = instance.m_worldUpAxisIndex;
                return true;
            }
            case "m_comBiasX":
            case "comBiasX":
            {
                value = instance.m_comBiasX;
                return true;
            }
            case "m_extractRagdollPose":
            case "extractRagdollPose":
            {
                value = instance.m_extractRagdollPose;
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
            case "m_eventToSendWhenOffBalance":
            case "eventToSendWhenOffBalance":
            {
                if (value is not Havok.hk2014.hkbEvent castValue) return false;
                instance.m_eventToSendWhenOffBalance = castValue;
                return true;
            }
            case "m_offBalanceEventThreshold":
            case "offBalanceEventThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_offBalanceEventThreshold = castValue;
                return true;
            }
            case "m_worldUpAxisIndex":
            case "worldUpAxisIndex":
            {
                if (value is not int castValue) return false;
                instance.m_worldUpAxisIndex = castValue;
                return true;
            }
            case "m_comBiasX":
            case "comBiasX":
            {
                if (value is not float castValue) return false;
                instance.m_comBiasX = castValue;
                return true;
            }
            case "m_extractRagdollPose":
            case "extractRagdollPose":
            {
                if (value is not bool castValue) return false;
                instance.m_extractRagdollPose = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
