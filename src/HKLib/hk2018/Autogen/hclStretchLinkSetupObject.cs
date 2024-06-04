// Automatically Generated

namespace HKLib.hk2018;

public class hclStretchLinkSetupObject : hclConstraintSetSetupObject
{
    public string? m_name;

    public hclSimulationSetupMesh? m_simulationMesh;

    public hclVertexSelectionInput m_movableParticlesSelection = new();

    public hclVertexSelectionInput m_fixedParticlesSelection = new();

    public hclVertexFloatInput m_rigidFactor = new();

    public hclVertexFloatInput m_stiffness = new();

    public Vector4 m_stretchDirection = new();

    public bool m_useStretchDirection;

    public bool m_useMeshTopology;

    public bool m_allowDynamicLinks;

    public bool m_useTopologicalStretchDistance;

}

