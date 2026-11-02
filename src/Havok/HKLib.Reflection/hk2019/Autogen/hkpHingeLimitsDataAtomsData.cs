// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkpHingeLimitsDataAtomsData : HavokData<HKLib.hk2019.hkpHingeLimitsData.Atoms> 
{
    public hkpHingeLimitsDataAtomsData(HavokType type, HKLib.hk2019.hkpHingeLimitsData.Atoms instance) : base(type, instance) {}

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
            case "m_angLimit":
            case "angLimit":
            {
                if (instance.m_angLimit is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_2dAng":
            case "2dAng":
            {
                if (instance.m_2dAng is not TGet castValue) return false;
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
                if (value is not HKLib.hk2019.hkpSetLocalRotationsConstraintAtom castValue) return false;
                instance.m_rotations = castValue;
                return true;
            }
            case "m_angLimit":
            case "angLimit":
            {
                if (value is not HKLib.hk2019.hkpAngLimitConstraintAtom castValue) return false;
                instance.m_angLimit = castValue;
                return true;
            }
            case "m_2dAng":
            case "2dAng":
            {
                if (value is not HKLib.hk2019.hkp2dAngConstraintAtom castValue) return false;
                instance.m_2dAng = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
