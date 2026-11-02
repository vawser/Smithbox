// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkpRackAndPinionConstraintDataAtomsData : HavokData<HKLib.hk2019.hkpRackAndPinionConstraintData.Atoms> 
{
    public hkpRackAndPinionConstraintDataAtomsData(HavokType type, HKLib.hk2019.hkpRackAndPinionConstraintData.Atoms instance) : base(type, instance) {}

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
            case "m_rackAndPinion":
            case "rackAndPinion":
            {
                if (instance.m_rackAndPinion is not TGet castValue) return false;
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
            case "m_rackAndPinion":
            case "rackAndPinion":
            {
                if (value is not HKLib.hk2019.hkpRackAndPinionConstraintAtom castValue) return false;
                instance.m_rackAndPinion = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
