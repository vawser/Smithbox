using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkPackfileSectionHeader : IHavokObject
    {
        public virtual uint Signature { get => 3337569377; }
        
        public sbyte m_sectionTag_0;
        public sbyte m_sectionTag_1;
        public sbyte m_sectionTag_2;
        public sbyte m_sectionTag_3;
        public sbyte m_sectionTag_4;
        public sbyte m_sectionTag_5;
        public sbyte m_sectionTag_6;
        public sbyte m_sectionTag_7;
        public sbyte m_sectionTag_8;
        public sbyte m_sectionTag_9;
        public sbyte m_sectionTag_10;
        public sbyte m_sectionTag_11;
        public sbyte m_sectionTag_12;
        public sbyte m_sectionTag_13;
        public sbyte m_sectionTag_14;
        public sbyte m_sectionTag_15;
        public sbyte m_sectionTag_16;
        public sbyte m_sectionTag_17;
        public sbyte m_sectionTag_18;
        public sbyte m_nullByte;
        public int m_absoluteDataStart;
        public int m_localFixupsOffset;
        public int m_globalFixupsOffset;
        public int m_virtualFixupsOffset;
        public int m_exportsOffset;
        public int m_importsOffset;
        public int m_endOffset;
        public int m_pad_0;
        public int m_pad_1;
        public int m_pad_2;
        public int m_pad_3;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_sectionTag_0 = br.ReadSByte();
            m_sectionTag_1 = br.ReadSByte();
            m_sectionTag_2 = br.ReadSByte();
            m_sectionTag_3 = br.ReadSByte();
            m_sectionTag_4 = br.ReadSByte();
            m_sectionTag_5 = br.ReadSByte();
            m_sectionTag_6 = br.ReadSByte();
            m_sectionTag_7 = br.ReadSByte();
            m_sectionTag_8 = br.ReadSByte();
            m_sectionTag_9 = br.ReadSByte();
            m_sectionTag_10 = br.ReadSByte();
            m_sectionTag_11 = br.ReadSByte();
            m_sectionTag_12 = br.ReadSByte();
            m_sectionTag_13 = br.ReadSByte();
            m_sectionTag_14 = br.ReadSByte();
            m_sectionTag_15 = br.ReadSByte();
            m_sectionTag_16 = br.ReadSByte();
            m_sectionTag_17 = br.ReadSByte();
            m_sectionTag_18 = br.ReadSByte();
            m_nullByte = br.ReadSByte();
            m_absoluteDataStart = br.ReadInt32();
            m_localFixupsOffset = br.ReadInt32();
            m_globalFixupsOffset = br.ReadInt32();
            m_virtualFixupsOffset = br.ReadInt32();
            m_exportsOffset = br.ReadInt32();
            m_importsOffset = br.ReadInt32();
            m_endOffset = br.ReadInt32();
            m_pad_0 = br.ReadInt32();
            m_pad_1 = br.ReadInt32();
            m_pad_2 = br.ReadInt32();
            m_pad_3 = br.ReadInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSByte(m_sectionTag_0);
            bw.WriteSByte(m_sectionTag_1);
            bw.WriteSByte(m_sectionTag_2);
            bw.WriteSByte(m_sectionTag_3);
            bw.WriteSByte(m_sectionTag_4);
            bw.WriteSByte(m_sectionTag_5);
            bw.WriteSByte(m_sectionTag_6);
            bw.WriteSByte(m_sectionTag_7);
            bw.WriteSByte(m_sectionTag_8);
            bw.WriteSByte(m_sectionTag_9);
            bw.WriteSByte(m_sectionTag_10);
            bw.WriteSByte(m_sectionTag_11);
            bw.WriteSByte(m_sectionTag_12);
            bw.WriteSByte(m_sectionTag_13);
            bw.WriteSByte(m_sectionTag_14);
            bw.WriteSByte(m_sectionTag_15);
            bw.WriteSByte(m_sectionTag_16);
            bw.WriteSByte(m_sectionTag_17);
            bw.WriteSByte(m_sectionTag_18);
            bw.WriteSByte(m_nullByte);
            bw.WriteInt32(m_absoluteDataStart);
            bw.WriteInt32(m_localFixupsOffset);
            bw.WriteInt32(m_globalFixupsOffset);
            bw.WriteInt32(m_virtualFixupsOffset);
            bw.WriteInt32(m_exportsOffset);
            bw.WriteInt32(m_importsOffset);
            bw.WriteInt32(m_endOffset);
            bw.WriteInt32(m_pad_0);
            bw.WriteInt32(m_pad_1);
            bw.WriteInt32(m_pad_2);
            bw.WriteInt32(m_pad_3);
        }
    }
}
