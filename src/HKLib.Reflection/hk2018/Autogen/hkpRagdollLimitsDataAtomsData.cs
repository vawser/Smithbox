// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpRagdollLimitsDataAtomsData : HavokData<hkpRagdollLimitsData.Atoms> 
{
    public hkpRagdollLimitsDataAtomsData(HavokType type, hkpRagdollLimitsData.Atoms instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_rotations":
            case "rotations":
            {
                if (instance.m_rotations is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_twistLimit":
            case "twistLimit":
            {
                if (instance.m_twistLimit is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_coneLimit":
            case "coneLimit":
            {
                if (instance.m_coneLimit is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_planesLimit":
            case "planesLimit":
            {
                if (instance.m_planesLimit is not TGet castValue) return false;
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
            case "m_rotations":
            case "rotations":
            {
                if (value is not hkpSetLocalRotationsConstraintAtom castValue) return false;
                instance.m_rotations = castValue;
                return true;
            }
            case "m_twistLimit":
            case "twistLimit":
            {
                if (value is not hkpTwistLimitConstraintAtom castValue) return false;
                instance.m_twistLimit = castValue;
                return true;
            }
            case "m_coneLimit":
            case "coneLimit":
            {
                if (value is not hkpConeLimitConstraintAtom castValue) return false;
                instance.m_coneLimit = castValue;
                return true;
            }
            case "m_planesLimit":
            case "planesLimit":
            {
                if (value is not hkpConeLimitConstraintAtom castValue) return false;
                instance.m_planesLimit = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
