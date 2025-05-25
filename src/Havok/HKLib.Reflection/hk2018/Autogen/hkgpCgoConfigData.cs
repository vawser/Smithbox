// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;
using HKLib.hk2018.hkgpCgo;

namespace HKLib.Reflection.hk2018;

internal class hkgpCgoConfigData : HavokData<Config> 
{
    public hkgpCgoConfigData(HavokType type, Config instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_propertyBag":
            case "propertyBag":
            {
                if (instance.m_propertyBag is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_semantic":
            case "semantic":
            {
                if (instance.m_semantic is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_semantic is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_combinator":
            case "combinator":
            {
                if (instance.m_combinator is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_combinator is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_maxDistance":
            case "maxDistance":
            {
                if (instance.m_maxDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxShrink":
            case "maxShrink":
            {
                if (instance.m_maxShrink is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxAngle":
            case "maxAngle":
            {
                if (instance.m_maxAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minEdgeRatio":
            case "minEdgeRatio":
            {
                if (instance.m_minEdgeRatio is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxAngleDrift":
            case "maxAngleDrift":
            {
                if (instance.m_maxAngleDrift is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_weldDistance":
            case "weldDistance":
            {
                if (instance.m_weldDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_updateThreshold":
            case "updateThreshold":
            {
                if (instance.m_updateThreshold is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_degenerateTolerance":
            case "degenerateTolerance":
            {
                if (instance.m_degenerateTolerance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_flatAngle":
            case "flatAngle":
            {
                if (instance.m_flatAngle is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxVertices":
            case "maxVertices":
            {
                if (instance.m_maxVertices is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_inverseOrientation":
            case "inverseOrientation":
            {
                if (instance.m_inverseOrientation is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_proportionalShrinking":
            case "proportionalShrinking":
            {
                if (instance.m_proportionalShrinking is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_multiPass":
            case "multiPass":
            {
                if (instance.m_multiPass is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_protectNakedBoundaries":
            case "protectNakedBoundaries":
            {
                if (instance.m_protectNakedBoundaries is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_protectMaterialBoundaries":
            case "protectMaterialBoundaries":
            {
                if (instance.m_protectMaterialBoundaries is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_decimateComponents":
            case "decimateComponents":
            {
                if (instance.m_decimateComponents is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_solverAccuracy":
            case "solverAccuracy":
            {
                if (instance.m_solverAccuracy is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_solverAccuracy is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_minDistance":
            case "minDistance":
            {
                if (instance.m_minDistance is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_minConvergence":
            case "minConvergence":
            {
                if (instance.m_minConvergence is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_project":
            case "project":
            {
                if (instance.m_project is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_buildClusters":
            case "buildClusters":
            {
                if (instance.m_buildClusters is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_useAccumulatedError":
            case "useAccumulatedError":
            {
                if (instance.m_useAccumulatedError is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_useLegacySolver":
            case "useLegacySolver":
            {
                if (instance.m_useLegacySolver is not TGet castValue) return false;
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
            case "m_propertyBag":
            case "propertyBag":
            {
                if (value is not hkPropertyBag castValue) return false;
                instance.m_propertyBag = castValue;
                return true;
            }
            case "m_semantic":
            case "semantic":
            {
                if (value is Config.VertexSemantic castValue)
                {
                    instance.m_semantic = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_semantic = (Config.VertexSemantic)intValue;
                    return true;
                }
                return false;
            }
            case "m_combinator":
            case "combinator":
            {
                if (value is Config.VertexCombinator castValue)
                {
                    instance.m_combinator = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_combinator = (Config.VertexCombinator)intValue;
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
                if (value is Config.SolverAccuracy castValue)
                {
                    instance.m_solverAccuracy = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_solverAccuracy = (Config.SolverAccuracy)intValue;
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
