// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkpStiffSpringConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkpStiffSpringConstraintAtom>
{
    public hkpStiffSpringConstraintAtomData(Havok.hk2019.hkpStiffSpringConstraintAtom instance) : base(instance) { }

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
            case "m_length":
            case "length":
            {
                value = instance.m_length;
                return true;
            }
            case "m_maxLength":
            case "maxLength":
            {
                value = instance.m_maxLength;
                return true;
            }
            case "m_springConstant":
            case "springConstant":
            {
                value = instance.m_springConstant;
                return true;
            }
            case "m_springDamping":
            case "springDamping":
            {
                value = instance.m_springDamping;
                return true;
            }
            case "m_tauFactor":
            case "tauFactor":
            {
                value = instance.m_tauFactor;
                return true;
            }
            case "m_dampingFactor":
            case "dampingFactor":
            {
                value = instance.m_dampingFactor;
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
