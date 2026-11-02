// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkpCogWheelConstraintDataAtomsData : HavokData<HKLib.hk2019.hkpCogWheelConstraintData.Atoms> 
{
    public hkpCogWheelConstraintDataAtomsData(HavokType type, HKLib.hk2019.hkpCogWheelConstraintData.Atoms instance) : base(type, instance) {}

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
            case "m_cogWheels":
            case "cogWheels":
            {
                if (instance.m_cogWheels is not TGet castValue) return false;
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
            case "m_cogWheels":
            case "cogWheels":
            {
                if (value is not HKLib.hk2019.hkpCogWheelConstraintAtom castValue) return false;
                instance.m_cogWheels = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
