// Automatically Generated

namespace HKLib.hk2018;

public class hclVertexFloatInput : IHavokObject
{
    public hclVertexFloatInput.VertexFloatType m_type;

    public float m_constantValue;

    public string? m_channelName;

    public bool m_overrideScale;

    public float m_overrideScaleMin;

    public float m_overrideScaleMax;


    public enum VertexFloatType : int
    {
        VERTEX_FLOAT_CONSTANT = 0,
        VERTEX_FLOAT_CHANNEL = 1
    }

}

