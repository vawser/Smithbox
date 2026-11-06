// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkpBridgeConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkpBridgeConstraintAtom>
{
    public hkpBridgeConstraintAtomData(Havok.hk2019.hkpBridgeConstraintAtom instance) : base(instance) { }

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
            case "m_constraintData":
            case "constraintData":
            {
                value = instance.m_constraintData;
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
            case "m_constraintData":
            case "constraintData":
            {
                if (value is null)
                {
                    instance.m_constraintData = default;
                    return true;
                }
                if (value is Havok.hk2019.hkpConstraintData castValue)
                {
                    instance.m_constraintData = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
