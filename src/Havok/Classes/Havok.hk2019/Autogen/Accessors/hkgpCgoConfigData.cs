// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkgpCgoConfigData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkgpCgo.Config>
{
    public hkgpCgoConfigData(Havok.hk2019.hkgpCgo.Config instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_semantic":
            case "semantic":
            {
                value = instance.m_semantic;
                return true;
            }
            case "m_combinator":
            case "combinator":
            {
                value = instance.m_combinator;
                return true;
            }
            case "m_maxDistance":
            case "maxDistance":
            {
                value = instance.m_maxDistance;
                return true;
            }
            case "m_maxShrink":
            case "maxShrink":
            {
                value = instance.m_maxShrink;
                return true;
            }
            case "m_maxAngle":
            case "maxAngle":
            {
                value = instance.m_maxAngle;
                return true;
            }
            case "m_minEdgeRatio":
            case "minEdgeRatio":
            {
                value = instance.m_minEdgeRatio;
                return true;
            }
            case "m_maxAngleDrift":
            case "maxAngleDrift":
            {
                value = instance.m_maxAngleDrift;
                return true;
            }
            case "m_weldDistance":
            case "weldDistance":
            {
                value = instance.m_weldDistance;
                return true;
            }
            case "m_updateThreshold":
            case "updateThreshold":
            {
                value = instance.m_updateThreshold;
                return true;
            }
            case "m_degenerateTolerance":
            case "degenerateTolerance":
            {
                value = instance.m_degenerateTolerance;
                return true;
            }
            case "m_flatAngle":
            case "flatAngle":
            {
                value = instance.m_flatAngle;
                return true;
            }
            case "m_maxVertices":
            case "maxVertices":
            {
                value = instance.m_maxVertices;
                return true;
            }
            case "m_inverseOrientation":
            case "inverseOrientation":
            {
                value = instance.m_inverseOrientation;
                return true;
            }
            case "m_proportionalShrinking":
            case "proportionalShrinking":
            {
                value = instance.m_proportionalShrinking;
                return true;
            }
            case "m_multiPass":
            case "multiPass":
            {
                value = instance.m_multiPass;
                return true;
            }
            case "m_protectNakedBoundaries":
            case "protectNakedBoundaries":
            {
                value = instance.m_protectNakedBoundaries;
                return true;
            }
            case "m_protectMaterialBoundaries":
            case "protectMaterialBoundaries":
            {
                value = instance.m_protectMaterialBoundaries;
                return true;
            }
            case "m_decimateComponents":
            case "decimateComponents":
            {
                value = instance.m_decimateComponents;
                return true;
            }
            case "m_solverAccuracy":
            case "solverAccuracy":
            {
                value = instance.m_solverAccuracy;
                return true;
            }
            case "m_minDistance":
            case "minDistance":
            {
                value = instance.m_minDistance;
                return true;
            }
            case "m_minConvergence":
            case "minConvergence":
            {
                value = instance.m_minConvergence;
                return true;
            }
            case "m_project":
            case "project":
            {
                value = instance.m_project;
                return true;
            }
            case "m_buildClusters":
            case "buildClusters":
            {
                value = instance.m_buildClusters;
                return true;
            }
            case "m_useAccumulatedError":
            case "useAccumulatedError":
            {
                value = instance.m_useAccumulatedError;
                return true;
            }
            case "m_useLegacySolver":
            case "useLegacySolver":
            {
                value = instance.m_useLegacySolver;
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
            case "m_semantic":
            case "semantic":
            {
                if (value is Havok.hk2019.hkgpCgo.Config.VertexSemantic castValue)
                {
                    instance.m_semantic = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_semantic = (Havok.hk2019.hkgpCgo.Config.VertexSemantic)intValue;
                    return true;
                }
                return false;
            }
            case "m_combinator":
            case "combinator":
            {
                if (value is Havok.hk2019.hkgpCgo.Config.VertexCombinator castValue)
                {
                    instance.m_combinator = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_combinator = (Havok.hk2019.hkgpCgo.Config.VertexCombinator)intValue;
                    return true;
                }
                return false;
            }
            case "m_maxDistance":
            case "maxDistance":
            {
                if (value is not float castValue) return false;
                instance.m_maxDistance = castValue;
                return true;
            }
            case "m_maxShrink":
            case "maxShrink":
            {
                if (value is not float castValue) return false;
                instance.m_maxShrink = castValue;
                return true;
            }
            case "m_maxAngle":
            case "maxAngle":
            {
                if (value is not float castValue) return false;
                instance.m_maxAngle = castValue;
                return true;
            }
            case "m_minEdgeRatio":
            case "minEdgeRatio":
            {
                if (value is not float castValue) return false;
                instance.m_minEdgeRatio = castValue;
                return true;
            }
            case "m_maxAngleDrift":
            case "maxAngleDrift":
            {
                if (value is not float castValue) return false;
                instance.m_maxAngleDrift = castValue;
                return true;
            }
            case "m_weldDistance":
            case "weldDistance":
            {
                if (value is not float castValue) return false;
                instance.m_weldDistance = castValue;
                return true;
            }
            case "m_updateThreshold":
            case "updateThreshold":
            {
                if (value is not float castValue) return false;
                instance.m_updateThreshold = castValue;
                return true;
            }
            case "m_degenerateTolerance":
            case "degenerateTolerance":
            {
                if (value is not float castValue) return false;
                instance.m_degenerateTolerance = castValue;
                return true;
            }
            case "m_flatAngle":
            case "flatAngle":
            {
                if (value is not float castValue) return false;
                instance.m_flatAngle = castValue;
                return true;
            }
            case "m_maxVertices":
            case "maxVertices":
            {
                if (value is not int castValue) return false;
                instance.m_maxVertices = castValue;
                return true;
            }
            case "m_inverseOrientation":
            case "inverseOrientation":
            {
                if (value is not bool castValue) return false;
                instance.m_inverseOrientation = castValue;
                return true;
            }
            case "m_proportionalShrinking":
            case "proportionalShrinking":
            {
                if (value is not bool castValue) return false;
                instance.m_proportionalShrinking = castValue;
                return true;
            }
            case "m_multiPass":
            case "multiPass":
            {
                if (value is not bool castValue) return false;
                instance.m_multiPass = castValue;
                return true;
            }
            case "m_protectNakedBoundaries":
            case "protectNakedBoundaries":
            {
                if (value is not bool castValue) return false;
                instance.m_protectNakedBoundaries = castValue;
                return true;
            }
            case "m_protectMaterialBoundaries":
            case "protectMaterialBoundaries":
            {
                if (value is not bool castValue) return false;
                instance.m_protectMaterialBoundaries = castValue;
                return true;
            }
            case "m_decimateComponents":
            case "decimateComponents":
            {
                if (value is not bool castValue) return false;
                instance.m_decimateComponents = castValue;
                return true;
            }
            case "m_solverAccuracy":
            case "solverAccuracy":
            {
                if (value is Havok.hk2019.hkgpCgo.Config.SolverAccuracy castValue)
                {
                    instance.m_solverAccuracy = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_solverAccuracy = (Havok.hk2019.hkgpCgo.Config.SolverAccuracy)intValue;
                    return true;
                }
                return false;
            }
            case "m_minDistance":
            case "minDistance":
            {
                if (value is not float castValue) return false;
                instance.m_minDistance = castValue;
                return true;
            }
            case "m_minConvergence":
            case "minConvergence":
            {
                if (value is not float castValue) return false;
                instance.m_minConvergence = castValue;
                return true;
            }
            case "m_project":
            case "project":
            {
                if (value is not bool castValue) return false;
                instance.m_project = castValue;
                return true;
            }
            case "m_buildClusters":
            case "buildClusters":
            {
                if (value is not bool castValue) return false;
                instance.m_buildClusters = castValue;
                return true;
            }
            case "m_useAccumulatedError":
            case "useAccumulatedError":
            {
                if (value is not bool castValue) return false;
                instance.m_useAccumulatedError = castValue;
                return true;
            }
            case "m_useLegacySolver":
            case "useLegacySolver":
            {
                if (value is not bool castValue) return false;
                instance.m_useLegacySolver = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
