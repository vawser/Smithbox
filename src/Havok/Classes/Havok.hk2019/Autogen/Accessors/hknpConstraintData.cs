// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hknpConstraintData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hknpConstraint>
{
    public hknpConstraintData(Havok.hk2019.hknpConstraint instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_bodyIdA":
            case "bodyIdA":
            {
                value = instance.m_bodyIdA;
                return true;
            }
            case "m_bodyIdB":
            case "bodyIdB":
            {
                value = instance.m_bodyIdB;
                return true;
            }
            case "m_data":
            case "data":
            {
                value = instance.m_data;
                return true;
            }
            case "m_id":
            case "id":
            {
                value = instance.m_id;
                return true;
            }
            case "m_groupId":
            case "groupId":
            {
                value = instance.m_groupId;
                return true;
            }
            case "m_nextInGroup":
            case "nextInGroup":
            {
                value = instance.m_nextInGroup;
                return true;
            }
            case "m_prevInGroup":
            case "prevInGroup":
            {
                value = instance.m_prevInGroup;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_desiredGroupId":
            case "desiredGroupId":
            {
                value = instance.m_desiredGroupId;
                return true;
            }
            case "m_sizeOfAtoms":
            case "sizeOfAtoms":
            {
                value = instance.m_sizeOfAtoms;
                return true;
            }
            case "m_sizeOfSchemas":
            case "sizeOfSchemas":
            {
                value = instance.m_sizeOfSchemas;
                return true;
            }
            case "m_numSolverResults":
            case "numSolverResults":
            {
                value = instance.m_numSolverResults;
                return true;
            }
            case "m_numSolverElemTempsAndDirectSolverInformation":
            case "numSolverElemTempsAndDirectSolverInformation":
            {
                value = instance.m_numSolverElemTempsAndDirectSolverInformation;
                return true;
            }
            case "m_runtimeSize":
            case "runtimeSize":
            {
                value = instance.m_runtimeSize;
                return true;
            }
            case "m_userData":
            case "userData":
            {
                value = instance.m_userData;
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
            case "m_bodyIdA":
            case "bodyIdA":
            {
                if (value is not Havok.hk2019.hknpBodyId castValue) return false;
                instance.m_bodyIdA = castValue;
                return true;
            }
            case "m_bodyIdB":
            case "bodyIdB":
            {
                if (value is not Havok.hk2019.hknpBodyId castValue) return false;
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
                if (value is Havok.hk2019.hkpConstraintData castValue)
                {
                    instance.m_data = castValue;
                    return true;
                }
                return false;
            }
            case "m_id":
            case "id":
            {
                if (value is not Havok.hk2019.hknpConstraintId castValue) return false;
                instance.m_id = castValue;
                return true;
            }
            case "m_groupId":
            case "groupId":
            {
                if (value is not Havok.hk2019.hknpConstraintGroupId castValue) return false;
                instance.m_groupId = castValue;
                return true;
            }
            case "m_nextInGroup":
            case "nextInGroup":
            {
                if (value is not Havok.hk2019.hknpConstraintId castValue) return false;
                instance.m_nextInGroup = castValue;
                return true;
            }
            case "m_prevInGroup":
            case "prevInGroup":
            {
                if (value is not Havok.hk2019.hknpConstraintId castValue) return false;
                instance.m_prevInGroup = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is Havok.hk2019.hknpConstraint.FlagsEnum castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is ushort ushortValue)
                {
                    instance.m_flags = (Havok.hk2019.hknpConstraint.FlagsEnum)ushortValue;
                    return true;
                }
                return false;
            }
            case "m_desiredGroupId":
            case "desiredGroupId":
            {
                if (value is not Havok.hk2019.hknpConstraintGroupId castValue) return false;
                instance.m_desiredGroupId = castValue;
                return true;
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
            case "m_numSolverElemTempsAndDirectSolverInformation":
            case "numSolverElemTempsAndDirectSolverInformation":
            {
                if (value is not byte castValue) return false;
                instance.m_numSolverElemTempsAndDirectSolverInformation = castValue;
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
