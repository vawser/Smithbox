using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclBoneSpaceDeformerFourBlendEntryBlock : IHavokObject
    {
        public virtual uint Signature { get => 487655624; }
        
        public ushort m_vertexIndices_0;
        public ushort m_vertexIndices_1;
        public ushort m_vertexIndices_2;
        public ushort m_vertexIndices_3;
        public ushort m_boneIndices_0;
        public ushort m_boneIndices_1;
        public ushort m_boneIndices_2;
        public ushort m_boneIndices_3;
        public ushort m_boneIndices_4;
        public ushort m_boneIndices_5;
        public ushort m_boneIndices_6;
        public ushort m_boneIndices_7;
        public ushort m_boneIndices_8;
        public ushort m_boneIndices_9;
        public ushort m_boneIndices_10;
        public ushort m_boneIndices_11;
        public ushort m_boneIndices_12;
        public ushort m_boneIndices_13;
        public ushort m_boneIndices_14;
        public ushort m_boneIndices_15;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_vertexIndices_0 = br.ReadUInt16();
            m_vertexIndices_1 = br.ReadUInt16();
            m_vertexIndices_2 = br.ReadUInt16();
            m_vertexIndices_3 = br.ReadUInt16();
            m_boneIndices_0 = br.ReadUInt16();
            m_boneIndices_1 = br.ReadUInt16();
            m_boneIndices_2 = br.ReadUInt16();
            m_boneIndices_3 = br.ReadUInt16();
            m_boneIndices_4 = br.ReadUInt16();
            m_boneIndices_5 = br.ReadUInt16();
            m_boneIndices_6 = br.ReadUInt16();
            m_boneIndices_7 = br.ReadUInt16();
            m_boneIndices_8 = br.ReadUInt16();
            m_boneIndices_9 = br.ReadUInt16();
            m_boneIndices_10 = br.ReadUInt16();
            m_boneIndices_11 = br.ReadUInt16();
            m_boneIndices_12 = br.ReadUInt16();
            m_boneIndices_13 = br.ReadUInt16();
            m_boneIndices_14 = br.ReadUInt16();
            m_boneIndices_15 = br.ReadUInt16();
            br.ReadUInt64();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt16(m_vertexIndices_0);
            bw.WriteUInt16(m_vertexIndices_1);
            bw.WriteUInt16(m_vertexIndices_2);
            bw.WriteUInt16(m_vertexIndices_3);
            bw.WriteUInt16(m_boneIndices_0);
            bw.WriteUInt16(m_boneIndices_1);
            bw.WriteUInt16(m_boneIndices_2);
            bw.WriteUInt16(m_boneIndices_3);
            bw.WriteUInt16(m_boneIndices_4);
            bw.WriteUInt16(m_boneIndices_5);
            bw.WriteUInt16(m_boneIndices_6);
            bw.WriteUInt16(m_boneIndices_7);
            bw.WriteUInt16(m_boneIndices_8);
            bw.WriteUInt16(m_boneIndices_9);
            bw.WriteUInt16(m_boneIndices_10);
            bw.WriteUInt16(m_boneIndices_11);
            bw.WriteUInt16(m_boneIndices_12);
            bw.WriteUInt16(m_boneIndices_13);
            bw.WriteUInt16(m_boneIndices_14);
            bw.WriteUInt16(m_boneIndices_15);
            bw.WriteUInt64(0);
        }
    }
}
