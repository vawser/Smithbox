// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkpRagdollConstraintDataData : HavokData<HKLib.hk2019.hkpRagdollConstraintData> 
{
    public hkpRagdollConstraintDataData(HavokType type, HKLib.hk2019.hkpRagdollConstraintData instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numNonDirectlySolvedAtoms":
            case "numNonDirectlySolvedAtoms":
            {
                if (instance.m_numNonDirectlySolvedAtoms is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_atoms":
            case "atoms":
            {
                if (instance.m_atoms is not TGet castValue) return false;
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
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_numNonDirectlySolvedAtoms":
            case "numNonDirectlySolvedAtoms":
            {
                if (value is not byte castValue) return false;
                instance.m_numNonDirectlySolvedAtoms = castValue;
                return true;
            }
            case "m_atoms":
            case "atoms":
            {
                if (value is not HKLib.hk2019.hkpRagdollConstraintData.Atoms castValue) return false;
                instance.m_atoms = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
