// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkpStiffSpringConstraintDataAtomsData : HavokData<HKLib.hk2019.hkpStiffSpringConstraintData.Atoms> 
{
    public hkpStiffSpringConstraintDataAtomsData(HavokType type, HKLib.hk2019.hkpStiffSpringConstraintData.Atoms instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_pivots":
            case "pivots":
            {
                if (instance.m_pivots is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_setupStabilization":
            case "setupStabilization":
            {
                if (instance.m_setupStabilization is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_spring":
            case "spring":
            {
                if (instance.m_spring is not TGet castValue) return false;
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
            case "m_pivots":
            case "pivots":
            {
                if (value is not HKLib.hk2019.hkpSetLocalTranslationsConstraintAtom castValue) return false;
                instance.m_pivots = castValue;
                return true;
            }
            case "m_setupStabilization":
            case "setupStabilization":
            {
                if (value is not HKLib.hk2019.hkpSetupStabilizationAtom castValue) return false;
                instance.m_setupStabilization = castValue;
                return true;
            }
            case "m_spring":
            case "spring":
            {
                if (value is not HKLib.hk2019.hkpStiffSpringConstraintAtom castValue) return false;
                instance.m_spring = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
