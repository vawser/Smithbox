// Automatically Generated

namespace HKLib.hk2018;

public class hclObjectSpaceDeformer : IHavokObject
{
    public List<hclObjectSpaceDeformer.EightBlendEntryBlock> m_eightBlendEntries = new();

    public List<hclObjectSpaceDeformer.SevenBlendEntryBlock> m_sevenBlendEntries = new();

    public List<hclObjectSpaceDeformer.SixBlendEntryBlock> m_sixBlendEntries = new();

    public List<hclObjectSpaceDeformer.FiveBlendEntryBlock> m_fiveBlendEntries = new();

    public List<hclObjectSpaceDeformer.FourBlendEntryBlock> m_fourBlendEntries = new();

    public List<hclObjectSpaceDeformer.ThreeBlendEntryBlock> m_threeBlendEntries = new();

    public List<hclObjectSpaceDeformer.TwoBlendEntryBlock> m_twoBlendEntries = new();

    public List<hclObjectSpaceDeformer.OneBlendEntryBlock> m_oneBlendEntries = new();

    public List<byte> m_controlBytes = new();

    public ushort m_startVertexIndex;

    public ushort m_endVertexIndex;

    public bool m_partialWrite;


    public class LocalBlockUnpackedPNTB : IHavokObject
    {
        public readonly Vector4[] m_localPosition = new Vector4[16];

        public readonly Vector4[] m_localNormal = new Vector4[16];

        public readonly Vector4[] m_localTangent = new Vector4[16];

        public readonly Vector4[] m_localBiTangent = new Vector4[16];

    }


    public class LocalBlockUnpackedPNT : IHavokObject
    {
        public readonly Vector4[] m_localPosition = new Vector4[16];

        public readonly Vector4[] m_localNormal = new Vector4[16];

        public readonly Vector4[] m_localTangent = new Vector4[16];

    }


    public class LocalBlockUnpackedPN : IHavokObject
    {
        public readonly Vector4[] m_localPosition = new Vector4[16];

        public readonly Vector4[] m_localNormal = new Vector4[16];

    }


    public class LocalBlockUnpackedP : IHavokObject
    {
        public readonly Vector4[] m_localPosition = new Vector4[16];

    }


    public class LocalBlockPNTB : IHavokObject
    {
        public readonly hkPackedVector3[] m_localPosition = new hkPackedVector3[16];

        public readonly hkPackedVector3[] m_localNormal = new hkPackedVector3[16];

        public readonly hkPackedVector3[] m_localTangent = new hkPackedVector3[16];

        public readonly hkPackedVector3[] m_localBiTangent = new hkPackedVector3[16];

    }


    public class LocalBlockPNT : IHavokObject
    {
        public readonly hkPackedVector3[] m_localPosition = new hkPackedVector3[16];

        public readonly hkPackedVector3[] m_localNormal = new hkPackedVector3[16];

        public readonly hkPackedVector3[] m_localTangent = new hkPackedVector3[16];

    }


    public class LocalBlockPN : IHavokObject
    {
        public readonly hkPackedVector3[] m_localPosition = new hkPackedVector3[16];

        public readonly hkPackedVector3[] m_localNormal = new hkPackedVector3[16];

    }


    public class LocalBlockP : IHavokObject
    {
        public readonly hkPackedVector3[] m_localPosition = new hkPackedVector3[16];

    }


    public class OneBlendEntryBlock : IHavokObject
    {
        public readonly ushort[] m_vertexIndices = new ushort[16];

        public readonly ushort[] m_boneIndices = new ushort[16];

    }


    public class TwoBlendEntryBlock : IHavokObject
    {
        public readonly ushort[] m_vertexIndices = new ushort[16];

        public readonly ushort[] m_boneIndices = new ushort[32];

        public readonly byte[] m_boneWeights = new byte[32];

    }


    public class ThreeBlendEntryBlock : IHavokObject
    {
        public readonly ushort[] m_vertexIndices = new ushort[16];

        public readonly ushort[] m_boneIndices = new ushort[48];

        public readonly byte[] m_boneWeights = new byte[48];

    }


    public class FourBlendEntryBlock : IHavokObject
    {
        public readonly ushort[] m_vertexIndices = new ushort[16];

        public readonly ushort[] m_boneIndices = new ushort[64];

        public readonly byte[] m_boneWeights = new byte[64];

    }


    public class FiveBlendEntryBlock : IHavokObject
    {
        public readonly ushort[] m_vertexIndices = new ushort[16];

        public readonly ushort[] m_boneIndices = new ushort[80];

        public readonly ushort[] m_boneWeights = new ushort[80];

    }


    public class SixBlendEntryBlock : IHavokObject
    {
        public readonly ushort[] m_vertexIndices = new ushort[16];

        public readonly ushort[] m_boneIndices = new ushort[96];

        public readonly ushort[] m_boneWeights = new ushort[96];

    }


    public class SevenBlendEntryBlock : IHavokObject
    {
        public readonly ushort[] m_vertexIndices = new ushort[16];

        public readonly ushort[] m_boneIndices = new ushort[112];

        public readonly ushort[] m_boneWeights = new ushort[112];

    }


    public class EightBlendEntryBlock : IHavokObject
    {
        public readonly ushort[] m_vertexIndices = new ushort[16];

        public readonly ushort[] m_boneIndices = new ushort[128];

        public readonly ushort[] m_boneWeights = new ushort[128];

    }


}

