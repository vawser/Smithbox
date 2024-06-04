// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hknpConstraintData : HavokData<hknpConstraint> 
{
    public hknpConstraintData(HavokType type, hknpConstraint instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bodyIdA":
            case "bodyIdA":
            {
                if (instance.m_bodyIdA is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bodyIdB":
            case "bodyIdB":
            {
                if (instance.m_bodyIdB is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_data":
            case "data":
            {
                if (instance.m_data is null)
                {
                    return true;
                }
                if (instance.m_data is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_id":
            case "id":
            {
                if (instance.m_id is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_groupId":
            case "groupId":
            {
                if (instance.m_groupId is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nextInGroup":
            case "nextInGroup":
            {
                if (instance.m_nextInGroup is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_prevInGroup":
            case "prevInGroup":
            {
                if (instance.m_prevInGroup is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (instance.m_flags is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((ushort)instance.m_flags is TGet ushortValue)
                {
                    value = ushortValue;
                    return true;
                }
                return false;
            }
            case "m_sizeOfAtoms":
            case "sizeOfAtoms":
            {
                if (instance.m_sizeOfAtoms is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_sizeOfSchemas":
            case "sizeOfSchemas":
            {
                if (instance.m_sizeOfSchemas is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numSolverResults":
            case "numSolverResults":
            {
                if (instance.m_numSolverResults is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_numSolverElemTemps":
            case "numSolverElemTemps":
            {
                if (instance.m_numSolverElemTemps is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_runtimeSize":
            case "runtimeSize":
            {
                if (instance.m_runtimeSize is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (instance.m_userData is not TGet castValue) return false;
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
            case "m_bodyIdA":
            case "bodyIdA":
            {
                if (value is not hknpBodyId castValue) return false;
                instance.m_bodyIdA = castValue;
                return true;
            }
            case "m_bodyIdB":
            case "bodyIdB":
            {
                if (value is not hknpBodyId castValue) return false;
                instance.m_bodyIdB = castValue;
                return true;
            }
            case "m_data":
            case "data":
            {
                if (value is null)
                {
                    instance.m_data = default;
                    return true;
                }
                if (value is hkpConstraintData castValue)
                {
                    instance.m_data = castValue;
                    return true;
                }
                return false;
            }
            case "m_id":
            case "id":
            {
                if (value is not hknpConstraintId castValue) return false;
                instance.m_id = castValue;
                return true;
            }
            case "m_groupId":
            case "groupId":
            {
                if (value is not hknpConstraintGroupId castValue) return false;
                instance.m_groupId = castValue;
                return true;
            }
            case "m_nextInGroup":
            case "nextInGroup":
            {
                if (value is not hknpConstraintId castValue) return false;
                instance.m_nextInGroup = castValue;
                return true;
            }
            case "m_prevInGroup":
            case "prevInGroup":
            {
                if (value is not hknpConstraintId castValue) return false;
                instance.m_prevInGroup = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is hknpConstraint.FlagsEnum castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_flags = (hknpConstraint.FlagsEnum)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_sizeOfAtoms":
            case "sizeOfAtoms":
            {
                if (value is not ushort castValue) return false;
                instance.m_sizeOfAtoms = castValue;
                return true;
            }
            case "m_sizeOfSchemas":
            case "sizeOfSchemas":
            {
                if (value is not ushort castValue) return false;
                instance.m_sizeOfSchemas = castValue;
                return true;
            }
            case "m_numSolverResults":
            case "numSolverResults":
            {
                if (value is not byte castValue) return false;
                instance.m_numSolverResults = castValue;
                return true;
            }
            case "m_numSolverElemTemps":
            case "numSolverElemTemps":
            {
                if (value is not byte castValue) return false;
                instance.m_numSolverElemTemps = castValue;
                return true;
            }
            case "m_runtimeSize":
            case "runtimeSize":
            {
                if (value is not ushort castValue) return false;
                instance.m_runtimeSize = castValue;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                if (value is not ulong castValue) return false;
                instance.m_userData = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
