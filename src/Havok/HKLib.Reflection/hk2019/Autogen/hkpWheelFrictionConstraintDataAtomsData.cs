// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkpWheelFrictionConstraintDataAtomsData : HavokData<HKLib.hk2019.hkpWheelFrictionConstraintData.Atoms> 
{
    public hkpWheelFrictionConstraintDataAtomsData(HavokType type, HKLib.hk2019.hkpWheelFrictionConstraintData.Atoms instance) : base(type, instance) {}

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
            case "m_friction":
            case "friction":
            {
                if (instance.m_friction is not TGet castValue) return false;
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
            case "m_friction":
            case "friction":
            {
                if (value is not HKLib.hk2019.hkpWheelFrictionConstraintAtom castValue) return false;
                instance.m_friction = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
