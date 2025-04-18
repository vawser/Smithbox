using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpShapeInstance : IHavokObject
    {
        public virtual uint Signature { get => 1581100549; }
        
        public enum Flags
        {
            HAS_TRANSLATION = 2,
            HAS_ROTATION = 4,
            HAS_SCALE = 8,
            FLIP_ORIENTATION = 16,
            SCALE_SURFACE = 32,
            IS_ENABLED = 64,
            DEFAULT_FLAGS = 64,
        }
        
        public Matrix4x4 m_transform;
        public Vector4 m_scale;
        public hknpShape m_shape;
        public ushort m_shapeTag;
        public ushort m_destructionTag;
        public byte m_padding_0;
        public byte m_padding_1;
        public byte m_padding_2;
        public byte m_padding_3;
        public byte m_padding_4;
        public byte m_padding_5;
        public byte m_padding_6;
        public byte m_padding_7;
        public byte m_padding_8;
        public byte m_padding_9;
        public byte m_padding_10;
        public byte m_padding_11;
        public byte m_padding_12;
        public byte m_padding_13;
        public byte m_padding_14;
        public byte m_padding_15;
        public byte m_padding_16;
        public byte m_padding_17;
        public byte m_padding_18;
        public byte m_padding_19;
        public byte m_padding_20;
        public byte m_padding_21;
        public byte m_padding_22;
        public byte m_padding_23;
        public byte m_padding_24;
        public byte m_padding_25;
        public byte m_padding_26;
        public byte m_padding_27;
        public byte m_padding_28;
        public byte m_padding_29;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_transform = des.ReadTransform(br);
            m_scale = des.ReadVector4(br);
            m_shape = des.ReadClassPointer<hknpShape>(br);
            m_shapeTag = br.ReadUInt16();
            m_destructionTag = br.ReadUInt16();
            m_padding_0 = br.ReadByte();
            m_padding_1 = br.ReadByte();
            m_padding_2 = br.ReadByte();
            m_padding_3 = br.ReadByte();
            m_padding_4 = br.ReadByte();
            m_padding_5 = br.ReadByte();
            m_padding_6 = br.ReadByte();
            m_padding_7 = br.ReadByte();
            m_padding_8 = br.ReadByte();
            m_padding_9 = br.ReadByte();
            m_padding_10 = br.ReadByte();
            m_padding_11 = br.ReadByte();
            m_padding_12 = br.ReadByte();
            m_padding_13 = br.ReadByte();
            m_padding_14 = br.ReadByte();
            m_padding_15 = br.ReadByte();
            m_padding_16 = br.ReadByte();
            m_padding_17 = br.ReadByte();
            m_padding_18 = br.ReadByte();
            m_padding_19 = br.ReadByte();
            m_padding_20 = br.ReadByte();
            m_padding_21 = br.ReadByte();
            m_padding_22 = br.ReadByte();
            m_padding_23 = br.ReadByte();
            m_padding_24 = br.ReadByte();
            m_padding_25 = br.ReadByte();
            m_padding_26 = br.ReadByte();
            m_padding_27 = br.ReadByte();
            m_padding_28 = br.ReadByte();
            m_padding_29 = br.ReadByte();
            br.ReadUInt32();
            br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteTransform(bw, m_transform);
            s.WriteVector4(bw, m_scale);
            s.WriteClassPointer<hknpShape>(bw, m_shape);
            bw.WriteUInt16(m_shapeTag);
            bw.WriteUInt16(m_destructionTag);
            bw.WriteByte(m_padding_0);
            bw.WriteByte(m_padding_1);
            bw.WriteByte(m_padding_2);
            bw.WriteByte(m_padding_3);
            bw.WriteByte(m_padding_4);
            bw.WriteByte(m_padding_5);
            bw.WriteByte(m_padding_6);
            bw.WriteByte(m_padding_7);
            bw.WriteByte(m_padding_8);
            bw.WriteByte(m_padding_9);
            bw.WriteByte(m_padding_10);
            bw.WriteByte(m_padding_11);
            bw.WriteByte(m_padding_12);
            bw.WriteByte(m_padding_13);
            bw.WriteByte(m_padding_14);
            bw.WriteByte(m_padding_15);
            bw.WriteByte(m_padding_16);
            bw.WriteByte(m_padding_17);
            bw.WriteByte(m_padding_18);
            bw.WriteByte(m_padding_19);
            bw.WriteByte(m_padding_20);
            bw.WriteByte(m_padding_21);
            bw.WriteByte(m_padding_22);
            bw.WriteByte(m_padding_23);
            bw.WriteByte(m_padding_24);
            bw.WriteByte(m_padding_25);
            bw.WriteByte(m_padding_26);
            bw.WriteByte(m_padding_27);
            bw.WriteByte(m_padding_28);
            bw.WriteByte(m_padding_29);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
        }
    }
}
