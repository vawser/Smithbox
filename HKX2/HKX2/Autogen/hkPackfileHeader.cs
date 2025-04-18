using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkPackfileHeader : IHavokObject
    {
        public virtual uint Signature { get => 1446864575; }
        
        public int m_magic_0;
        public int m_magic_1;
        public int m_userTag;
        public int m_fileVersion;
        public byte m_layoutRules_0;
        public byte m_layoutRules_1;
        public byte m_layoutRules_2;
        public byte m_layoutRules_3;
        public int m_numSections;
        public int m_contentsSectionIndex;
        public int m_contentsSectionOffset;
        public int m_contentsClassNameSectionIndex;
        public int m_contentsClassNameSectionOffset;
        public sbyte m_contentsVersion_0;
        public sbyte m_contentsVersion_1;
        public sbyte m_contentsVersion_2;
        public sbyte m_contentsVersion_3;
        public sbyte m_contentsVersion_4;
        public sbyte m_contentsVersion_5;
        public sbyte m_contentsVersion_6;
        public sbyte m_contentsVersion_7;
        public sbyte m_contentsVersion_8;
        public sbyte m_contentsVersion_9;
        public sbyte m_contentsVersion_10;
        public sbyte m_contentsVersion_11;
        public sbyte m_contentsVersion_12;
        public sbyte m_contentsVersion_13;
        public sbyte m_contentsVersion_14;
        public sbyte m_contentsVersion_15;
        public int m_flags;
        public ushort m_maxpredicate;
        public ushort m_predicateArraySizePlusPadding;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_magic_0 = br.ReadInt32();
            m_magic_1 = br.ReadInt32();
            m_userTag = br.ReadInt32();
            m_fileVersion = br.ReadInt32();
            m_layoutRules_0 = br.ReadByte();
            m_layoutRules_1 = br.ReadByte();
            m_layoutRules_2 = br.ReadByte();
            m_layoutRules_3 = br.ReadByte();
            m_numSections = br.ReadInt32();
            m_contentsSectionIndex = br.ReadInt32();
            m_contentsSectionOffset = br.ReadInt32();
            m_contentsClassNameSectionIndex = br.ReadInt32();
            m_contentsClassNameSectionOffset = br.ReadInt32();
            m_contentsVersion_0 = br.ReadSByte();
            m_contentsVersion_1 = br.ReadSByte();
            m_contentsVersion_2 = br.ReadSByte();
            m_contentsVersion_3 = br.ReadSByte();
            m_contentsVersion_4 = br.ReadSByte();
            m_contentsVersion_5 = br.ReadSByte();
            m_contentsVersion_6 = br.ReadSByte();
            m_contentsVersion_7 = br.ReadSByte();
            m_contentsVersion_8 = br.ReadSByte();
            m_contentsVersion_9 = br.ReadSByte();
            m_contentsVersion_10 = br.ReadSByte();
            m_contentsVersion_11 = br.ReadSByte();
            m_contentsVersion_12 = br.ReadSByte();
            m_contentsVersion_13 = br.ReadSByte();
            m_contentsVersion_14 = br.ReadSByte();
            m_contentsVersion_15 = br.ReadSByte();
            m_flags = br.ReadInt32();
            m_maxpredicate = br.ReadUInt16();
            m_predicateArraySizePlusPadding = br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt32(m_magic_0);
            bw.WriteInt32(m_magic_1);
            bw.WriteInt32(m_userTag);
            bw.WriteInt32(m_fileVersion);
            bw.WriteByte(m_layoutRules_0);
            bw.WriteByte(m_layoutRules_1);
            bw.WriteByte(m_layoutRules_2);
            bw.WriteByte(m_layoutRules_3);
            bw.WriteInt32(m_numSections);
            bw.WriteInt32(m_contentsSectionIndex);
            bw.WriteInt32(m_contentsSectionOffset);
            bw.WriteInt32(m_contentsClassNameSectionIndex);
            bw.WriteInt32(m_contentsClassNameSectionOffset);
            bw.WriteSByte(m_contentsVersion_0);
            bw.WriteSByte(m_contentsVersion_1);
            bw.WriteSByte(m_contentsVersion_2);
            bw.WriteSByte(m_contentsVersion_3);
            bw.WriteSByte(m_contentsVersion_4);
            bw.WriteSByte(m_contentsVersion_5);
            bw.WriteSByte(m_contentsVersion_6);
            bw.WriteSByte(m_contentsVersion_7);
            bw.WriteSByte(m_contentsVersion_8);
            bw.WriteSByte(m_contentsVersion_9);
            bw.WriteSByte(m_contentsVersion_10);
            bw.WriteSByte(m_contentsVersion_11);
            bw.WriteSByte(m_contentsVersion_12);
            bw.WriteSByte(m_contentsVersion_13);
            bw.WriteSByte(m_contentsVersion_14);
            bw.WriteSByte(m_contentsVersion_15);
            bw.WriteInt32(m_flags);
            bw.WriteUInt16(m_maxpredicate);
            bw.WriteUInt16(m_predicateArraySizePlusPadding);
        }
    }
}
