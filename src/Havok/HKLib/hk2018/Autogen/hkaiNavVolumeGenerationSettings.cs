// Automatically Generated

namespace HKLib.hk2018;

public class hkaiNavVolumeGenerationSettings : hkReferencedObject
{
    public hkAabb m_volumeAabb = new();

    public float m_maxHorizontalRange;

    public float m_maxVerticalRange;

    public Vector4 m_up = new();

    public float m_characterHeight;

    public float m_characterDepth;

    public float m_characterWidth;

    public float m_cellWidth;

    public hkaiNavVolumeGenerationSettings.CellWidthToResolutionRounding m_resolutionRoundingMode;

    public hkaiNavVolumeGenerationSettings.ChunkSettings m_chunkSettings = new();

    public float m_border;

    public bool m_useBorderCells;

    public hkaiNavVolumeGenerationSettings.MergingSettings m_mergingSettings = new();

    public float m_minRegionVolume;

    public float m_minDistanceToSeedPoints;

    public List<Vector4> m_regionSeedPoints = new();

    public hkaiNavVolumeGenerationSettings.MaterialConstructionInfo m_defaultConstructionInfo = new();

    public List<hkaiNavVolumeGenerationSettings.MaterialConstructionInfo> m_materialMap = new();

    public List<hkaiCarver?> m_carvers = new();

    public List<hkaiMaterialPainter?> m_painters = new();

    public bool m_saveInputSnapshot;

    public string? m_snapshotFilename;


    public enum CellWidthToResolutionRounding : int
    {
        ROUND_TO_ZERO = 0,
        ROUND_TO_NEAREST = 1
    }

    [Flags]
    public enum MaterialFlagsBits : int
    {
        MATERIAL_NONE = 0,
        MATERIAL_BLOCKING = 1,
        MATERIAL_DEFAULT = 1
    }

    public class MergingSettings : IHavokObject
    {
        public float m_nodeWeight;

        public float m_edgeWeight;

        public bool m_estimateNewEdges;

        public int m_iterationsStabilizationThreshold;

        public int m_maxMergingIterations;

        public int m_randomSeed;

        public float m_multiplier;

        public bool m_useSimpleFirstMergePass;

    }


    public class ChunkSettings : IHavokObject
    {
        public ushort m_maxChunkSizeX;

        public ushort m_maxChunkSizeY;

        public ushort m_maxChunkSizeZ;

        public bool m_doGreedyMergeAfterCombine;

    }


    public class MaterialConstructionInfo : IHavokObject
    {
        public int m_materialIndex;

        public hkaiNavVolumeGenerationSettings.MaterialFlagsBits m_flags;

        public int m_resolution;

    }


}

