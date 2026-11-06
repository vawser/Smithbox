// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaFootstepAnalysisInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaFootstepAnalysisInfo>
{
    public hkaFootstepAnalysisInfoData(Havok.hk2018.hkaFootstepAnalysisInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                value = instance.m_propertyBag;
                return true;
            }
            case "m_name":
            case "name":
            {
                value = instance.m_name;
                return true;
            }
            case "m_nameStrike":
            case "nameStrike":
            {
                value = instance.m_nameStrike;
                return true;
            }
            case "m_nameLift":
            case "nameLift":
            {
                value = instance.m_nameLift;
                return true;
            }
            case "m_nameLock":
            case "nameLock":
            {
                value = instance.m_nameLock;
                return true;
            }
            case "m_nameUnlock":
            case "nameUnlock":
            {
                value = instance.m_nameUnlock;
                return true;
            }
            case "m_minPos":
            case "minPos":
            {
                value = instance.m_minPos;
                return true;
            }
            case "m_maxPos":
            case "maxPos":
            {
                value = instance.m_maxPos;
                return true;
            }
            case "m_minVel":
            case "minVel":
            {
                value = instance.m_minVel;
                return true;
            }
            case "m_maxVel":
            case "maxVel":
            {
                value = instance.m_maxVel;
                return true;
            }
            case "m_allBonesDown":
            case "allBonesDown":
            {
                value = instance.m_allBonesDown;
                return true;
            }
            case "m_anyBonesDown":
            case "anyBonesDown":
            {
                value = instance.m_anyBonesDown;
                return true;
            }
            case "m_posTol":
            case "posTol":
            {
                value = instance.m_posTol;
                return true;
            }
            case "m_velTol":
            case "velTol":
            {
                value = instance.m_velTol;
                return true;
            }
            case "m_duration":
            case "duration":
            {
                value = instance.m_duration;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not Havok.hk2018.hkPropertyBag castValue) return false;
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
