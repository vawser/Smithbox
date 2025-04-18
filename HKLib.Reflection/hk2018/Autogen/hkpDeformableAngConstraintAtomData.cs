// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpDeformableAngConstraintAtomData : HavokData<hkpDeformableAngConstraintAtom> 
{
    public hkpDeformableAngConstraintAtomData(HavokType type, hkpDeformableAngConstraintAtom instance) : base(type, instance) {}

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
            case "m_offset":
            case "offset":
            {
                if (instance.m_offset is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_yieldStrengthDiag":
            case "yieldStrengthDiag":
            {
                if (instance.m_yieldStrengthDiag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_yieldStrengthOffDiag":
            case "yieldStrengthOffDiag":
            {
                if (instance.m_yieldStrengthOffDiag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ultimateStrengthDiag":
            case "ultimateStrengthDiag":
            {
                if (instance.m_ultimateStrengthDiag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_ultimateStrengthOffDiag":
            case "ultimateStrengthOffDiag":
            {
                if (instance.m_ultimateStrengthOffDiag is not TGet castValue) return false;
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
            case "m_offset":
            case "offset":
            {
                if (value is not Quaternion castValue) return false;
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
