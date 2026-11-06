// Automatically Generated

#nullable enable

namespace Havok.hk2019.Accessors;

internal sealed class hkaiLineOfSightUtilInputBaseData : Havok.Formats.HavokFieldAccess<Havok.hk2019.hkaiLineOfSightUtil.InputBase>
{
    public hkaiLineOfSightUtilInputBaseData(Havok.hk2019.hkaiLineOfSightUtil.InputBase instance) : base(instance) { }

    public override bool TryGetField(string fieldName, out object? value)
    {
        value = default;
        switch (fieldName)
        {
            case "m_startPoint":
            case "startPoint":
            {
                value = instance.m_startPoint;
                return true;
            }
            case "m_up":
            case "up":
            {
                value = instance.m_up;
                return true;
            }
            case "m_startFaceKey":
            case "startFaceKey":
            {
                value = instance.m_startFaceKey;
                return true;
            }
            case "m_maxNumberOfIterations":
            case "maxNumberOfIterations":
            {
                value = instance.m_maxNumberOfIterations;
                return true;
            }
            case "m_agentInfo":
            case "agentInfo":
            {
                value = instance.m_agentInfo;
                return true;
            }
            case "m_searchRadius":
            case "searchRadius":
            {
                value = instance.m_searchRadius;
                return true;
            }
            case "m_maximumPathLength":
            case "maximumPathLength":
            {
                value = instance.m_maximumPathLength;
                return true;
            }
            case "m_costModifier":
            case "costModifier":
            {
                value = instance.m_costModifier;
                return true;
            }
            case "m_edgeFilter":
            case "edgeFilter":
            {
                value = instance.m_edgeFilter;
                return true;
            }
            case "m_outputEdgesOnFailure":
            case "outputEdgesOnFailure":
            {
                value = instance.m_outputEdgesOnFailure;
                return true;
            }
            case "m_projectedRadiusCheck":
            case "projectedRadiusCheck":
            {
                value = instance.m_projectedRadiusCheck;
                return true;
            }
            case "m_exactInternalVertexHandling":
            case "exactInternalVertexHandling":
            {
                value = instance.m_exactInternalVertexHandling;
                return true;
            }
            case "m_isWallClimbing":
            case "isWallClimbing":
            {
                value = instance.m_isWallClimbing;
                return true;
            }
            case "m_mode":
            case "mode":
            {
                value = instance.m_mode;
                return true;
            }
            case "m_userEdgeHandling":
            case "userEdgeHandling":
            {
                value = instance.m_userEdgeHandling;
                return true;
            }
            case "m_ignoreBackfacingEdges":
            case "ignoreBackfacingEdges":
            {
                value = instance.m_ignoreBackfacingEdges;
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
                if (value is not Havok.hk2019.hkaiAgentTraversalInfo castValue) return false;
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
                if (value is Havok.hk2019.hkaiAstarCostModifier castValue)
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
                if (value is Havok.hk2019.hkaiAstarEdgeFilter castValue)
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
                if (value is Havok.hk2019.hkaiLineOfSightUtil.InputBase.QueryMode castValue)
                {
                    instance.m_mode = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_mode = (Havok.hk2019.hkaiLineOfSightUtil.InputBase.QueryMode)byteValue;
                    return true;
                }
                return false;
            }
            case "m_userEdgeHandling":
            case "userEdgeHandling":
            {
                if (value is Havok.hk2019.hkaiLineOfSightUtil.UserEdgeFlagBits castValue)
                {
                    instance.m_userEdgeHandling = castValue;
                    return true;
                }
                if (value is byte byteValue)
                {
                    instance.m_userEdgeHandling = (Havok.hk2019.hkaiLineOfSightUtil.UserEdgeFlagBits)byteValue;
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
            default:
            return false;
        }
    }

}
