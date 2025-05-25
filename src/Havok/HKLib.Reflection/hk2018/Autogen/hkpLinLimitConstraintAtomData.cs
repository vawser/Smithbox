// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpLinLimitConstraintAtomData : HavokData<hkpLinLimitConstraintAtom> 
{
    public hkpLinLimitConstraintAtomData(HavokType type, hkpLinLimitConstraintAtom instance) : base(type, instance) {}

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
            case "m_axisIndex":
            case "axisIndex":
            {
                if (instance.m_axisIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_min":
            case "min":
            {
                if (instance.m_min is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_max":
            case "max":
            {
                if (instance.m_max is not TGet castValue) return false;
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
                if (value is hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (hkpConstraintAtom.AtomType)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_axisIndex":
            case "axisIndex":
            {
                if (value is not byte castValue) return false;
                instance.m_axisIndex = castValue;
                return true;
            }
            case "m_min":
            case "min":
            {
                if (value is not float castValue) return false;
                instance.m_min = castValue;
                return true;
            }
            case "m_max":
            case "max":
            {
                if (value is not float castValue) return false;
                instance.m_max = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
