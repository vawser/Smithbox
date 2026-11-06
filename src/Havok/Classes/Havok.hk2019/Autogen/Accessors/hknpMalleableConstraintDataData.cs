// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpMalleableConstraintDataData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpMalleableConstraintData>
{
    public hknpMalleableConstraintDataData(Havok.hk2019.hknpMalleableConstraintData instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
                return true;
            }
            case "m_numNonDirectlySolvedAtoms":
            case "numNonDirectlySolvedAtoms":
            {
                value = instance.m_numNonDirectlySolvedAtoms;
                return true;
            }
            case "m_constraintData":
            case "constraintData":
            {
                value = instance.m_constraintData;
                return true;
            }
            case "m_atom":
            case "atom":
            {
                value = instance.m_atom;
                return true;
            }
            case "m_wantsRuntime":
            case "wantsRuntime":
            {
                value = instance.m_wantsRuntime;
                return true;
            }
            case "m_strength":
            case "strength":
            {
                value = instance.m_strength;
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
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            case "m_numNonDirectlySolvedAtoms":
            case "numNonDirectlySolvedAtoms":
            {
                if (value is not byte castValue) return false;
                instance.m_numNonDirectlySolvedAtoms = castValue;
                return true;
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
            case "m_atom":
            case "atom":
            {
                if (value is not Havok.hk2019.hknpBridgeConstraintAtom castValue) return false;
                instance.m_atom = castValue;
                return true;
            }
            case "m_wantsRuntime":
            case "wantsRuntime":
            {
                if (value is not bool castValue) return false;
                instance.m_wantsRuntime = castValue;
                return true;
            }
            case "m_strength":
            case "strength":
            {
                if (value is not float castValue) return false;
                instance.m_strength = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
