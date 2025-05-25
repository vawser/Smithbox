// Automatically Generated

namespace HKLib.hk2018;

public class hkaiNavVolumePathSearchParameters : IHavokObject
{
    public Vector4 m_up = new();

    public hkaiNavVolumePathSearchParameters.LineOfSightFlags m_lineOfSightFlags;

    public float m_heuristicWeight;

    public float m_maximumPathLength;

    public hkaiSearchParameters.BufferSizes m_bufferSizes = new();


    [Flags]
    public enum LineOfSightFlags : int
    {
        NO_LINE_OF_SIGHT_CHECK = 0,
        EARLY_OUT_IF_NO_COST_MODIFIER = 1,
        EARLY_OUT_ALWAYS = 4
    }

}

