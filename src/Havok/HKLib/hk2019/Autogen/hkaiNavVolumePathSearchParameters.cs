// Automatically Generated

namespace HKLib.hk2019;

public class hkaiNavVolumePathSearchParameters : IHavokObject
{
    public Vector4 m_up = new();

    public hkaiNavVolumePathSearchParameters.LineOfSightType m_lineOfSightType;

    public float m_heuristicWeight;

    public float m_maximumPathLength;

    public hkaiSearchParameters.BufferSizes m_bufferSizes = new();


    public enum LineOfSightType : int
    {
        NO_LINE_OF_SIGHT_CHECK = 0,
        EARLY_OUT_IF_NO_COST_MODIFIER = 1,
        EARLY_OUT_ALWAYS = 2
    }

}
