// Automatically Generated

#nullable enable

namespace Havok.hk2018.Accessors;

internal sealed class hkaiCornerPredictorData : Havok.Formats.HavokFieldAccess<Havok.hk2018.hkaiCornerPredictor>
{
    public hkaiCornerPredictorData(Havok.hk2018.hkaiCornerPredictor instance) : base(instance) { }

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
            case "m_nextTravelVector":
            case "nextTravelVector":
            {
                value = instance.m_nextTravelVector;
                return true;
            }
            case "m_nextEnterTurnPoint":
            case "nextEnterTurnPoint":
            {
                value = instance.m_nextEnterTurnPoint;
                return true;
            }
            case "m_nextTransform":
            case "nextTransform":
            {
                value = instance.m_nextTransform;
                return true;
            }
            case "m_nextEdgeIndex":
            case "nextEdgeIndex":
            {
                value = instance.m_nextEdgeIndex;
                return true;
            }
            case "m_nextIsLeft":
            case "nextIsLeft":
            {
                value = instance.m_nextIsLeft;
                return true;
            }
            case "m_nextUserEdgeTraversals":
            case "nextUserEdgeTraversals":
            {
                value = instance.m_nextUserEdgeTraversals;
                return true;
            }
            case "m_prevResult":
            case "prevResult":
            {
                value = instance.m_prevResult;
                return true;
            }
            case "m_edgePath":
            case "edgePath":
            {
                value = instance.m_edgePath;
                return true;
            }
            case "m_streamingCollection":
            case "streamingCollection":
            {
                value = instance.m_streamingCollection;
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
                if (value is not List<Havok.hk2018.hkaiCornerPredictor.UserEdgeTraversal> castValue) return false;
                instance.m_nextUserEdgeTraversals = castValue;
                return true;
            }
            case "m_prevResult":
            case "prevResult":
            {
                if (value is Havok.hk2018.hkaiCornerPredictor.StepForwardResult castValue)
                {
                    instance.m_prevResult = castValue;
                    return true;
                }
                if (value is int intValue)
                {
                    instance.m_prevResult = (Havok.hk2018.hkaiCornerPredictor.StepForwardResult)intValue;
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
                if (value is Havok.hk2018.hkaiEdgePath castValue)
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
                if (value is Havok.hk2018.hkaiStreamingCollection castValue)
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
