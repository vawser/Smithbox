// Automatically Generated

#nullable enable

namespace Havok.hk2015.Accessors;

internal sealed class hkpBreakableConstraintDataData : Havok.Formats.HavokFieldAccess<Havok.hk2015.hkpBreakableConstraintData>
{
    public hkpBreakableConstraintDataData(Havok.hk2015.hkpBreakableConstraintData instance) : base(instance) { }

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
            case "m_constraintData":
            case "constraintData":
            {
                value = instance.m_constraintData;
                return true;
            }
            case "m_atoms":
            case "atoms":
            {
                value = instance.m_atoms;
                return true;
            }
            case "m_solverResultLimit":
            case "solverResultLimit":
            {
                value = instance.m_solverResultLimit;
                return true;
            }
            case "m_removeWhenBroken":
            case "removeWhenBroken":
            {
                value = instance.m_removeWhenBroken;
                return true;
            }
            case "m_revertBackVelocityOnBreak":
            case "revertBackVelocityOnBreak":
            {
                value = instance.m_revertBackVelocityOnBreak;
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
            case "m_constraintData":
            case "constraintData":
            {
                if (value is null)
                {
                    instance.m_constraintData = default;
                    return true;
                }
                if (value is Havok.hk2015.hkpConstraintData castValue)
                {
                    instance.m_constraintData = castValue;
                    return true;
                }
                return false;
            }
            case "m_atoms":
            case "atoms":
            {
                if (value is not Havok.hk2015.hkpBridgeAtoms castValue) return false;
                instance.m_atoms = castValue;
                return true;
            }
            case "m_solverResultLimit":
            case "solverResultLimit":
            {
                if (value is not float castValue) return false;
                instance.m_solverResultLimit = castValue;
                return true;
            }
            case "m_removeWhenBroken":
            case "removeWhenBroken":
            {
                if (value is not bool castValue) return false;
                instance.m_removeWhenBroken = castValue;
                return true;
            }
            case "m_revertBackVelocityOnBreak":
            case "revertBackVelocityOnBreak":
            {
                if (value is not bool castValue) return false;
                instance.m_revertBackVelocityOnBreak = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
