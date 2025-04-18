// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiNavMeshEdgeMatchingParametersData : HavokData<hkaiNavMeshEdgeMatchingParameters> 
{
    public hkaiNavMeshEdgeMatchingParametersData(HavokType type, hkaiNavMeshEdgeMatchingParameters instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_maxStepHeight":
            case "maxStepHeight":
            {
                if (instance.m_maxStepHeight is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxSeparation":
            case "maxSeparation":
            {
                if (instance.m_maxSeparation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxOverhang":
            case "maxOverhang":
            {
                if (instance.m_maxOverhang is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_behindFaceTolerance":
            case "behindFaceTolerance":
            {
                if (instance.m_behindFaceTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cosPlanarAlignmentAngle":
            case "cosPlanarAlignmentAngle":
            {
                if (instance.m_cosPlanarAlignmentAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cosVerticalAlignmentAngle":
            case "cosVerticalAlignmentAngle":
            {
                if (instance.m_cosVerticalAlignmentAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minEdgeOverlap":
            case "minEdgeOverlap":
            {
                if (instance.m_minEdgeOverlap is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeTraversibilityHorizontalEpsilon":
            case "edgeTraversibilityHorizontalEpsilon":
            {
                if (instance.m_edgeTraversibilityHorizontalEpsilon is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeTraversibilityVerticalEpsilon":
            case "edgeTraversibilityVerticalEpsilon":
            {
                if (instance.m_edgeTraversibilityVerticalEpsilon is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cosClimbingFaceNormalAlignmentAngle":
            case "cosClimbingFaceNormalAlignmentAngle":
            {
                if (instance.m_cosClimbingFaceNormalAlignmentAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_cosClimbingEdgeAlignmentAngle":
            case "cosClimbingEdgeAlignmentAngle":
            {
                if (instance.m_cosClimbingEdgeAlignmentAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minAngleBetweenFaces":
            case "minAngleBetweenFaces":
            {
                if (instance.m_minAngleBetweenFaces is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_edgeParallelTolerance":
            case "edgeParallelTolerance":
            {
                if (instance.m_edgeParallelTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_useSafeEdgeTraversibilityHorizontalEpsilon":
            case "useSafeEdgeTraversibilityHorizontalEpsilon":
            {
                if (instance.m_useSafeEdgeTraversibilityHorizontalEpsilon is not TGet castValue) return false;
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
            case "m_maxStepHeight":
            case "maxStepHeight":
            {
                if (value is not float castValue) return false;
                instance.m_maxStepHeight = castValue;
                return true;
            }
            case "m_maxSeparation":
            case "maxSeparation":
            {
                if (value is not float castValue) return false;
                instance.m_maxSeparation = castValue;
                return true;
            }
            case "m_maxOverhang":
            case "maxOverhang":
            {
                if (value is not float castValue) return false;
                instance.m_maxOverhang = castValue;
                return true;
            }
            case "m_behindFaceTolerance":
            case "behindFaceTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_behindFaceTolerance = castValue;
                return true;
            }
            case "m_cosPlanarAlignmentAngle":
            case "cosPlanarAlignmentAngle":
            {
                if (value is not float castValue) return false;
                instance.m_cosPlanarAlignmentAngle = castValue;
                return true;
            }
            case "m_cosVerticalAlignmentAngle":
            case "cosVerticalAlignmentAngle":
            {
                if (value is not float castValue) return false;
                instance.m_cosVerticalAlignmentAngle = castValue;
                return true;
            }
            case "m_minEdgeOverlap":
            case "minEdgeOverlap":
            {
                if (value is not float castValue) return false;
                instance.m_minEdgeOverlap = castValue;
                return true;
            }
            case "m_edgeTraversibilityHorizontalEpsilon":
            case "edgeTraversibilityHorizontalEpsilon":
            {
                if (value is not float castValue) return false;
                instance.m_edgeTraversibilityHorizontalEpsilon = castValue;
                return true;
            }
            case "m_edgeTraversibilityVerticalEpsilon":
            case "edgeTraversibilityVerticalEpsilon":
            {
                if (value is not float castValue) return false;
                instance.m_edgeTraversibilityVerticalEpsilon = castValue;
                return true;
            }
            case "m_cosClimbingFaceNormalAlignmentAngle":
            case "cosClimbingFaceNormalAlignmentAngle":
            {
                if (value is not float castValue) return false;
                instance.m_cosClimbingFaceNormalAlignmentAngle = castValue;
                return true;
            }
            case "m_cosClimbingEdgeAlignmentAngle":
            case "cosClimbingEdgeAlignmentAngle":
            {
                if (value is not float castValue) return false;
                instance.m_cosClimbingEdgeAlignmentAngle = castValue;
                return true;
            }
            case "m_minAngleBetweenFaces":
            case "minAngleBetweenFaces":
            {
                if (value is not float castValue) return false;
                instance.m_minAngleBetweenFaces = castValue;
                return true;
            }
            case "m_edgeParallelTolerance":
            case "edgeParallelTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_edgeParallelTolerance = castValue;
                return true;
            }
            case "m_useSafeEdgeTraversibilityHorizontalEpsilon":
            case "useSafeEdgeTraversibilityHorizontalEpsilon":
            {
                if (value is not bool castValue) return false;
                instance.m_useSafeEdgeTraversibilityHorizontalEpsilon = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
