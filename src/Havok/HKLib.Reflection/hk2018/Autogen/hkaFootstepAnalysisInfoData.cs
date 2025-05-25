// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaFootstepAnalysisInfoData : HavokData<hkaFootstepAnalysisInfo> 
{
    public hkaFootstepAnalysisInfoData(HavokType type, hkaFootstepAnalysisInfo instance) : base(type, instance) {}

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
            case "m_name":
            case "name":
            {
                if (instance.m_name is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nameStrike":
            case "nameStrike":
            {
                if (instance.m_nameStrike is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nameLift":
            case "nameLift":
            {
                if (instance.m_nameLift is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nameLock":
            case "nameLock":
            {
                if (instance.m_nameLock is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nameUnlock":
            case "nameUnlock":
            {
                if (instance.m_nameUnlock is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minPos":
            case "minPos":
            {
                if (instance.m_minPos is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxPos":
            case "maxPos":
            {
                if (instance.m_maxPos is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minVel":
            case "minVel":
            {
                if (instance.m_minVel is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxVel":
            case "maxVel":
            {
                if (instance.m_maxVel is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_allBonesDown":
            case "allBonesDown":
            {
                if (instance.m_allBonesDown is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_anyBonesDown":
            case "anyBonesDown":
            {
                if (instance.m_anyBonesDown is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_posTol":
            case "posTol":
            {
                if (instance.m_posTol is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_velTol":
            case "velTol":
            {
                if (instance.m_velTol is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_duration":
            case "duration":
            {
                if (instance.m_duration is not TGet castValue) return false;
                value = castValue;
                return true;
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
            case "m_name":
            case "name":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_name = castValue;
                return true;
            }
            case "m_nameStrike":
            case "nameStrike":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_nameStrike = castValue;
                return true;
            }
            case "m_nameLift":
            case "nameLift":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_nameLift = castValue;
                return true;
            }
            case "m_nameLock":
            case "nameLock":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_nameLock = castValue;
                return true;
            }
            case "m_nameUnlock":
            case "nameUnlock":
            {
                if (value is not List<byte> castValue) return false;
                instance.m_nameUnlock = castValue;
                return true;
            }
            case "m_minPos":
            case "minPos":
            {
                if (value is not List<float> castValue) return false;
                instance.m_minPos = castValue;
                return true;
            }
            case "m_maxPos":
            case "maxPos":
            {
                if (value is not List<float> castValue) return false;
                instance.m_maxPos = castValue;
                return true;
            }
            case "m_minVel":
            case "minVel":
            {
                if (value is not List<float> castValue) return false;
                instance.m_minVel = castValue;
                return true;
            }
            case "m_maxVel":
            case "maxVel":
            {
                if (value is not List<float> castValue) return false;
                instance.m_maxVel = castValue;
                return true;
            }
            case "m_allBonesDown":
            case "allBonesDown":
            {
                if (value is not List<float> castValue) return false;
                instance.m_allBonesDown = castValue;
                return true;
            }
            case "m_anyBonesDown":
            case "anyBonesDown":
            {
                if (value is not List<float> castValue) return false;
                instance.m_anyBonesDown = castValue;
                return true;
            }
            case "m_posTol":
            case "posTol":
            {
                if (value is not float castValue) return false;
                instance.m_posTol = castValue;
                return true;
            }
            case "m_velTol":
            case "velTol":
            {
                if (value is not float castValue) return false;
                instance.m_velTol = castValue;
                return true;
            }
            case "m_duration":
            case "duration":
            {
                if (value is not float castValue) return false;
                instance.m_duration = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
