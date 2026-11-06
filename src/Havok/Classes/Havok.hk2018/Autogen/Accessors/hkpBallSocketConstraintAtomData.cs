// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkpBallSocketConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkpBallSocketConstraintAtom>
{
    public hkpBallSocketConstraintAtomData(Havok.hk2018.hkpBallSocketConstraintAtom instance) : base(instance) { }

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
            case "m_solvingMethod":
            case "solvingMethod":
            {
                value = instance.m_solvingMethod;
                return true;
            }
            case "m_bodiesToNotify":
            case "bodiesToNotify":
            {
                value = instance.m_bodiesToNotify;
                return true;
            }
            case "m_velocityStabilizationFactor":
            case "velocityStabilizationFactor":
            {
                value = instance.m_velocityStabilizationFactor;
                return true;
            }
            case "m_enableLinearImpulseLimit":
            case "enableLinearImpulseLimit":
            {
                value = instance.m_enableLinearImpulseLimit;
                return true;
            }
            case "m_breachImpulse":
            case "breachImpulse":
            {
                value = instance.m_breachImpulse;
                return true;
            }
            case "m_inertiaStabilizationFactor":
            case "inertiaStabilizationFactor":
            {
                value = instance.m_inertiaStabilizationFactor;
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
            case "m_solvingMethod":
            case "solvingMethod":
            {
                if (value is Havok.hk2018.hkpConstraintAtom.SolvingMethod castValue)
                {
                    instance.m_solvingMethod = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_solvingMethod = (Havok.hk2018.hkpConstraintAtom.SolvingMethod)byteValue;
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
                if (value is not Havok.hk2018.hkUFloat8 castValue) return false;
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
