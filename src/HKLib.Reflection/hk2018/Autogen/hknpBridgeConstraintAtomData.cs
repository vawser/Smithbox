// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpBridgeConstraintAtomData : HavokData<hknpBridgeConstraintAtom> 
{
    public hknpBridgeConstraintAtomData(HavokType type, hknpBridgeConstraintAtom instance) : base(type, instance) {}

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
            case "m_constraintData":
            case "constraintData":
            {
                if (instance.m_constraintData is null)
                {
                    return true;
                }
                if (instance.m_constraintData is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_numSolverResults":
            case "numSolverResults":
            {
                if (instance.m_numSolverResults is not TGet castValue) return false;
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
            case "m_constraintData":
            case "constraintData":
            {
                if (value is null)
                {
                    instance.m_constraintData = default;
                    return true;
                }
                if (value is hkpConstraintData castValue)
                {
                    instance.m_constraintData = castValue;
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
            default:
            return false;
        }
    }

}
