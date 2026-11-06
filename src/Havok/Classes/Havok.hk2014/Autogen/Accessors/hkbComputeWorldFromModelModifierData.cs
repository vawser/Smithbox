// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbComputeWorldFromModelModifierData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbComputeWorldFromModelModifier>
{
    public hkbComputeWorldFromModelModifierData(Havok.hk2014.hkbComputeWorldFromModelModifier instance) : base(instance) { }

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
            case "m_poseMatchingRootBoneIndex":
            case "poseMatchingRootBoneIndex":
            {
                value = instance.m_poseMatchingRootBoneIndex;
                return true;
            }
            case "m_poseMatchingOtherBoneIndex":
            case "poseMatchingOtherBoneIndex":
            {
                value = instance.m_poseMatchingOtherBoneIndex;
                return true;
            }
            case "m_poseMatchingAnotherBoneIndex":
            case "poseMatchingAnotherBoneIndex":
            {
                value = instance.m_poseMatchingAnotherBoneIndex;
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
            case "m_poseMatchingRootBoneIndex":
            case "poseMatchingRootBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_poseMatchingRootBoneIndex = castValue;
                return true;
            }
            case "m_poseMatchingOtherBoneIndex":
            case "poseMatchingOtherBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_poseMatchingOtherBoneIndex = castValue;
                return true;
            }
            case "m_poseMatchingAnotherBoneIndex":
            case "poseMatchingAnotherBoneIndex":
            {
                if (value is not short castValue) return false;
                instance.m_poseMatchingAnotherBoneIndex = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
