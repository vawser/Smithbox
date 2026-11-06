// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiNavMeshEdgeMatchingParametersData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiNavMeshEdgeMatchingParameters>
{
    public hkaiNavMeshEdgeMatchingParametersData(Havok.hk2019.hkaiNavMeshEdgeMatchingParameters instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_maxStepHeight":
            case "maxStepHeight":
            {
                value = instance.m_maxStepHeight;
                return true;
            }
            case "m_maxSeparation":
            case "maxSeparation":
            {
                value = instance.m_maxSeparation;
                return true;
            }
            case "m_maxOverhang":
            case "maxOverhang":
            {
                value = instance.m_maxOverhang;
                return true;
            }
            case "m_behindFaceTolerance":
            case "behindFaceTolerance":
            {
                value = instance.m_behindFaceTolerance;
                return true;
            }
            case "m_cosPlanarAlignmentAngle":
            case "cosPlanarAlignmentAngle":
            {
                value = instance.m_cosPlanarAlignmentAngle;
                return true;
            }
            case "m_cosVerticalAlignmentAngle":
            case "cosVerticalAlignmentAngle":
            {
                value = instance.m_cosVerticalAlignmentAngle;
                return true;
            }
            case "m_minEdgeOverlap":
            case "minEdgeOverlap":
            {
                value = instance.m_minEdgeOverlap;
                return true;
            }
            case "m_edgeTraversibilityHorizontalEpsilon":
            case "edgeTraversibilityHorizontalEpsilon":
            {
                value = instance.m_edgeTraversibilityHorizontalEpsilon;
                return true;
            }
            case "m_edgeTraversibilityVerticalEpsilon":
            case "edgeTraversibilityVerticalEpsilon":
            {
                value = instance.m_edgeTraversibilityVerticalEpsilon;
                return true;
            }
            case "m_cosClimbingFaceNormalAlignmentAngle":
            case "cosClimbingFaceNormalAlignmentAngle":
            {
                value = instance.m_cosClimbingFaceNormalAlignmentAngle;
                return true;
            }
            case "m_cosClimbingEdgeAlignmentAngle":
            case "cosClimbingEdgeAlignmentAngle":
            {
                value = instance.m_cosClimbingEdgeAlignmentAngle;
                return true;
            }
            case "m_minAngleBetweenFaces":
            case "minAngleBetweenFaces":
            {
                value = instance.m_minAngleBetweenFaces;
                return true;
            }
            case "m_edgeParallelTolerance":
            case "edgeParallelTolerance":
            {
                value = instance.m_edgeParallelTolerance;
                return true;
            }
            case "m_useSafeEdgeTraversibilityHorizontalEpsilon":
            case "useSafeEdgeTraversibilityHorizontalEpsilon":
            {
                value = instance.m_useSafeEdgeTraversibilityHorizontalEpsilon;
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
