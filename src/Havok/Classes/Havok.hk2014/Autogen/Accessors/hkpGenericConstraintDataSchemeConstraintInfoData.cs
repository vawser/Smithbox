// Automatically Generated

#nullable enable

namespace Havok.hk2014.Accessors;

internal sealed class hkpGenericConstraintDataSchemeConstraintInfoData : Havok.Formats.HavokFieldAccess<Havok.hk2014.hkpGenericConstraintDataSchemeConstraintInfo>
{
    public hkpGenericConstraintDataSchemeConstraintInfoData(Havok.hk2014.hkpGenericConstraintDataSchemeConstraintInfo instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_maxSizeOfSchema":
            case "maxSizeOfSchema":
            {
                value = instance.m_maxSizeOfSchema;
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
            default:
            return false;
        }
    }

    public override bool TrySetField(string fieldName, object? value)
    {
        switch (fieldName)
        {
            case "m_maxSizeOfSchema":
            case "maxSizeOfSchema":
            {
                if (value is not int castValue) return false;
                instance.m_maxSizeOfSchema = castValue;
                return true;
            }
            case "m_sizeOfSchemas":
            case "sizeOfSchemas":
            {
                if (value is not int castValue) return false;
                instance.m_sizeOfSchemas = castValue;
                return true;
            }
            case "m_numSolverResults":
            case "numSolverResults":
            {
                if (value is not int castValue) return false;
                instance.m_numSolverResults = castValue;
                return true;
            }
            case "m_numSolverElemTemps":
            case "numSolverElemTemps":
            {
                if (value is not int castValue) return false;
                instance.m_numSolverElemTemps = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
