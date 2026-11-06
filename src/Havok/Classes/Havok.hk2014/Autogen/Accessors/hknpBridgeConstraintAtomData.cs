// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpBridgeConstraintAtomData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpBridgeConstraintAtom>
{
    public hknpBridgeConstraintAtomData(Havok.hk2014.hknpBridgeConstraintAtom instance) : base(instance) { }

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
            case "m_numSolverResults":
            case "numSolverResults":
            {
                value = instance.m_numSolverResults;
                return true;
            }
            case "m_constraintData":
            case "constraintData":
            {
                value = instance.m_constraintData;
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
                if (value is Havok.hk2014.hkpConstraintAtom.AtomType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_type = (Havok.hk2014.hkpConstraintAtom.AtomType)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_numSolverResults":
            case "numSolverResults":
            {
                if (value is not int castValue) return false;
                instance.m_numSolverResults = castValue;
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
                if (value is Havok.hk2014.hkpConstraintData castValue)
                {
                    instance.m_constraintData = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
