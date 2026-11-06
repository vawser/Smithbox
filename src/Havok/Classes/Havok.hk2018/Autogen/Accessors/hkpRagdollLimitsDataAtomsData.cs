// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkpRagdollLimitsDataAtomsData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkpRagdollLimitsData.Atoms>
{
    public hkpRagdollLimitsDataAtomsData(Havok.hk2018.hkpRagdollLimitsData.Atoms instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_rotations":
            case "rotations":
            {
                value = instance.m_rotations;
                return true;
            }
            case "m_twistLimit":
            case "twistLimit":
            {
                value = instance.m_twistLimit;
                return true;
            }
            case "m_coneLimit":
            case "coneLimit":
            {
                value = instance.m_coneLimit;
                return true;
            }
            case "m_planesLimit":
            case "planesLimit":
            {
                value = instance.m_planesLimit;
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
            case "m_rotations":
            case "rotations":
            {
                if (value is not Havok.hk2018.hkpSetLocalRotationsConstraintAtom castValue) return false;
                instance.m_rotations = castValue;
                return true;
            }
            case "m_twistLimit":
            case "twistLimit":
            {
                if (value is not Havok.hk2018.hkpTwistLimitConstraintAtom castValue) return false;
                instance.m_twistLimit = castValue;
                return true;
            }
            case "m_coneLimit":
            case "coneLimit":
            {
                if (value is not Havok.hk2018.hkpConeLimitConstraintAtom castValue) return false;
                instance.m_coneLimit = castValue;
                return true;
            }
            case "m_planesLimit":
            case "planesLimit":
            {
                if (value is not Havok.hk2018.hkpConeLimitConstraintAtom castValue) return false;
                instance.m_planesLimit = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
