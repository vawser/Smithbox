// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpPulleyConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpPulleyConstraintAtom>
{
    public hkpPulleyConstraintAtomData(Havok.hk2014.hkpPulleyConstraintAtom instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                value = instance.m_type;
                return true;
            }
            case "m_fixedPivotAinWorld":
            case "fixedPivotAinWorld":
            {
                value = instance.m_fixedPivotAinWorld;
                return true;
            }
            case "m_fixedPivotBinWorld":
            case "fixedPivotBinWorld":
            {
                value = instance.m_fixedPivotBinWorld;
                return true;
            }
            case "m_ropeLength":
            case "ropeLength":
            {
                value = instance.m_ropeLength;
                return true;
            }
            case "m_leverageOnBodyB":
            case "leverageOnBodyB":
            {
                value = instance.m_leverageOnBodyB;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2014.hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (Havok.hk2014.hkpConstraintAtom.AtomType)ushortValue;
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
