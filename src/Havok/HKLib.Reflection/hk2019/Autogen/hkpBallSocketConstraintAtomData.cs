// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkpBallSocketConstraintAtomData : HavokData<HKLib.hk2019.hkpBallSocketConstraintAtom> 
{
    public hkpBallSocketConstraintAtomData(HavokType type, HKLib.hk2019.hkpBallSocketConstraintAtom instance) : base(type, instance) {}

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
            case "m_solvingMethod":
            case "solvingMethod":
            {
                if (instance.m_solvingMethod is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_solvingMethod is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_bodiesToNotify":
            case "bodiesToNotify":
            {
                if (instance.m_bodiesToNotify is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_velocityStabilizationFactor":
            case "velocityStabilizationFactor":
            {
                if (instance.m_velocityStabilizationFactor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_enableLinearImpulseLimit":
            case "enableLinearImpulseLimit":
            {
                if (instance.m_enableLinearImpulseLimit is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_breachImpulse":
            case "breachImpulse":
            {
                if (instance.m_breachImpulse is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_tauFactor":
            case "tauFactor":
            {
                if (instance.m_tauFactor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_dampingFactor":
            case "dampingFactor":
            {
                if (instance.m_dampingFactor is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_inertiaStabilizationFactor":
            case "inertiaStabilizationFactor":
            {
                if (instance.m_inertiaStabilizationFactor is not TGet castValue) return false;
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
                if (value is HKLib.hk2019.hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (HKLib.hk2019.hkpConstraintAtom.AtomType)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_solvingMethod":
            case "solvingMethod":
            {
                if (value is HKLib.hk2019.hkpConstraintAtom.SolvingMethod castValue)
                {
                    instance.m_solvingMethod = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_solvingMethod = (HKLib.hk2019.hkpConstraintAtom.SolvingMethod)byteValue;
                    return true;
                }
                return false;
            }
            case "m_bodiesToNotify":
            case "bodiesToNotify":
            {
                if (value is not byte castValue) return false;
                instance.m_bodiesToNotify = castValue;
                return true;
            }
            case "m_velocityStabilizationFactor":
            case "velocityStabilizationFactor":
            {
                if (value is not HKLib.hk2019.hkUFloat8 castValue) return false;
                instance.m_velocityStabilizationFactor = castValue;
                return true;
            }
            case "m_enableLinearImpulseLimit":
            case "enableLinearImpulseLimit":
            {
                if (value is not bool castValue) return false;
                instance.m_enableLinearImpulseLimit = castValue;
                return true;
            }
            case "m_breachImpulse":
            case "breachImpulse":
            {
                if (value is not float castValue) return false;
                instance.m_breachImpulse = castValue;
                return true;
            }
            case "m_tauFactor":
            case "tauFactor":
            {
                if (value is not float castValue) return false;
                instance.m_tauFactor = castValue;
                return true;
            }
            case "m_dampingFactor":
            case "dampingFactor":
            {
                if (value is not float castValue) return false;
                instance.m_dampingFactor = castValue;
                return true;
            }
            case "m_inertiaStabilizationFactor":
            case "inertiaStabilizationFactor":
            {
                if (value is not float castValue) return false;
                instance.m_inertiaStabilizationFactor = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
