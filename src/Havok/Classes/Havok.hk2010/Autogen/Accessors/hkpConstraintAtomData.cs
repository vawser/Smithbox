// Automatically Generated

#nullable enable

namespace Havok.hk2010.Accessors;

internal sealed class hkpConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2010.hkpConstraintAtom>
{
    public hkpConstraintAtomData(Havok.hk2010.hkpConstraintAtom instance) : base(instance) { }

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
                if (value is Havok.hk2010.hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (Havok.hk2010.hkpConstraintAtom.AtomType)ushortValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
