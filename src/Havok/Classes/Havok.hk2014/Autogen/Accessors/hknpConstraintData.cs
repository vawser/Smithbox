// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hknpConstraintData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hknpConstraint>
{
    public hknpConstraintData(Havok.hk2014.hknpConstraint instance) : base(instance) { }

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
            case "m_flags":
            case "flags":
            {
                value = instance.m_flags;
                return true;
            }
            case "m_type":
            case "type":
            {
                value = instance.m_type;
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
            case "m_numSolverElemTemps":
            case "numSolverElemTemps":
            {
                value = instance.m_numSolverElemTemps;
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
                if (value is not uint castValue) return false;
                instance.m_bodyIdA = castValue;
                return true;
            }
            case "m_bodyIdB":
            case "bodyIdB":
            {
                if (value is not uint castValue) return false;
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
                if (value is Havok.hk2014.hkpConstraintData castValue)
                {
                    instance.m_data = castValue;
                    return true;
                }
                return false;
            }
            case "m_id":
            case "id":
            {
                if (value is not uint castValue) return false;
                instance.m_id = castValue;
                return true;
            }
            case "m_flags":
            case "flags":
            {
                if (value is Havok.hk2014.hknpConstraint.FlagsEnum castValue)
                {
                    instance.m_flags = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_flags = (Havok.hk2014.hknpConstraint.FlagsEnum)byteValue;
                    return true;
                }
                return false;
            }
            case "m_type":
            case "type":
            {
                if (value is Havok.hk2014.hkpConstraintData.ConstraintType castValue)
                {
                    instance.m_type = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_type = (Havok.hk2014.hkpConstraintData.ConstraintType)byteValue;
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
