// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbExtractRagdollPoseModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbExtractRagdollPoseModifier>
{
    public hkbExtractRagdollPoseModifierData(Havok.hk2014.hkbExtractRagdollPoseModifier instance) : base(instance) { }

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
            case "m_poseMatchingBone0":
            case "poseMatchingBone0":
            {
                value = instance.m_poseMatchingBone0;
                return true;
            }
            case "m_poseMatchingBone1":
            case "poseMatchingBone1":
            {
                value = instance.m_poseMatchingBone1;
                return true;
            }
            case "m_poseMatchingBone2":
            case "poseMatchingBone2":
            {
                value = instance.m_poseMatchingBone2;
                return true;
            }
            case "m_enableComputeWorldFromModel":
            case "enableComputeWorldFromModel":
            {
                value = instance.m_enableComputeWorldFromModel;
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
            case "m_poseMatchingBone0":
            case "poseMatchingBone0":
            {
                if (value is not short castValue) return false;
                instance.m_poseMatchingBone0 = castValue;
                return true;
            }
            case "m_poseMatchingBone1":
            case "poseMatchingBone1":
            {
                if (value is not short castValue) return false;
                instance.m_poseMatchingBone1 = castValue;
                return true;
            }
            case "m_poseMatchingBone2":
            case "poseMatchingBone2":
            {
                if (value is not short castValue) return false;
                instance.m_poseMatchingBone2 = castValue;
                return true;
            }
            case "m_enableComputeWorldFromModel":
            case "enableComputeWorldFromModel":
            {
                if (value is not bool castValue) return false;
                instance.m_enableComputeWorldFromModel = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
