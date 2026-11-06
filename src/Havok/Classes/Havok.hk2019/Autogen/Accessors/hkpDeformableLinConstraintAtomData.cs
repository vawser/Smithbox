// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkpDeformableLinConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkpDeformableLinConstraintAtom>
{
    public hkpDeformableLinConstraintAtomData(Havok.hk2019.hkpDeformableLinConstraintAtom instance) : base(instance) { }

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
            case "m_offset":
            case "offset":
            {
                value = instance.m_offset;
                return true;
            }
            case "m_yieldStrengthDiag":
            case "yieldStrengthDiag":
            {
                value = instance.m_yieldStrengthDiag;
                return true;
            }
            case "m_yieldStrengthOffDiag":
            case "yieldStrengthOffDiag":
            {
                value = instance.m_yieldStrengthOffDiag;
                return true;
            }
            case "m_ultimateStrengthDiag":
            case "ultimateStrengthDiag":
            {
                value = instance.m_ultimateStrengthDiag;
                return true;
            }
            case "m_ultimateStrengthOffDiag":
            case "ultimateStrengthOffDiag":
            {
                value = instance.m_ultimateStrengthOffDiag;
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
                if (value is Havok.hk2019.hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (Havok.hk2019.hkpConstraintAtom.AtomType)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_offset":
            case "offset":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_offset = castValue;
                return true;
            }
            case "m_yieldStrengthDiag":
            case "yieldStrengthDiag":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_yieldStrengthDiag = castValue;
                return true;
            }
            case "m_yieldStrengthOffDiag":
            case "yieldStrengthOffDiag":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_yieldStrengthOffDiag = castValue;
                return true;
            }
            case "m_ultimateStrengthDiag":
            case "ultimateStrengthDiag":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_ultimateStrengthDiag = castValue;
                return true;
            }
            case "m_ultimateStrengthOffDiag":
            case "ultimateStrengthOffDiag":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_ultimateStrengthOffDiag = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
