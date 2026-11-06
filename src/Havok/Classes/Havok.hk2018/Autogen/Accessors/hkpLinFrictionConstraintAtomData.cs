// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkpLinFrictionConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkpLinFrictionConstraintAtom>
{
    public hkpLinFrictionConstraintAtomData(Havok.hk2018.hkpLinFrictionConstraintAtom instance) : base(instance) { }

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
            case "m_isEnabled":
            case "isEnabled":
            {
                value = instance.m_isEnabled;
                return true;
            }
            case "m_frictionAxis":
            case "frictionAxis":
            {
                value = instance.m_frictionAxis;
                return true;
            }
            case "m_maxFrictionForce":
            case "maxFrictionForce":
            {
                value = instance.m_maxFrictionForce;
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
            case "m_isEnabled":
            case "isEnabled":
            {
                if (value is not byte castValue) return false;
                instance.m_isEnabled = castValue;
                return true;
            }
            case "m_frictionAxis":
            case "frictionAxis":
            {
                if (value is not byte castValue) return false;
                instance.m_frictionAxis = castValue;
                return true;
            }
            case "m_maxFrictionForce":
            case "maxFrictionForce":
            {
                if (value is not float castValue) return false;
                instance.m_maxFrictionForce = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
