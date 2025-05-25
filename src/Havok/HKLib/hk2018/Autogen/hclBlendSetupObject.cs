// Automatically Generated

namespace HKLib.hk2018;

public class hclBlendSetupObject : hclOperatorSetupObject
{
    public string? m_name;

    public hclBufferSetupObject? m_A;

    public hclBufferSetupObject? m_B;

    public hclBufferSetupObject? m_C;

    public hclVertexSelectionInput m_vertexSelection = new();

    public bool m_blendNormals;

    public bool m_blendTangents;

    public bool m_blendBitangents;

    public bool m_dynamicBlend;

    public hclVertexFloatInput m_blendWeights = new();

    public float m_dynamicBlendDefaultWeight;

    public float m_dynamicBlendTransitionPeriod;

    public bool m_mapToScurve;

}

