// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkpLinLimitConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkpLinLimitConstraintAtom>
{
    public hkpLinLimitConstraintAtomData(Havok.hk2018.hkpLinLimitConstraintAtom instance) : base(instance) { }

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
            case "m_axisIndex":
            case "axisIndex":
            {
                value = instance.m_axisIndex;
                return true;
            }
            case "m_min":
            case "min":
            {
                value = instance.m_min;
                return true;
            }
            case "m_max":
            case "max":
            {
                value = instance.m_max;
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
                if (value is Havok.hk2018.hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (Havok.hk2018.hkpConstraintAtom.AtomType)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_axisIndex":
            case "axisIndex":
            {
                if (value is not byte castValue) return false;
                instance.m_axisIndex = castValue;
                return true;
            }
            case "m_min":
            case "min":
            {
                if (value is not float castValue) return false;
                instance.m_min = castValue;
                return true;
            }
            case "m_max":
            case "max":
            {
                if (value is not float castValue) return false;
                instance.m_max = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
