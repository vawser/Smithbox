// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkbCharacterSetupData : HavokData<hkbCharacterSetup> 
{
    public hkbCharacterSetupData(HavokType type, hkbCharacterSetup instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_retargetingSkeletonMappers":
            case "retargetingSkeletonMappers":
            {
                if (instance.m_retargetingSkeletonMappers is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_animationSkeleton":
            case "animationSkeleton":
            {
                if (instance.m_animationSkeleton is null)
                {
                    return true;
                }
                if (instance.m_animationSkeleton is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_ragdollToAnimationSkeletonMapper":
            case "ragdollToAnimationSkeletonMapper":
            {
                if (instance.m_ragdollToAnimationSkeletonMapper is null)
                {
                    return true;
                }
                if (instance.m_ragdollToAnimationSkeletonMapper is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_animationToRagdollSkeletonMapper":
            case "animationToRagdollSkeletonMapper":
            {
                if (instance.m_animationToRagdollSkeletonMapper is null)
                {
                    return true;
                }
                if (instance.m_animationToRagdollSkeletonMapper is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_data":
            case "data":
            {
                if (instance.m_data is null)
                {
                    return true;
                }
                if (instance.m_data is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_retargetingSkeletonMappers":
            case "retargetingSkeletonMappers":
            {
                if (value is not List<hkaSkeletonMapper?> castValue) return false;
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
                if (value is hkaSkeleton castValue)
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
                if (value is hkaSkeletonMapper castValue)
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
                if (value is hkaSkeletonMapper castValue)
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
                if (value is HKLib.hk2018.hkbCharacterData castValue)
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
