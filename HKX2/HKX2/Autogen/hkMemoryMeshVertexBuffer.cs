using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkMemoryMeshVertexBuffer : hkMeshVertexBuffer
    {
        public override uint Signature { get => 1366656759; }
        
        public hkVertexFormat m_format;
        public int m_elementOffsets_0;
        public int m_elementOffsets_1;
        public int m_elementOffsets_2;
        public int m_elementOffsets_3;
        public int m_elementOffsets_4;
        public int m_elementOffsets_5;
        public int m_elementOffsets_6;
        public int m_elementOffsets_7;
        public int m_elementOffsets_8;
        public int m_elementOffsets_9;
        public int m_elementOffsets_10;
        public int m_elementOffsets_11;
        public int m_elementOffsets_12;
        public int m_elementOffsets_13;
        public int m_elementOffsets_14;
        public int m_elementOffsets_15;
        public int m_elementOffsets_16;
        public int m_elementOffsets_17;
        public int m_elementOffsets_18;
        public int m_elementOffsets_19;
        public int m_elementOffsets_20;
        public int m_elementOffsets_21;
        public int m_elementOffsets_22;
        public int m_elementOffsets_23;
        public int m_elementOffsets_24;
        public int m_elementOffsets_25;
        public int m_elementOffsets_26;
        public int m_elementOffsets_27;
        public int m_elementOffsets_28;
        public int m_elementOffsets_29;
        public int m_elementOffsets_30;
        public int m_elementOffsets_31;
        public List<byte> m_memory;
        public int m_vertexStride;
        public bool m_locked;
        public int m_numVertices;
        public bool m_isBigEndian;
        public bool m_isSharable;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_format = new hkVertexFormat();
            m_format.Read(des, br);
            m_elementOffsets_0 = br.ReadInt32();
            m_elementOffsets_1 = br.ReadInt32();
            m_elementOffsets_2 = br.ReadInt32();
            m_elementOffsets_3 = br.ReadInt32();
            m_elementOffsets_4 = br.ReadInt32();
            m_elementOffsets_5 = br.ReadInt32();
            m_elementOffsets_6 = br.ReadInt32();
            m_elementOffsets_7 = br.ReadInt32();
            m_elementOffsets_8 = br.ReadInt32();
            m_elementOffsets_9 = br.ReadInt32();
            m_elementOffsets_10 = br.ReadInt32();
            m_elementOffsets_11 = br.ReadInt32();
            m_elementOffsets_12 = br.ReadInt32();
            m_elementOffsets_13 = br.ReadInt32();
            m_elementOffsets_14 = br.ReadInt32();
            m_elementOffsets_15 = br.ReadInt32();
            m_elementOffsets_16 = br.ReadInt32();
            m_elementOffsets_17 = br.ReadInt32();
            m_elementOffsets_18 = br.ReadInt32();
            m_elementOffsets_19 = br.ReadInt32();
            m_elementOffsets_20 = br.ReadInt32();
            m_elementOffsets_21 = br.ReadInt32();
            m_elementOffsets_22 = br.ReadInt32();
            m_elementOffsets_23 = br.ReadInt32();
            m_elementOffsets_24 = br.ReadInt32();
            m_elementOffsets_25 = br.ReadInt32();
            m_elementOffsets_26 = br.ReadInt32();
            m_elementOffsets_27 = br.ReadInt32();
            m_elementOffsets_28 = br.ReadInt32();
            m_elementOffsets_29 = br.ReadInt32();
            m_elementOffsets_30 = br.ReadInt32();
            m_elementOffsets_31 = br.ReadInt32();
            br.ReadUInt32();
            m_memory = des.ReadByteArray(br);
            m_vertexStride = br.ReadInt32();
            m_locked = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_numVertices = br.ReadInt32();
            m_isBigEndian = br.ReadBoolean();
            m_isSharable = br.ReadBoolean();
            br.ReadUInt16();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_format.Write(s, bw);
            bw.WriteInt32(m_elementOffsets_0);
            bw.WriteInt32(m_elementOffsets_1);
            bw.WriteInt32(m_elementOffsets_2);
            bw.WriteInt32(m_elementOffsets_3);
            bw.WriteInt32(m_elementOffsets_4);
            bw.WriteInt32(m_elementOffsets_5);
            bw.WriteInt32(m_elementOffsets_6);
            bw.WriteInt32(m_elementOffsets_7);
            bw.WriteInt32(m_elementOffsets_8);
            bw.WriteInt32(m_elementOffsets_9);
            bw.WriteInt32(m_elementOffsets_10);
            bw.WriteInt32(m_elementOffsets_11);
            bw.WriteInt32(m_elementOffsets_12);
            bw.WriteInt32(m_elementOffsets_13);
            bw.WriteInt32(m_elementOffsets_14);
            bw.WriteInt32(m_elementOffsets_15);
            bw.WriteInt32(m_elementOffsets_16);
            bw.WriteInt32(m_elementOffsets_17);
            bw.WriteInt32(m_elementOffsets_18);
            bw.WriteInt32(m_elementOffsets_19);
            bw.WriteInt32(m_elementOffsets_20);
            bw.WriteInt32(m_elementOffsets_21);
            bw.WriteInt32(m_elementOffsets_22);
            bw.WriteInt32(m_elementOffsets_23);
            bw.WriteInt32(m_elementOffsets_24);
            bw.WriteInt32(m_elementOffsets_25);
            bw.WriteInt32(m_elementOffsets_26);
            bw.WriteInt32(m_elementOffsets_27);
            bw.WriteInt32(m_elementOffsets_28);
            bw.WriteInt32(m_elementOffsets_29);
            bw.WriteInt32(m_elementOffsets_30);
            bw.WriteInt32(m_elementOffsets_31);
            bw.WriteUInt32(0);
            s.WriteByteArray(bw, m_memory);
            bw.WriteInt32(m_vertexStride);
            bw.WriteBoolean(m_locked);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteInt32(m_numVertices);
            bw.WriteBoolean(m_isBigEndian);
            bw.WriteBoolean(m_isSharable);
            bw.WriteUInt16(0);
        }
    }
}
