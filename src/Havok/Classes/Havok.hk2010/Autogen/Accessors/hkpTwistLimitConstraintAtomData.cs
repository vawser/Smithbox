// Automatically Generated

#nullable enable

namespace Havok.hk2010.Accessors;

internal sealed class hkpTwistLimitConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2010.hkpTwistLimitConstraintAtom>
{
    public hkpTwistLimitConstraintAtomData(Havok.hk2010.hkpTwistLimitConstraintAtom instance) : base(instance) { }

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
            case "m_twistAxis":
            case "twistAxis":
            {
                value = instance.m_twistAxis;
                return true;
            }
            case "m_refAxis":
            case "refAxis":
            {
                value = instance.m_refAxis;
                return true;
            }
            case "m_minAngle":
            case "minAngle":
            {
                value = instance.m_minAngle;
                return true;
            }
            case "m_maxAngle":
            case "maxAngle":
            {
                value = instance.m_maxAngle;
                return true;
            }
            case "m_angularLimitsTauFactor":
            case "angularLimitsTauFactor":
            {
                value = instance.m_angularLimitsTauFactor;
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
            case "m_isEnabled":
            case "isEnabled":
            {
                if (value is not byte castValue) return false;
                instance.m_isEnabled = castValue;
                return true;
            }
            case "m_twistAxis":
            case "twistAxis":
            {
                if (value is not byte castValue) return false;
                instance.m_twistAxis = castValue;
                return true;
            }
            case "m_refAxis":
            case "refAxis":
            {
                if (value is not byte castValue) return false;
                instance.m_refAxis = castValue;
                return true;
            }
            case "m_minAngle":
            case "minAngle":
            {
                if (value is not float castValue) return false;
                instance.m_minAngle = castValue;
                return true;
            }
            case "m_maxAngle":
            case "maxAngle":
            {
                if (value is not float castValue) return false;
                instance.m_maxAngle = castValue;
                return true;
            }
            case "m_angularLimitsTauFactor":
            case "angularLimitsTauFactor":
            {
                if (value is not float castValue) return false;
                instance.m_angularLimitsTauFactor = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
