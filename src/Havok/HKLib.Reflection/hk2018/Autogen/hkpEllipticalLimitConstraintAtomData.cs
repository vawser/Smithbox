// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkpEllipticalLimitConstraintAtomData : HavokData<hkpEllipticalLimitConstraintAtom> 
{
    public hkpEllipticalLimitConstraintAtomData(HavokType type, hkpEllipticalLimitConstraintAtom instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                if (instance.m_type is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((ushort)instance.m_type is TGet ushortValue)
                {
                    value = ushortValue;
                    return true;
                }
                return false;
            }
            case "m_isEnabled":
            case "isEnabled":
            {
                if (instance.m_isEnabled is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_elipticalLimitEnabled":
            case "elipticalLimitEnabled":
            {
                if (instance.m_elipticalLimitEnabled is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_coneLimitEnabled":
            case "coneLimitEnabled":
            {
                if (instance.m_coneLimitEnabled is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_angle0":
            case "angle0":
            {
                if (instance.m_angle0 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_angle1":
            case "angle1":
            {
                if (instance.m_angle1 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_coneAngle":
            case "coneAngle":
            {
                if (instance.m_coneAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_angleCorrected0":
            case "angleCorrected0":
            {
                if (instance.m_angleCorrected0 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_angleCorrected1":
            case "angleCorrected1":
            {
                if (instance.m_angleCorrected1 is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_coneAngleCorrected":
            case "coneAngleCorrected":
            {
                if (instance.m_coneAngleCorrected is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_angleCorrected0Inv":
            case "angleCorrected0Inv":
            {
                if (instance.m_angleCorrected0Inv is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_angleCorrected1Inv":
            case "angleCorrected1Inv":
            {
                if (instance.m_angleCorrected1Inv is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_angularLimitsTauFactor":
            case "angularLimitsTauFactor":
            {
                if (instance.m_angularLimitsTauFactor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_angularLimitsDampFactor":
            case "angularLimitsDampFactor":
            {
                if (instance.m_angularLimitsDampFactor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
    {
        switch (fieldName)
        {
            case "m_type":
            case "type":
            {
                if (value is hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (hkpConstraintAtom.AtomType)ushortValue;
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
