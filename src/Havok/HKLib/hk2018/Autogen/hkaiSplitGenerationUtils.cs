// Automatically Generated

namespace HKLib.hk2018;

public interface hkaiSplitGenerationUtils : IHavokObject
{

    public enum SplitMethod : int
    {
        SPLIT_UNIFORM = 0,
        SPLIT_ADAPTIVE = 1
    }

    public enum SplitAndGenerateOptions : int
    {
        SIMPLIFY_INDIVIDUALLY = 0,
        SIMPLIFY_INDIVIDUALLY_BORDER_PRESERVE = 1,
        SIMPLIFY_ALL_AT_ONCE = 2,
        SIMPLIFY_TWO_PASS = 3
    }

    public class Settings : IHavokObject
    {
        public hkaiSplitGenerationUtils.SplitAndGenerateOptions m_simplificationOptions;

        public hkaiSplitGenerationUtils.SplitMethod m_splitMethod;

        public bool m_generateClusterGraphs;

        public int m_desiredFacesPerCluster;

        public float m_borderPreserveShrinkSize;

        public float m_streamingEdgeMatchTolerance;

        public int m_numX;

        public int m_numY;

        public int m_maxSplits;

        public int m_desiredTrisPerChunk;

    }


}

