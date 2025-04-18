// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshPathSearchParametersData : HavokData<hkaiNavMeshPathSearchParameters> 
{
    public hkaiNavMeshPathSearchParametersData(HavokType type, hkaiNavMeshPathSearchParameters instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_up":
            case "up":
            {
                if (instance.m_up is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_validateInputs":
            case "validateInputs":
            {
                if (instance.m_validateInputs is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_outputPathFlags":
            case "outputPathFlags":
            {
                if (instance.m_outputPathFlags is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_outputPathFlags is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_lineOfSightFlags":
            case "lineOfSightFlags":
            {
                if (instance.m_lineOfSightFlags is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_lineOfSightFlags is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_useHierarchicalHeuristic":
            case "useHierarchicalHeuristic":
            {
                if (instance.m_useHierarchicalHeuristic is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_projectedRadiusCheck":
            case "projectedRadiusCheck":
            {
                if (instance.m_projectedRadiusCheck is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_useGrandparentDistanceCalculation":
            case "useGrandparentDistanceCalculation":
            {
                if (instance.m_useGrandparentDistanceCalculation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_outputUnreachablePaths":
            case "outputUnreachablePaths":
            {
                if (instance.m_outputUnreachablePaths is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_recordClearanceCacheMisses":
            case "recordClearanceCacheMisses":
            {
                if (instance.m_recordClearanceCacheMisses is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_heuristicWeight":
            case "heuristicWeight":
            {
                if (instance.m_heuristicWeight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_simpleRadiusThreshold":
            case "simpleRadiusThreshold":
            {
                if (instance.m_simpleRadiusThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maximumPathLength":
            case "maximumPathLength":
            {
                if (instance.m_maximumPathLength is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_searchSphereRadius":
            case "searchSphereRadius":
            {
                if (instance.m_searchSphereRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_searchCapsuleRadius":
            case "searchCapsuleRadius":
            {
                if (instance.m_searchCapsuleRadius is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_bufferSizes":
            case "bufferSizes":
            {
                if (instance.m_bufferSizes is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_hierarchyBufferSizes":
            case "hierarchyBufferSizes":
            {
                if (instance.m_hierarchyBufferSizes is not TGet castValue) return false;
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
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            case "m_validateInputs":
            case "validateInputs":
            {
                if (value is not bool castValue) return false;
                instance.m_validateInputs = castValue;
                return true;
            }
            case "m_outputPathFlags":
            case "outputPathFlags":
            {
                if (value is hkaiNavMeshPathSearchParameters.OutputPathFlags castValue)
                {
                    instance.m_outputPathFlags = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_outputPathFlags = (hkaiNavMeshPathSearchParameters.OutputPathFlags)byteValue;
                    return true;
                }
                return false;
            }
            case "m_lineOfSightFlags":
            case "lineOfSightFlags":
            {
                if (value is hkaiNavMeshPathSearchParameters.LineOfSightFlags castValue)
                {
                    instance.m_lineOfSightFlags = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_lineOfSightFlags = (hkaiNavMeshPathSearchParameters.LineOfSightFlags)byteValue;
                    return true;
                }
                return false;
            }
            case "m_useHierarchicalHeuristic":
            case "useHierarchicalHeuristic":
            {
                if (value is not bool castValue) return false;
                instance.m_useHierarchicalHeuristic = castValue;
                return true;
            }
            case "m_projectedRadiusCheck":
            case "projectedRadiusCheck":
            {
                if (value is not bool castValue) return false;
                instance.m_projectedRadiusCheck = castValue;
                return true;
            }
            case "m_useGrandparentDistanceCalculation":
            case "useGrandparentDistanceCalculation":
            {
                if (value is not bool castValue) return false;
                instance.m_useGrandparentDistanceCalculation = castValue;
                return true;
            }
            case "m_outputUnreachablePaths":
            case "outputUnreachablePaths":
            {
                if (value is not bool castValue) return false;
                instance.m_outputUnreachablePaths = castValue;
                return true;
            }
            case "m_recordClearanceCacheMisses":
            case "recordClearanceCacheMisses":
            {
                if (value is not bool castValue) return false;
                instance.m_recordClearanceCacheMisses = castValue;
                return true;
            }
            case "m_heuristicWeight":
            case "heuristicWeight":
            {
                if (value is not float castValue) return false;
                instance.m_heuristicWeight = castValue;
                return true;
            }
            case "m_simpleRadiusThreshold":
            case "simpleRadiusThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_simpleRadiusThreshold = castValue;
                return true;
            }
            case "m_maximumPathLength":
            case "maximumPathLength":
            {
                if (value is not float castValue) return false;
                instance.m_maximumPathLength = castValue;
                return true;
            }
            case "m_searchSphereRadius":
            case "searchSphereRadius":
            {
                if (value is not float castValue) return false;
                instance.m_searchSphereRadius = castValue;
                return true;
            }
            case "m_searchCapsuleRadius":
            case "searchCapsuleRadius":
            {
                if (value is not float castValue) return false;
                instance.m_searchCapsuleRadius = castValue;
                return true;
            }
            case "m_bufferSizes":
            case "bufferSizes":
            {
                if (value is not hkaiSearchParameters.BufferSizes castValue) return false;
                instance.m_bufferSizes = castValue;
                return true;
            }
            case "m_hierarchyBufferSizes":
            case "hierarchyBufferSizes":
            {
                if (value is not hkaiSearchParameters.BufferSizes castValue) return false;
                instance.m_hierarchyBufferSizes = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
