// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshPathSearchParametersData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshPathSearchParameters>
{
    public hkaiNavMeshPathSearchParametersData(Havok.hk2019.hkaiNavMeshPathSearchParameters instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_up":
            case "up":
            {
                value = instance.m_up;
                return true;
            }
            case "m_validateInputs":
            case "validateInputs":
            {
                value = instance.m_validateInputs;
                return true;
            }
            case "m_outputPathFlags":
            case "outputPathFlags":
            {
                value = instance.m_outputPathFlags;
                return true;
            }
            case "m_lineOfSightType":
            case "lineOfSightType":
            {
                value = instance.m_lineOfSightType;
                return true;
            }
            case "m_handleInternalVertices":
            case "handleInternalVertices":
            {
                value = instance.m_handleInternalVertices;
                return true;
            }
            case "m_useHierarchicalHeuristic":
            case "useHierarchicalHeuristic":
            {
                value = instance.m_useHierarchicalHeuristic;
                return true;
            }
            case "m_projectedRadiusCheck":
            case "projectedRadiusCheck":
            {
                value = instance.m_projectedRadiusCheck;
                return true;
            }
            case "m_useGrandparentDistanceCalculation":
            case "useGrandparentDistanceCalculation":
            {
                value = instance.m_useGrandparentDistanceCalculation;
                return true;
            }
            case "m_outputUnreachablePaths":
            case "outputUnreachablePaths":
            {
                value = instance.m_outputUnreachablePaths;
                return true;
            }
            case "m_recordClearanceCacheMisses":
            case "recordClearanceCacheMisses":
            {
                value = instance.m_recordClearanceCacheMisses;
                return true;
            }
            case "m_heuristicWeight":
            case "heuristicWeight":
            {
                value = instance.m_heuristicWeight;
                return true;
            }
            case "m_simpleRadiusThreshold":
            case "simpleRadiusThreshold":
            {
                value = instance.m_simpleRadiusThreshold;
                return true;
            }
            case "m_maximumPathLength":
            case "maximumPathLength":
            {
                value = instance.m_maximumPathLength;
                return true;
            }
            case "m_searchSphereRadius":
            case "searchSphereRadius":
            {
                value = instance.m_searchSphereRadius;
                return true;
            }
            case "m_searchCapsuleRadius":
            case "searchCapsuleRadius":
            {
                value = instance.m_searchCapsuleRadius;
                return true;
            }
            case "m_bufferSizes":
            case "bufferSizes":
            {
                value = instance.m_bufferSizes;
                return true;
            }
            case "m_hierarchyBufferSizes":
            case "hierarchyBufferSizes":
            {
                value = instance.m_hierarchyBufferSizes;
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
                if (value is Havok.hk2019.hkaiNavMeshPathSearchParameters.OutputPathFlags castValue)
                {
                    instance.m_outputPathFlags = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_outputPathFlags = (Havok.hk2019.hkaiNavMeshPathSearchParameters.OutputPathFlags)byteValue;
                    return true;
                }
                return false;
            }
            case "m_lineOfSightType":
            case "lineOfSightType":
            {
                if (value is Havok.hk2019.hkaiNavMeshPathSearchParameters.LineOfSightType castValue)
                {
                    instance.m_lineOfSightType = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_lineOfSightType = (Havok.hk2019.hkaiNavMeshPathSearchParameters.LineOfSightType)byteValue;
                    return true;
                }
                return false;
            }
            case "m_handleInternalVertices":
            case "handleInternalVertices":
            {
                if (value is not bool castValue) return false;
                instance.m_handleInternalVertices = castValue;
                return true;
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
                if (value is not Havok.hk2019.hkaiSearchParameters.BufferSizes castValue) return false;
                instance.m_bufferSizes = castValue;
                return true;
            }
            case "m_hierarchyBufferSizes":
            case "hierarchyBufferSizes":
            {
                if (value is not Havok.hk2019.hkaiSearchParameters.BufferSizes castValue) return false;
                instance.m_hierarchyBufferSizes = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
