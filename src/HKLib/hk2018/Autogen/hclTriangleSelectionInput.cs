// Automatically Generated

namespace HKLib.hk2018;

public class hclTriangleSelectionInput : IHavokObject
{
    public hclTriangleSelectionInput.TriangleSelectionType m_type;

    public string? m_channelName;


    public enum TriangleSelectionType : int
    {
        TRIANGLE_SELECTION_ALL = 0,
        TRIANGLE_SELECTION_NONE = 1,
        TRIANGLE_SELECTION_CHANNEL = 2,
        TRIANGLE_SELECTION_INVERSE_CHANNEL = 3
    }

}

