// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkbCharacterSetupData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkbCharacterSetup>
{
    public hkbCharacterSetupData(Havok.hk2014.hkbCharacterSetup instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_retargetingSkeletonMappers":
            case "retargetingSkeletonMappers":
            {
                value = instance.m_retargetingSkeletonMappers;
                return true;
            }
            case "m_animationSkeleton":
            case "animationSkeleton":
            {
                value = instance.m_animationSkeleton;
                return true;
            }
            case "m_ragdollToAnimationSkeletonMapper":
            case "ragdollToAnimationSkeletonMapper":
            {
                value = instance.m_ragdollToAnimationSkeletonMapper;
                return true;
            }
            case "m_animationToRagdollSkeletonMapper":
            case "animationToRagdollSkeletonMapper":
            {
                value = instance.m_animationToRagdollSkeletonMapper;
                return true;
            }
            case "m_data":
            case "data":
            {
                value = instance.m_data;
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
            case "m_retargetingSkeletonMappers":
            case "retargetingSkeletonMappers":
            {
                if (value is not List<Havok.hk2014.hkaSkeletonMapper?> castValue) return false;
                instance.m_retargetingSkeletonMappers = castValue;
                return true;
            }
            case "m_animationSkeleton":
            case "animationSkeleton":
            {
                if (value is null)
                {
                    instance.m_animationSkeleton = default;
                    return true;
                }
                if (value is Havok.hk2014.hkaSkeleton castValue)
                {
                    instance.m_animationSkeleton = castValue;
                    return true;
                }
                return false;
            }
            case "m_ragdollToAnimationSkeletonMapper":
            case "ragdollToAnimationSkeletonMapper":
            {
                if (value is null)
                {
                    instance.m_ragdollToAnimationSkeletonMapper = default;
                    return true;
                }
                if (value is Havok.hk2014.hkaSkeletonMapper castValue)
                {
                    instance.m_ragdollToAnimationSkeletonMapper = castValue;
                    return true;
                }
                return false;
            }
            case "m_animationToRagdollSkeletonMapper":
            case "animationToRagdollSkeletonMapper":
            {
                if (value is null)
                {
                    instance.m_animationToRagdollSkeletonMapper = default;
                    return true;
                }
                if (value is Havok.hk2014.hkaSkeletonMapper castValue)
                {
                    instance.m_animationToRagdollSkeletonMapper = castValue;
                    return true;
                }
                return false;
            }
            case "m_data":
            case "data":
            {
                if (value is null)
                {
                    instance.m_data = default;
                    return true;
                }
                if (value is Havok.hk2014.hkbCharacterData castValue)
                {
                    instance.m_data = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
