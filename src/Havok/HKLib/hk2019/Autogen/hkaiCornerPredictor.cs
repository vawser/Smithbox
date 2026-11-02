// Automatically Generated

namespace HKLib.hk2019;

public class hkaiCornerPredictor : IHavokObject
{
    public Vector4 m_up = new();

    public Vector4 m_nextTravelVector = new();

    public Vector4 m_nextEnterTurnPoint = new();

    public Matrix4x4 m_nextTransform = new();

    public int m_nextEdgeIndex;

    public uint m_nextIsLeft;

    public List<hkaiCornerPredictor.UserEdgeTraversal> m_nextUserEdgeTraversals = new();

    public object? m_lastFaceData;

    public int m_nextEdgeToCross;

    public Matrix4x4 m_transformEdgesCrossed = new();

    public hkaiCornerPredictor.StepForwardResult m_prevResult;

    public hkaiEdgePath? m_edgePath;

    public hkaiStreamingCollection? m_streamingCollection;


    public enum StepForwardResult : int
    {
        NORMAL = 0,
        AT_GOAL = 1,
        PARTIAL_PREDICTION_ERROR = 2,
        FULL_PREDICTION_ERROR = 3,
        INIT_PARTIAL_ERROR = 4,
        INIT_FULL_ERROR = 5,
        NOT_INITED = 6
    }

    public class FaceTraversal : IHavokObject
    {
        public Vector4 m_entrancePointLocal = new();

        public float m_distance;

        public object? m_faceDataPtr;

    }

    public class UserEdgeTraversal : IHavokObject
    {
        public Vector4 m_entrancePointLocal = new();

        public object? m_edgeDataPtr;

    }

}
