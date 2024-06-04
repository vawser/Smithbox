// Automatically Generated

namespace HKLib.hk2018;

public class hclLocalRangeSetupObject : hclConstraintSetSetupObject
{
    public string? m_name;

    public hclSimulationSetupMesh? m_simulationMesh;

    public hclBufferSetupObject? m_referenceBufferSetup;

    public hclVertexSelectionInput m_vertexSelection = new();

    public hclVertexFloatInput m_maximumDistance = new();

    public hclVertexFloatInput m_minNormalDistance = new();

    public hclVertexFloatInput m_maxNormalDistance = new();

    public float m_stiffness;

    public hclLocalRangeConstraintSet.ShapeType m_localRangeShape;

    public bool m_useMinNormalDistance;

    public bool m_useMaxNormalDistance;

}

