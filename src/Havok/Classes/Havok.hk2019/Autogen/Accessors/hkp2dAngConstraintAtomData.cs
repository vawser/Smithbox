// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkp2dAngConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkp2dAngConstraintAtom>
{
    public hkp2dAngConstraintAtomData(Havok.hk2019.hkp2dAngConstraintAtom instance) : base(instance) { }

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
            case "m_freeRotationAxis":
            case "freeRotationAxis":
            {
                value = instance.m_freeRotationAxis;
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
            case "m_freeRotationAxis":
            case "freeRotationAxis":
            {
                if (value is not byte castValue) return false;
                instance.m_freeRotationAxis = castValue;
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
