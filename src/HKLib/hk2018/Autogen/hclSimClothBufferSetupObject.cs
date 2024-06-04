// Automatically Generated

namespace HKLib.hk2018;

public class hclSimClothBufferSetupObject : hclBufferSetupObject
{
    public hclSimClothBufferSetupObject.Type m_type;

    public string? m_name;

    public hclSimClothSetupObject? m_simClothSetupObject;


    public enum Type : int
    {
        SIM_CLOTH_MESH_CURRENT_POSITIONS = 0,
        SIM_CLOTH_MESH_PREVIOUS_POSITIONS = 1,
        SIM_CLOTH_MESH_ORIGINAL_POSE = 2
    }

}

