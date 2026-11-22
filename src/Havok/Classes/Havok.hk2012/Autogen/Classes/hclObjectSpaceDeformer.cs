// Automatically Generated

namespace Havok.hk2012;

public class hclObjectSpaceDeformer : IHavokObject
{
    public List<hclObjectSpaceDeformer.FourBlendEntryBlock> m_fourBlendEntries = new();

    public List<hclObjectSpaceDeformer.ThreeBlendEntryBlock> m_threeBlendEntries = new();

    public List<hclObjectSpaceDeformer.TwoBlendEntryBlock> m_twoBlendEntries = new();

    public List<hclObjectSpaceDeformer.OneBlendEntryBlock> m_oneBlendEntries = new();

    public List<byte> m_controlBytes = new();

    public ushort m_startVertexIndex;

    public ushort m_endVertexIndex;

    public ushort m_batchSizeSpu;

    public bool m_partialWrite;


    public class FourBlendEntryBlock : IHavokObject
    {
        public readonly ushort[] m_vertexIndices = new ushort[16];

        public readonly ushort[] m_boneIndices = new ushort[64];

        public readonly byte[] m_boneWeights = new byte[64];

    }

    public class ThreeBlendEntryBlock : IHavokObject
    {
        public readonly ushort[] m_vertexIndices = new ushort[16];

        public readonly ushort[] m_boneIndices = new ushort[48];

        public readonly byte[] m_boneWeights = new byte[48];

    }

    public class TwoBlendEntryBlock : IHavokObject
    {
        public readonly ushort[] m_vertexIndices = new ushort[16];

        public readonly ushort[] m_boneIndices = new ushort[32];

        public readonly byte[] m_boneWeights = new byte[32];

    }

    public class OneBlendEntryBlock : IHavokObject
    {
        public readonly ushort[] m_vertexIndices = new ushort[16];

        public readonly ushort[] m_boneIndices = new ushort[16];

    }

    public class LocalBlockP : IHavokObject
    {
        public readonly short[] m_localPosition = new short[64];

    }

    public class LocalBlockPN : IHavokObject
    {
        public readonly short[] m_localPosition = new short[64];

        public readonly short[] m_localNormal = new short[64];

    }

    public class LocalBlockPNTB : IHavokObject
    {
        public readonly short[] m_localPosition = new short[64];

        public readonly short[] m_localNormal = new short[64];

        public readonly short[] m_localTangent = new short[64];

        public readonly short[] m_localBiTangent = new short[64];

    }

}
