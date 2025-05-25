// Automatically Generated

using System.Diagnostics.CodeAnalysis;
using HKLib.hk2018;

namespace HKLib.Reflection.hk2018;

internal class hkaiLineOfSightUtilLineOfSightInputData : HavokData<hkaiLineOfSightUtil.LineOfSightInput> 
{
    public hkaiLineOfSightUtilLineOfSightInputData(HavokType type, hkaiLineOfSightUtil.LineOfSightInput instance) : base(type, instance) {}

    public override bool TryGetField<TGet>(string fieldName, [MaybeNull] out TGet value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_startPoint":
            case "startPoint":
            {
                if (instance.m_startPoint is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_up":
            case "up":
            {
                if (instance.m_up is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_startFaceKey":
            case "startFaceKey":
            {
                if (instance.m_startFaceKey is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_maxNumberOfIterations":
            case "maxNumberOfIterations":
            {
                if (instance.m_maxNumberOfIterations is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_agentInfo":
            case "agentInfo":
            {
                if (instance.m_agentInfo is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_searchRadius":
            case "searchRadius":
            {
                if (instance.m_searchRadius is not TGet castValue) return false;
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
            case "m_costModifier":
            case "costModifier":
            {
                if (instance.m_costModifier is null)
                {
                    return true;
                }
                if (instance.m_costModifier is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_edgeFilter":
            case "edgeFilter":
            {
                if (instance.m_edgeFilter is null)
                {
                    return true;
                }
                if (instance.m_edgeFilter is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_outputEdgesOnFailure":
            case "outputEdgesOnFailure":
            {
                if (instance.m_outputEdgesOnFailure is not TGet castValue) return false;
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
            case "m_exactInternalVertexHandling":
            case "exactInternalVertexHandling":
            {
                if (instance.m_exactInternalVertexHandling is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_isWallClimbing":
            case "isWallClimbing":
            {
                if (instance.m_isWallClimbing is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_mode":
            case "mode":
            {
                if (instance.m_mode is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_mode is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_userEdgeHandling":
            case "userEdgeHandling":
            {
                if (instance.m_userEdgeHandling is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((byte)instance.m_userEdgeHandling is TGet byteValue)
                {
                    value = byteValue;
                    return true;
                }
                return false;
            }
            case "m_ignoreBackfacingEdges":
            case "ignoreBackfacingEdges":
            {
                if (instance.m_ignoreBackfacingEdges is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_goalPoint":
            case "goalPoint":
            {
                if (instance.m_goalPoint is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_goalFaceKey":
            case "goalFaceKey":
            {
                if (instance.m_goalFaceKey is not TGet castValue) return false;
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
            case "m_startPoint":
            case "startPoint":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_startPoint = castValue;
                return true;
            }
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            case "m_startFaceKey":
            case "startFaceKey":
            {
                if (value is not uint castValue) return false;
                instance.m_startFaceKey = castValue;
                return true;
            }
            case "m_maxNumberOfIterations":
            case "maxNumberOfIterations":
            {
                if (value is not int castValue) return false;
                instance.m_maxNumberOfIterations = castValue;
                return true;
            }
            case "m_agentInfo":
            case "agentInfo":
            {
                if (value is not hkaiAgentTraversalInfo castValue) return false;
                instance.m_agentInfo = castValue;
                return true;
            }
            case "m_searchRadius":
            case "searchRadius":
            {
                if (value is not float castValue) return false;
                instance.m_searchRadius = castValue;
                return true;
            }
            case "m_maximumPathLength":
            case "maximumPathLength":
            {
                if (value is not float castValue) return false;
                instance.m_maximumPathLength = castValue;
                return true;
            }
            case "m_costModifier":
            case "costModifier":
            {
                if (value is null)
                {
                    instance.m_costModifier = default;
                    return true;
                }
                if (value is hkaiAstarCostModifier castValue)
                {
                    instance.m_costModifier = castValue;
                    return true;
                }
                return false;
            }
            case "m_edgeFilter":
            case "edgeFilter":
            {
                if (value is null)
                {
                    instance.m_edgeFilter = default;
                    return true;
                }
                if (value is hkaiAstarEdgeFilter castValue)
                {
                    instance.m_edgeFilter = castValue;
                    return true;
                }
                return false;
            }
            case "m_outputEdgesOnFailure":
            case "outputEdgesOnFailure":
            {
                if (value is not bool castValue) return false;
                instance.m_outputEdgesOnFailure = castValue;
                return true;
            }
            case "m_projectedRadiusCheck":
            case "projectedRadiusCheck":
            {
                if (value is not bool castValue) return false;
                instance.m_projectedRadiusCheck = castValue;
                return true;
            }
            case "m_exactInternalVertexHandling":
            case "exactInternalVertexHandling":
            {
                if (value is not bool castValue) return false;
                instance.m_exactInternalVertexHandling = castValue;
                return true;
            }
            case "m_isWallClimbing":
            case "isWallClimbing":
            {
                if (value is not bool castValue) return false;
                instance.m_isWallClimbing = castValue;
                return true;
            }
            case "m_mode":
            case "mode":
            {
                if (value is hkaiLineOfSightUtil.InputBase.QueryMode castValue)
                {
                    instance.m_mode = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_mode = (hkaiLineOfSightUtil.InputBase.QueryMode)byteValue;
                    return true;
                }
                return false;
            }
            case "m_userEdgeHandling":
            case "userEdgeHandling":
            {
                if (value is hkaiLineOfSightUtil.UserEdgeFlagBits castValue)
                {
                    instance.m_userEdgeHandling = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_userEdgeHandling = (hkaiLineOfSightUtil.UserEdgeFlagBits)byteValue;
                    return true;
                }
                return false;
            }
            case "m_ignoreBackfacingEdges":
            case "ignoreBackfacingEdges":
            {
                if (value is not bool castValue) return false;
                instance.m_ignoreBackfacingEdges = castValue;
                return true;
            }
            case "m_goalPoint":
            case "goalPoint":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_goalPoint = castValue;
                return true;
            }
            case "m_goalFaceKey":
            case "goalFaceKey":
            {
                if (value is not uint castValue) return false;
                instance.m_goalFaceKey = castValue;
                return true;
            }
            default:
            return false;
        }
    }

}
