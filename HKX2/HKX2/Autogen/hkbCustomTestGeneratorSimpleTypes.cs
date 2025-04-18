using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbCustomTestGeneratorSimpleTypes : hkbCustomTestGeneratorHiddenTypes
    {
        public override uint Signature { get => 2066222481; }
        
        public long m_simpleTypeHkInt64;
        public ulong m_simpleTypeHkUint64;
        public bool m_simpleHiddenTypeCopyStart;
        public bool m_simpleTypeBool;
        public bool m_simpleTypeHkBool;
        public string m_simpleTypeCString;
        public string m_simpleTypeHkStringPtr;
        public sbyte m_simpleTypeHkInt8;
        public short m_simpleTypeHkInt16;
        public int m_simpleTypeHkInt32;
        public byte m_simpleTypeHkUint8;
        public ushort m_simpleTypeHkUint16;
        public uint m_simpleTypeHkUint32;
        public float m_simpleTypeHkReal;
        public sbyte m_simpleTypeHkInt8Default;
        public short m_simpleTypeHkInt16Default;
        public int m_simpleTypeHkInt32Default;
        public byte m_simpleTypeHkUint8Default;
        public ushort m_simpleTypeHkUint16Default;
        public uint m_simpleTypeHkUint32Default;
        public float m_simpleTypeHkRealDefault;
        public sbyte m_simpleTypeHkInt8Clamp;
        public short m_simpleTypeHkInt16Clamp;
        public int m_simpleTypeHkInt32Clamp;
        public byte m_simpleTypeHkUint8Clamp;
        public ushort m_simpleTypeHkUint16Clamp;
        public uint m_simpleTypeHkUint32Clamp;
        public float m_simpleTypeHkRealClamp;
        public bool m_simpleHiddenTypeCopyEnd;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_simpleTypeHkInt64 = br.ReadInt64();
            m_simpleTypeHkUint64 = br.ReadUInt64();
            m_simpleHiddenTypeCopyStart = br.ReadBoolean();
            m_simpleTypeBool = br.ReadBoolean();
            m_simpleTypeHkBool = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadByte();
            m_simpleTypeCString = des.ReadStringPointer(br);
            m_simpleTypeHkStringPtr = des.ReadStringPointer(br);
            m_simpleTypeHkInt8 = br.ReadSByte();
            br.ReadByte();
            m_simpleTypeHkInt16 = br.ReadInt16();
            m_simpleTypeHkInt32 = br.ReadInt32();
            m_simpleTypeHkUint8 = br.ReadByte();
            br.ReadByte();
            m_simpleTypeHkUint16 = br.ReadUInt16();
            m_simpleTypeHkUint32 = br.ReadUInt32();
            m_simpleTypeHkReal = br.ReadSingle();
            m_simpleTypeHkInt8Default = br.ReadSByte();
            br.ReadByte();
            m_simpleTypeHkInt16Default = br.ReadInt16();
            m_simpleTypeHkInt32Default = br.ReadInt32();
            m_simpleTypeHkUint8Default = br.ReadByte();
            br.ReadByte();
            m_simpleTypeHkUint16Default = br.ReadUInt16();
            m_simpleTypeHkUint32Default = br.ReadUInt32();
            m_simpleTypeHkRealDefault = br.ReadSingle();
            m_simpleTypeHkInt8Clamp = br.ReadSByte();
            br.ReadByte();
            m_simpleTypeHkInt16Clamp = br.ReadInt16();
            m_simpleTypeHkInt32Clamp = br.ReadInt32();
            m_simpleTypeHkUint8Clamp = br.ReadByte();
            br.ReadByte();
            m_simpleTypeHkUint16Clamp = br.ReadUInt16();
            m_simpleTypeHkUint32Clamp = br.ReadUInt32();
            m_simpleTypeHkRealClamp = br.ReadSingle();
            m_simpleHiddenTypeCopyEnd = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteInt64(m_simpleTypeHkInt64);
            bw.WriteUInt64(m_simpleTypeHkUint64);
            bw.WriteBoolean(m_simpleHiddenTypeCopyStart);
            bw.WriteBoolean(m_simpleTypeBool);
            bw.WriteBoolean(m_simpleTypeHkBool);
            bw.WriteUInt32(0);
            bw.WriteByte(0);
            s.WriteStringPointer(bw, m_simpleTypeCString);
            s.WriteStringPointer(bw, m_simpleTypeHkStringPtr);
            bw.WriteSByte(m_simpleTypeHkInt8);
            bw.WriteByte(0);
            bw.WriteInt16(m_simpleTypeHkInt16);
            bw.WriteInt32(m_simpleTypeHkInt32);
            bw.WriteByte(m_simpleTypeHkUint8);
            bw.WriteByte(0);
            bw.WriteUInt16(m_simpleTypeHkUint16);
            bw.WriteUInt32(m_simpleTypeHkUint32);
            bw.WriteSingle(m_simpleTypeHkReal);
            bw.WriteSByte(m_simpleTypeHkInt8Default);
            bw.WriteByte(0);
            bw.WriteInt16(m_simpleTypeHkInt16Default);
            bw.WriteInt32(m_simpleTypeHkInt32Default);
            bw.WriteByte(m_simpleTypeHkUint8Default);
            bw.WriteByte(0);
            bw.WriteUInt16(m_simpleTypeHkUint16Default);
            bw.WriteUInt32(m_simpleTypeHkUint32Default);
            bw.WriteSingle(m_simpleTypeHkRealDefault);
            bw.WriteSByte(m_simpleTypeHkInt8Clamp);
            bw.WriteByte(0);
            bw.WriteInt16(m_simpleTypeHkInt16Clamp);
            bw.WriteInt32(m_simpleTypeHkInt32Clamp);
            bw.WriteByte(m_simpleTypeHkUint8Clamp);
            bw.WriteByte(0);
            bw.WriteUInt16(m_simpleTypeHkUint16Clamp);
            bw.WriteUInt32(m_simpleTypeHkUint32Clamp);
            bw.WriteSingle(m_simpleTypeHkRealClamp);
            bw.WriteBoolean(m_simpleHiddenTypeCopyEnd);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
