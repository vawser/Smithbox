// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpHingeLimitsDataAtomsData : HavokData<hkpHingeLimitsData.Atoms> 
{
    public hkpHingeLimitsDataAtomsData(HavokType type, hkpHingeLimitsData.Atoms instance) : base(type, instance) {}

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
                if (value is not hkpSetLocalRotationsConstraintAtom castValue) return false;
                instance.m_rotations = castValue;
                return true;
            }
            case "m_angLimit":
            case "angLimit":
            {
                if (value is not hkpAngLimitConstraintAtom castValue) return false;
                instance.m_angLimit = castValue;
                return true;
            }
            case "m_2dAng":
            case "2dAng":
            {
                if (value is not hkp2dAngConstraintAtom castValue) return false;
                instance.m_2dAng = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
