// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkpEllipticalLimitConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkpEllipticalLimitConstraintAtom>
{
    public hkpEllipticalLimitConstraintAtomData(Havok.hk2018.hkpEllipticalLimitConstraintAtom instance) : base(instance) { }

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
            case "m_elipticalLimitEnabled":
            case "elipticalLimitEnabled":
            {
                value = instance.m_elipticalLimitEnabled;
                return true;
            }
            case "m_coneLimitEnabled":
            case "coneLimitEnabled":
            {
                value = instance.m_coneLimitEnabled;
                return true;
            }
            case "m_angle0":
            case "angle0":
            {
                value = instance.m_angle0;
                return true;
            }
            case "m_angle1":
            case "angle1":
            {
                value = instance.m_angle1;
                return true;
            }
            case "m_coneAngle":
            case "coneAngle":
            {
                value = instance.m_coneAngle;
                return true;
            }
            case "m_angleCorrected0":
            case "angleCorrected0":
            {
                value = instance.m_angleCorrected0;
                return true;
            }
            case "m_angleCorrected1":
            case "angleCorrected1":
            {
                value = instance.m_angleCorrected1;
                return true;
            }
            case "m_coneAngleCorrected":
            case "coneAngleCorrected":
            {
                value = instance.m_coneAngleCorrected;
                return true;
            }
            case "m_angleCorrected0Inv":
            case "angleCorrected0Inv":
            {
                value = instance.m_angleCorrected0Inv;
                return true;
            }
            case "m_angleCorrected1Inv":
            case "angleCorrected1Inv":
            {
                value = instance.m_angleCorrected1Inv;
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
            case "m_elipticalLimitEnabled":
            case "elipticalLimitEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_elipticalLimitEnabled = castValue;
                return true;
            }
            case "m_coneLimitEnabled":
            case "coneLimitEnabled":
            {
                if (value is not bool castValue) return false;
                instance.m_coneLimitEnabled = castValue;
                return true;
            }
            case "m_angle0":
            case "angle0":
            {
                if (value is not float castValue) return false;
                instance.m_angle0 = castValue;
                return true;
            }
            case "m_angle1":
            case "angle1":
            {
                if (value is not float castValue) return false;
                instance.m_angle1 = castValue;
                return true;
            }
            case "m_coneAngle":
            case "coneAngle":
            {
                if (value is not float castValue) return false;
                instance.m_coneAngle = castValue;
                return true;
            }
            case "m_angleCorrected0":
            case "angleCorrected0":
            {
                if (value is not float castValue) return false;
                instance.m_angleCorrected0 = castValue;
                return true;
            }
            case "m_angleCorrected1":
            case "angleCorrected1":
            {
                if (value is not float castValue) return false;
                instance.m_angleCorrected1 = castValue;
                return true;
            }
            case "m_coneAngleCorrected":
            case "coneAngleCorrected":
            {
                if (value is not float castValue) return false;
                instance.m_coneAngleCorrected = castValue;
                return true;
            }
            case "m_angleCorrected0Inv":
            case "angleCorrected0Inv":
            {
                if (value is not float castValue) return false;
                instance.m_angleCorrected0Inv = castValue;
                return true;
            }
            case "m_angleCorrected1Inv":
            case "angleCorrected1Inv":
            {
                if (value is not float castValue) return false;
                instance.m_angleCorrected1Inv = castValue;
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
            default:
            return false;
        }
    }

}
