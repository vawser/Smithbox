// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiAstarOutputParametersData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiAstarOutputParameters>
{
    public hkaiAstarOutputParametersData(Havok.hk2019.hkaiAstarOutputParameters instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_numIterations":
            case "numIterations":
            {
                value = instance.m_numIterations;
                return true;
            }
            case "m_goalIndex":
            case "goalIndex":
            {
                value = instance.m_goalIndex;
                return true;
            }
            case "m_pathLength":
            case "pathLength":
            {
                value = instance.m_pathLength;
                return true;
            }
            case "m_status":
            case "status":
            {
                value = instance.m_status;
                return true;
            }
            case "m_terminationCause":
            case "terminationCause":
            {
                value = instance.m_terminationCause;
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
            case "m_numIterations":
            case "numIterations":
            {
                if (value is not int castValue) return false;
                instance.m_numIterations = castValue;
                return true;
            }
            case "m_goalIndex":
            case "goalIndex":
            {
                if (value is not int castValue) return false;
                instance.m_goalIndex = castValue;
                return true;
            }
            case "m_pathLength":
            case "pathLength":
            {
                if (value is not float castValue) return false;
                instance.m_pathLength = castValue;
                return true;
            }
            case "m_status":
            case "status":
            {
                if (value is Havok.hk2019.hkaiAstarOutputParameters.SearchStatus castValue)
                {
                    instance.m_status = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_status = (Havok.hk2019.hkaiAstarOutputParameters.SearchStatus)byteValue;
                    return true;
                }
                return false;
            }
            case "m_terminationCause":
            case "terminationCause":
            {
                if (value is Havok.hk2019.hkaiAstarOutputParameters.TerminationCause castValue)
                {
                    instance.m_terminationCause = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_terminationCause = (Havok.hk2019.hkaiAstarOutputParameters.TerminationCause)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
