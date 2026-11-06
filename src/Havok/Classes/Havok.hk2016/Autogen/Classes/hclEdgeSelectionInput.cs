// Automatically Generated

namespace Havok.hk2016;

public class hclEdgeSelectionInput : IHavokObject
{
    public hclEdgeSelectionInput.EdgeSelectionType m_type;

    public string? m_channelName;


    public enum EdgeSelectionType : int
    {
        EDGE_SELECTION_ALL = 0,
        EDGE_SELECTION_NONE = 1,
        EDGE_SELECTION_CHANNEL = 2,
        EDGE_SELECTION_INVERSE_CHANNEL = 3
    }

}
