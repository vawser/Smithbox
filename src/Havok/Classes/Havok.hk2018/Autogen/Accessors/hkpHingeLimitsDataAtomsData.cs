// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkpHingeLimitsDataAtomsData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkpHingeLimitsData.Atoms>
{
    public hkpHingeLimitsDataAtomsData(Havok.hk2018.hkpHingeLimitsData.Atoms instance) : base(instance) { }

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
            case "m_angLimit":
            case "angLimit":
            {
                value = instance.m_angLimit;
                return true;
            }
            case "m_2dAng":
            case "2dAng":
            {
                value = instance.m_2dAng;
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
            case "m_angLimit":
            case "angLimit":
            {
                if (value is not Havok.hk2018.hkpAngLimitConstraintAtom castValue) return false;
                instance.m_angLimit = castValue;
                return true;
            }
            case "m_2dAng":
            case "2dAng":
            {
                if (value is not Havok.hk2018.hkp2dAngConstraintAtom castValue) return false;
                instance.m_2dAng = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
