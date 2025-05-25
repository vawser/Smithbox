// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpWheelFrictionConstraintDataAtomsData : HavokData<hkpWheelFrictionConstraintData.Atoms> 
{
    public hkpWheelFrictionConstraintDataAtomsData(HavokType type, hkpWheelFrictionConstraintData.Atoms instance) : base(type, instance) {}

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
                if (value is not hkpSetLocalTransformsConstraintAtom castValue) return false;
                instance.m_transforms = castValue;
                return true;
            }
            case "m_friction":
            case "friction":
            {
                if (value is not hkpWheelFrictionConstraintAtom castValue) return false;
                instance.m_friction = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
