// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkpStiffSpringConstraintAtomData : HavokData<HKLib.hk2019.hkpStiffSpringConstraintAtom> 
{
    public hkpStiffSpringConstraintAtomData(HavokType type, HKLib.hk2019.hkpStiffSpringConstraintAtom instance) : base(type, instance) {}

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
            case "m_length":
            case "length":
            {
                if (instance.m_length is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxLength":
            case "maxLength":
            {
                if (instance.m_maxLength is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_springConstant":
            case "springConstant":
            {
                if (instance.m_springConstant is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_springDamping":
            case "springDamping":
            {
                if (instance.m_springDamping is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_tauFactor":
            case "tauFactor":
            {
                if (instance.m_tauFactor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dampingFactor":
            case "dampingFactor":
            {
                if (instance.m_dampingFactor is not TGet castValue) return false;
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
            case "m_length":
            case "length":
            {
                if (value is not float castValue) return false;
                instance.m_length = castValue;
                return true;
            }
            case "m_maxLength":
            case "maxLength":
            {
                if (value is not float castValue) return false;
                instance.m_maxLength = castValue;
                return true;
            }
            case "m_springConstant":
            case "springConstant":
            {
                if (value is not float castValue) return false;
                instance.m_springConstant = castValue;
                return true;
            }
            case "m_springDamping":
            case "springDamping":
            {
                if (value is not float castValue) return false;
                instance.m_springDamping = castValue;
                return true;
            }
            case "m_tauFactor":
            case "tauFactor":
            {
                if (value is not float castValue) return false;
                instance.m_tauFactor = castValue;
                return true;
            }
            case "m_dampingFactor":
            case "dampingFactor":
            {
                if (value is not float castValue) return false;
                instance.m_dampingFactor = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
