using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclBoneSpaceDeformerThreeBlendEntryBlock : IHavokObject
    {
        public virtual uint Signature { get => 3411726818; }
        
        public ushort m_vertexIndices_0;
        public ushort m_vertexIndices_1;
        public ushort m_vertexIndices_2;
        public ushort m_vertexIndices_3;
        public ushort m_vertexIndices_4;
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
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_vertexIndices_0 = br.ReadUInt16();
            m_vertexIndices_1 = br.ReadUInt16();
            m_vertexIndices_2 = br.ReadUInt16();
            m_vertexIndices_3 = br.ReadUInt16();
            m_vertexIndices_4 = br.ReadUInt16();
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
            br.ReadUInt64();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt16(m_vertexIndices_0);
            bw.WriteUInt16(m_vertexIndices_1);
            bw.WriteUInt16(m_vertexIndices_2);
            bw.WriteUInt16(m_vertexIndices_3);
            bw.WriteUInt16(m_vertexIndices_4);
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
            bw.WriteUInt64(0);
        }
    }
}
