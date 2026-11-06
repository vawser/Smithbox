// Automatically Generated

#nullable enable

namespace Havok.hk2010.Accessors;

internal sealed class hkp2dAngConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2010.hkp2dAngConstraintAtom>
{
    public hkp2dAngConstraintAtomData(Havok.hk2010.hkp2dAngConstraintAtom instance) : base(instance) { }

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
            case "m_freeRotationAxis":
            case "freeRotationAxis":
            {
                if (value is not byte castValue) return false;
                instance.m_freeRotationAxis = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
