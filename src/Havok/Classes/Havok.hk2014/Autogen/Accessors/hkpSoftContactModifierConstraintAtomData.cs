// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpSoftContactModifierConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpSoftContactModifierConstraintAtom>
{
    public hkpSoftContactModifierConstraintAtomData(Havok.hk2014.hkpSoftContactModifierConstraintAtom instance) : base(instance) { }

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
            case "m_modifierAtomSize":
            case "modifierAtomSize":
            {
                value = instance.m_modifierAtomSize;
                return true;
            }
            case "m_childSize":
            case "childSize":
            {
                value = instance.m_childSize;
                return true;
            }
            case "m_child":
            case "child":
            {
                value = instance.m_child;
                return true;
            }
            case "m_tau":
            case "tau":
            {
                value = instance.m_tau;
                return true;
            }
            case "m_maxAcceleration":
            case "maxAcceleration":
            {
                value = instance.m_maxAcceleration;
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
            case "m_modifierAtomSize":
            case "modifierAtomSize":
            {
                if (value is not ushort castValue) return false;
                instance.m_modifierAtomSize = castValue;
                return true;
            }
            case "m_childSize":
            case "childSize":
            {
                if (value is not ushort castValue) return false;
                instance.m_childSize = castValue;
                return true;
            }
            case "m_child":
            case "child":
            {
                if (value is null)
                {
                    instance.m_child = default;
                    return true;
                }
                if (value is Havok.hk2014.hkpConstraintAtom castValue)
                {
                    instance.m_child = castValue;
                    return true;
                }
                return false;
            }
            case "m_tau":
            case "tau":
            {
                if (value is not float castValue) return false;
                instance.m_tau = castValue;
                return true;
            }
            case "m_maxAcceleration":
            case "maxAcceleration":
            {
                if (value is not float castValue) return false;
                instance.m_maxAcceleration = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
