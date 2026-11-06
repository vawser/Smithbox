// Automatically Generated

#nullable enable

namespace Havok.hk55.Accessors;

internal sealed class hkpConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk55.hkpConstraintAtom>
{
    public hkpConstraintAtomData(Havok.hk55.hkpConstraintAtom instance) : base(instance) { }

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
                if (value is Havok.hk55.hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (Havok.hk55.hkpConstraintAtom.AtomType)ushortValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
