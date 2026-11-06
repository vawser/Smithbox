// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkpSetLocalRotationsConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkpSetLocalRotationsConstraintAtom>
{
    public hkpSetLocalRotationsConstraintAtomData(Havok.hk2019.hkpSetLocalRotationsConstraintAtom instance) : base(instance) { }

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
            case "m_rotationA":
            case "rotationA":
            {
                value = instance.m_rotationA;
                return true;
            }
            case "m_rotationB":
            case "rotationB":
            {
                value = instance.m_rotationB;
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
            case "m_rotationA":
            case "rotationA":
            {
                if (value is not Matrix3x3 castValue) return false;
                instance.m_rotationA = castValue;
                return true;
            }
            case "m_rotationB":
            case "rotationB":
            {
                if (value is not Matrix3x3 castValue) return false;
                instance.m_rotationB = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
