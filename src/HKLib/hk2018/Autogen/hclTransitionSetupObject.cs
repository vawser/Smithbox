// Automatically Generated

namespace HKLib.hk2018;

public class hclTransitionSetupObject : hclConstraintSetSetupObject
{
    public string? m_name;

    public hclSimulationSetupMesh? m_simulationMesh;

    public hclVertexSelectionInput m_vertexSelection = new();

    public hclVertexFloatInput m_toAnimDelay = new();

    public hclVertexFloatInput m_toSimDelay = new();

    public hclVertexFloatInput m_toSimMaxDistance = new();

    public float m_toAnimPeriod;

    public float m_toSimPeriod;

    public hclBufferSetupObject? m_referenceBufferSetup;

}

