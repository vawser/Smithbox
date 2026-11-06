// Automatically Generated

#nullable enable

namespace Havok.hk2010.Accessors;

internal sealed class hkpStiffSpringConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2010.hkpStiffSpringConstraintAtom>
{
    public hkpStiffSpringConstraintAtomData(Havok.hk2010.hkpStiffSpringConstraintAtom instance) : base(instance) { }

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
            case "m_length":
            case "length":
            {
                if (value is not float castValue) return false;
                instance.m_length = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
