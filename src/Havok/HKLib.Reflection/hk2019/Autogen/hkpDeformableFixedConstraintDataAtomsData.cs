// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkpDeformableFixedConstraintDataAtomsData : HavokData<HKLib.hk2019.hkpDeformableFixedConstraintData.Atoms> 
{
    public hkpDeformableFixedConstraintDataAtomsData(HavokType type, HKLib.hk2019.hkpDeformableFixedConstraintData.Atoms instance) : base(type, instance) {}

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
            case "m_lin":
            case "lin":
            {
                if (instance.m_lin is not TGet castValue) return false;
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
            case "m_lin":
            case "lin":
            {
                if (value is not HKLib.hk2019.hkpDeformableLinConstraintAtom castValue) return false;
                instance.m_lin = castValue;
                return true;
            }
            case "m_ang":
            case "ang":
            {
                if (value is not HKLib.hk2019.hkpDeformableAngConstraintAtom castValue) return false;
                instance.m_ang = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
