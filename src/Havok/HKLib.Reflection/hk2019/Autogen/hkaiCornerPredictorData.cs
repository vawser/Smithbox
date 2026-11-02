// Automatically Generated

using System.Diagnostics.CodeAnalysis;

namespace HKLib.Reflection.hk2019;

internal class hkaiCornerPredictorData : HavokData<HKLib.hk2019.hkaiCornerPredictor> 
{
    public hkaiCornerPredictorData(HavokType type, HKLib.hk2019.hkaiCornerPredictor instance) : base(type, instance) {}

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
            case "m_nextTravelVector":
            case "nextTravelVector":
            {
                if (instance.m_nextTravelVector is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nextEnterTurnPoint":
            case "nextEnterTurnPoint":
            {
                if (instance.m_nextEnterTurnPoint is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nextTransform":
            case "nextTransform":
            {
                if (instance.m_nextTransform is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nextEdgeIndex":
            case "nextEdgeIndex":
            {
                if (instance.m_nextEdgeIndex is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nextIsLeft":
            case "nextIsLeft":
            {
                if (instance.m_nextIsLeft is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_nextUserEdgeTraversals":
            case "nextUserEdgeTraversals":
            {
                if (instance.m_nextUserEdgeTraversals is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_lastFaceData":
            case "lastFaceData":
            {
                if (instance.m_lastFaceData is null)
                {
                    return true;
                }
                if (instance.m_lastFaceData is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_nextEdgeToCross":
            case "nextEdgeToCross":
            {
                if (instance.m_nextEdgeToCross is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_transformEdgesCrossed":
            case "transformEdgesCrossed":
            {
                if (instance.m_transformEdgesCrossed is not TGet castValue) return false;
                value = castValue;
                return true;
            }
            case "m_prevResult":
            case "prevResult":
            {
                if (instance.m_prevResult is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                if ((int)instance.m_prevResult is TGet intValue)
                {
                    value = intValue;
                    return true;
                }
                return false;
            }
            case "m_edgePath":
            case "edgePath":
            {
                if (instance.m_edgePath is null)
                {
                    return true;
                }
                if (instance.m_edgePath is TGet castValue)
                {
                    value = castValue;
                    return true;
                }
                return false;
            }
            case "m_streamingCollection":
            case "streamingCollection":
            {
                if (instance.m_streamingCollection is null)
                {
                    return true;
                }
                if (instance.m_streamingCollection is TGet castValue)
                {
                    value = castValue;
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
            case "m_up":
            case "up":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_up = castValue;
                return true;
            }
            case "m_nextTravelVector":
            case "nextTravelVector":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_nextTravelVector = castValue;
                return true;
            }
            case "m_nextEnterTurnPoint":
            case "nextEnterTurnPoint":
            {
                if (value is not Vector4 castValue) return false;
                instance.m_nextEnterTurnPoint = castValue;
                return true;
            }
            case "m_nextTransform":
            case "nextTransform":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_nextTransform = castValue;
                return true;
            }
            case "m_nextEdgeIndex":
            case "nextEdgeIndex":
            {
                if (value is not int castValue) return false;
                instance.m_nextEdgeIndex = castValue;
                return true;
            }
            case "m_nextIsLeft":
            case "nextIsLeft":
            {
                if (value is not uint castValue) return false;
                instance.m_nextIsLeft = castValue;
                return true;
            }
            case "m_nextUserEdgeTraversals":
            case "nextUserEdgeTraversals":
            {
                if (value is not List<HKLib.hk2019.hkaiCornerPredictor.UserEdgeTraversal> castValue) return false;
                instance.m_nextUserEdgeTraversals = castValue;
                return true;
            }
            case "m_lastFaceData":
            case "lastFaceData":
            {
                if (value is null)
                {
                    instance.m_lastFaceData = default;
                    return true;
                }
                if (value is object castValue)
                {
                    instance.m_lastFaceData = castValue;
                    return true;
                }
                return false;
            }
            case "m_nextEdgeToCross":
            case "nextEdgeToCross":
            {
                if (value is not int castValue) return false;
                instance.m_nextEdgeToCross = castValue;
                return true;
            }
            case "m_transformEdgesCrossed":
            case "transformEdgesCrossed":
            {
                if (value is not Matrix4x4 castValue) return false;
                instance.m_transformEdgesCrossed = castValue;
                return true;
            }
            case "m_prevResult":
            case "prevResult":
            {
                if (value is HKLib.hk2019.hkaiCornerPredictor.StepForwardResult castValue)
                {
                    instance.m_prevResult = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_prevResult = (HKLib.hk2019.hkaiCornerPredictor.StepForwardResult)intValue;
                    return true;
                }
                return false;
            }
            case "m_edgePath":
            case "edgePath":
            {
                if (value is null)
                {
                    instance.m_edgePath = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkaiEdgePath castValue)
                {
                    instance.m_edgePath = castValue;
                    return true;
                }
                return false;
            }
            case "m_streamingCollection":
            case "streamingCollection":
            {
                if (value is null)
                {
                    instance.m_streamingCollection = default;
                    return true;
                }
                if (value is HKLib.hk2019.hkaiStreamingCollection castValue)
                {
                    instance.m_streamingCollection = castValue;
                    return true;
                }
                return false;
            }
            default:
            return false;
        }
    }

}
