// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpRackAndPinionConstraintDataAtomsData : HavokData<hkpRackAndPinionConstraintData.Atoms> 
{
    public hkpRackAndPinionConstraintDataAtomsData(HavokType type, hkpRackAndPinionConstraintData.Atoms instance) : base(type, instance) {}

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
                if (value is not hkpSetLocalTransformsConstraintAtom castValue) return false;
                instance.m_transforms = castValue;
                return true;
            }
            case "m_rackAndPinion":
            case "rackAndPinion":
            {
                if (value is not hkpRackAndPinionConstraintAtom castValue) return false;
                instance.m_rackAndPinion = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
