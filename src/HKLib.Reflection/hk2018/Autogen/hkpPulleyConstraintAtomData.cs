// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpPulleyConstraintAtomData : HavokData<hkpPulleyConstraintAtom> 
{
    public hkpPulleyConstraintAtomData(HavokType type, hkpPulleyConstraintAtom instance) : base(type, instance) {}

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
            case "m_fixedPivotAinWorld":
            case "fixedPivotAinWorld":
            {
                if (instance.m_fixedPivotAinWorld is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_fixedPivotBinWorld":
            case "fixedPivotBinWorld":
            {
                if (instance.m_fixedPivotBinWorld is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ropeLength":
            case "ropeLength":
            {
                if (instance.m_ropeLength is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_leverageOnBodyB":
            case "leverageOnBodyB":
            {
                if (instance.m_leverageOnBodyB is not TGet castValue) return false;
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
            case "m_fixedPivotAinWorld":
            case "fixedPivotAinWorld":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_fixedPivotAinWorld = castValue;
                return true;
            }
            case "m_fixedPivotBinWorld":
            case "fixedPivotBinWorld":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_fixedPivotBinWorld = castValue;
                return true;
            }
            case "m_ropeLength":
            case "ropeLength":
            {
                if (value is not float castValue) return false;
                instance.m_ropeLength = castValue;
                return true;
            }
            case "m_leverageOnBodyB":
            case "leverageOnBodyB":
            {
                if (value is not float castValue) return false;
                instance.m_leverageOnBodyB = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
