// Automatically Generated

namespace HKLib.hk2018;

public class hclVertexSelectionInput : IHavokObject
{
    public hclVertexSelectionInput.VertexSelectionType m_type;

    public string? m_channelName;


    public enum VertexSelectionType : int
    {
        VERTEX_SELECTION_ALL = 0,
        VERTEX_SELECTION_NONE = 1,
        VERTEX_SELECTION_CHANNEL = 2,
        VERTEX_SELECTION_INVERSE_CHANNEL = 3
    }

}

