// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkpFixedConstraintDataAtomsData : HavokData<HKLib.hk2019.hkpFixedConstraintData.Atoms> 
{
    public hkpFixedConstraintDataAtomsData(HavokType type, HKLib.hk2019.hkpFixedConstraintData.Atoms instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_transforms":
            case "transforms":
            {
                if (instance.m_transforms is not TGet castValue) return false;
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
            case "m_ballSocket":
            case "ballSocket":
            {
                if (instance.m_ballSocket is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ang":
            case "ang":
            {
                if (instance.m_ang is not TGet castValue) return false;
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
            case "m_transforms":
            case "transforms":
            {
                if (value is not HKLib.hk2019.hkpSetLocalTransformsConstraintAtom castValue) return false;
                instance.m_transforms = castValue;
                return true;
            }
            case "m_setupStabilization":
            case "setupStabilization":
            {
                if (value is not HKLib.hk2019.hkpSetupStabilizationAtom castValue) return false;
                instance.m_setupStabilization = castValue;
                return true;
            }
            case "m_ballSocket":
            case "ballSocket":
            {
                if (value is not HKLib.hk2019.hkpBallSocketConstraintAtom castValue) return false;
                instance.m_ballSocket = castValue;
                return true;
            }
            case "m_ang":
            case "ang":
            {
                if (value is not HKLib.hk2019.hkp3dAngConstraintAtom castValue) return false;
                instance.m_ang = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
