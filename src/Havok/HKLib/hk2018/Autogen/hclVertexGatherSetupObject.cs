// Automatically Generated

namespace HKLib.hk2018;

public class hclVertexGatherSetupObject : hclOperatorSetupObject
{
    public string? m_name;

    public hclVertexGatherSetupObject.Direction m_direction;

    public hclSimClothBufferSetupObject? m_simulationBuffer;

    public hclVertexSelectionInput m_simulationParticleSelection = new();

    public hclBufferSetupObject? m_displayBuffer;

    public hclVertexSelectionInput m_displayVertexSelection = new();

    public float m_gatherAllThreshold;

    public bool m_gatherNormals;


    public enum Direction : int
    {
        SIMULATION_TO_DISPLAY = 0,
        DISPLAY_TO_SIMULATION = 1
    }

}

