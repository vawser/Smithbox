// Automatically Generated

namespace HKLib.hk2018;

public class hclBoneSpaceDeformer : IHavokObject
{
    public List<hclBoneSpaceDeformer.FourBlendEntryBlock> m_fourBlendEntries = new();

    public List<hclBoneSpaceDeformer.ThreeBlendEntryBlock> m_threeBlendEntries = new();

    public List<hclBoneSpaceDeformer.TwoBlendEntryBlock> m_twoBlendEntries = new();

    public List<hclBoneSpaceDeformer.OneBlendEntryBlock> m_oneBlendEntries = new();

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
        public readonly Vector4[] m_localPosition = new Vector4[16];

        public readonly hkPackedVector3[] m_localNormal = new hkPackedVector3[16];

        public readonly hkPackedVector3[] m_localTangent = new hkPackedVector3[16];

        public readonly hkPackedVector3[] m_localBiTangent = new hkPackedVector3[16];

    }


    public class LocalBlockPNT : IHavokObject
    {
        public readonly Vector4[] m_localPosition = new Vector4[16];

        public readonly hkPackedVector3[] m_localNormal = new hkPackedVector3[16];

        public readonly hkPackedVector3[] m_localTangent = new hkPackedVector3[16];

    }


    public class LocalBlockPN : IHavokObject
    {
        public readonly Vector4[] m_localPosition = new Vector4[16];

        public readonly hkPackedVector3[] m_localNormal = new hkPackedVector3[16];

    }


    public class LocalBlockP : IHavokObject
    {
        public readonly Vector4[] m_localPosition = new Vector4[16];

    }


    public class OneBlendEntryBlock : IHavokObject
    {
        public readonly ushort[] m_vertexIndices = new ushort[16];

        public readonly ushort[] m_boneIndices = new ushort[16];

    }


    public class TwoBlendEntryBlock : IHavokObject
    {
        public readonly ushort[] m_vertexIndices = new ushort[8];

        public readonly ushort[] m_boneIndices = new ushort[16];

    }


    public class ThreeBlendEntryBlock : IHavokObject
    {
        public readonly ushort[] m_vertexIndices = new ushort[5];

        public readonly ushort[] m_boneIndices = new ushort[15];

    }


    public class FourBlendEntryBlock : IHavokObject
    {
        public readonly ushort[] m_vertexIndices = new ushort[4];

        public readonly ushort[] m_boneIndices = new ushort[16];

    }


}

