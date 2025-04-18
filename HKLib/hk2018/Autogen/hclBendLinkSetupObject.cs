// Automatically Generated

namespace HKLib.hk2018;

public class hclBendLinkSetupObject : hclConstraintSetSetupObject
{
    public string? m_name;

    public hclSimulationSetupMesh? m_simulationMesh;

    public bool m_createStandardLinks;

    public hclVertexSelectionInput m_vertexSelection = new();

    public hclVertexFloatInput m_bendStiffness = new();

    public hclVertexFloatInput m_stretchStiffness = new();

    public hclVertexFloatInput m_flatnessFactor = new();

}

