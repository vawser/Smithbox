// Automatically Generated

namespace HKLib.hk2018;

public class hkxBlendShapeCollectionChannel : hkReferencedObject
{
    public string? m_name;

    public List<hkxBlendShapeCollectionChannel.BlendShape> m_blendShapes = new();

    public hkxVertexBuffer m_vertData = new();


    public class BlendShape : IHavokObject
    {
        public string? m_name;

        public List<hkxBlendShapeCollectionChannel.Channel> m_channnels = new();

    }


    public class Channel : IHavokObject
    {
        public string? m_name;

        public List<hkxBlendShapeCollectionChannel.KeyFrame> m_keyFrames = new();

        public List<hkxBlendShapeCollectionChannel.FloatCurve> m_curves = new();

    }


    public class FloatCurve : IHavokObject
    {
        public string? m_name;

        public List<hkxBlendShapeCollectionChannel.FloatCurveKey> m_values = new();

        public long m_timeStart;

        public long m_timeEnd;

    }


    public class FloatCurveKey : IHavokObject
    {
        public string? m_name;

        public float m_value;

        public long m_time;

    }


    public class KeyFrame : IHavokObject
    {
        public string? m_name;

        public uint m_baseVertex;

        public uint m_vertexCount;

        public double m_timeWeight;

    }


}

