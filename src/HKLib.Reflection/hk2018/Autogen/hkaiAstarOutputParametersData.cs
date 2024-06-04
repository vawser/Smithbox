// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiAstarOutputParametersData : HavokData<hkaiAstarOutputParameters> 
{
    public hkaiAstarOutputParametersData(HavokType type, hkaiAstarOutputParameters instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_numIterations":
            case "numIterations":
            {
                if (instance.m_numIterations is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_goalIndex":
            case "goalIndex":
            {
                if (instance.m_goalIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_pathLength":
            case "pathLength":
            {
                if (instance.m_pathLength is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_status":
            case "status":
            {
                if (instance.m_status is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_status is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_terminationCause":
            case "terminationCause":
            {
                if (instance.m_terminationCause is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_terminationCause is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

    public override bool TrySetField<TSet>(string fieldName, TSet value)
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
                if (value is hkaiAstarOutputParameters.SearchStatus castValue)
                {
                    instance.m_status = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_status = (hkaiAstarOutputParameters.SearchStatus)byteValue;
                    return true;
                }
                return false;
            }
            case "m_terminationCause":
            case "terminationCause":
            {
                if (value is hkaiAstarOutputParameters.TerminationCause castValue)
                {
                    instance.m_terminationCause = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_terminationCause = (hkaiAstarOutputParameters.TerminationCause)byteValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
