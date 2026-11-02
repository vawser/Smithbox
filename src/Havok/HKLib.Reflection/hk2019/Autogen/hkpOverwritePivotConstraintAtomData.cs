// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkpOverwritePivotConstraintAtomData : HavokData<HKLib.hk2019.hkpOverwritePivotConstraintAtom> 
{
    public hkpOverwritePivotConstraintAtomData(HavokType type, HKLib.hk2019.hkpOverwritePivotConstraintAtom instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((ushort)instance.m_type is TGet ushortValue)
                {
                    value = ushortValue;
                    return true;
                }
                return false;
            }
            case "m_copyToPivotBFromPivotA":
            case "copyToPivotBFromPivotA":
            {
                if (instance.m_copyToPivotBFromPivotA is not TGet castValue) return false;
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
            case "m_type":
            case "type":
            {
                if (value is HKLib.hk2019.hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (HKLib.hk2019.hkpConstraintAtom.AtomType)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_copyToPivotBFromPivotA":
            case "copyToPivotBFromPivotA":
            {
                if (value is not byte castValue) return false;
                instance.m_copyToPivotBFromPivotA = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
