// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkpRotationalConstraintDataAtomsData : HavokData<HKLib.hk2019.hkpRotationalConstraintData.Atoms> 
{
    public hkpRotationalConstraintDataAtomsData(HavokType type, HKLib.hk2019.hkpRotationalConstraintData.Atoms instance) : base(type, instance) {}

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
            case "m_rotations":
            case "rotations":
            {
                if (value is not HKLib.hk2019.hkpSetLocalRotationsConstraintAtom castValue) return false;
                instance.m_rotations = castValue;
                return true;
            }
            case "m_ang":
            case "ang":
            {
                if (value is not HKLib.hk2019.hkpAngConstraintAtom castValue) return false;
                instance.m_ang = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
