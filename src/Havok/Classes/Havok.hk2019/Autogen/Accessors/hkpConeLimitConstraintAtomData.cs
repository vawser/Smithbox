// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkpConeLimitConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkpConeLimitConstraintAtom>
{
    public hkpConeLimitConstraintAtomData(Havok.hk2019.hkpConeLimitConstraintAtom instance) : base(instance) { }

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
            case "m_twistAxisInA":
            case "twistAxisInA":
            {
                value = instance.m_twistAxisInA;
                return true;
            }
            case "m_refAxisInB":
            case "refAxisInB":
            {
                value = instance.m_refAxisInB;
                return true;
            }
            case "m_angularLimitsTauFactor":
            case "angularLimitsTauFactor":
            {
                value = instance.m_angularLimitsTauFactor;
                return true;
            }
            case "m_angularLimitsDampFactor":
            case "angularLimitsDampFactor":
            {
                value = instance.m_angularLimitsDampFactor;
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
            case "m_invCosMinAngle":
            case "invCosMinAngle":
            {
                value = instance.m_invCosMinAngle;
                return true;
            }
            case "m_invCosMaxAngle":
            case "invCosMaxAngle":
            {
                value = instance.m_invCosMaxAngle;
                return true;
            }
            case "m_tanMinAngle":
            case "tanMinAngle":
            {
                value = instance.m_tanMinAngle;
                return true;
            }
            case "m_tanMaxAngle":
            case "tanMaxAngle":
            {
                value = instance.m_tanMaxAngle;
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
            case "m_isEnabled":
            case "isEnabled":
            {
                if (value is not byte castValue) return false;
                instance.m_isEnabled = castValue;
                return true;
            }
            case "m_twistAxisInA":
            case "twistAxisInA":
            {
                if (value is not byte castValue) return false;
                instance.m_twistAxisInA = castValue;
                return true;
            }
            case "m_refAxisInB":
            case "refAxisInB":
            {
                if (value is not byte castValue) return false;
                instance.m_refAxisInB = castValue;
                return true;
            }
            case "m_angularLimitsTauFactor":
            case "angularLimitsTauFactor":
            {
                if (value is not float castValue) return false;
                instance.m_angularLimitsTauFactor = castValue;
                return true;
            }
            case "m_angularLimitsDampFactor":
            case "angularLimitsDampFactor":
            {
                if (value is not float castValue) return false;
                instance.m_angularLimitsDampFactor = castValue;
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
            case "m_invCosMinAngle":
            case "invCosMinAngle":
            {
                if (value is not float castValue) return false;
                instance.m_invCosMinAngle = castValue;
                return true;
            }
            case "m_invCosMaxAngle":
            case "invCosMaxAngle":
            {
                if (value is not float castValue) return false;
                instance.m_invCosMaxAngle = castValue;
                return true;
            }
            case "m_tanMinAngle":
            case "tanMinAngle":
            {
                if (value is not float castValue) return false;
                instance.m_tanMinAngle = castValue;
                return true;
            }
            case "m_tanMaxAngle":
            case "tanMaxAngle":
            {
                if (value is not float castValue) return false;
                instance.m_tanMaxAngle = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
