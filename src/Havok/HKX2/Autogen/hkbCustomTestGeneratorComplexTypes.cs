using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum CustomEnum
    {
        CUSTOM_ENUM_ALA = 0,
        CUSTOM_ENUM_DEPECHE = 1,
        CUSTOM_ENUM_FURIOUS = 5,
    }
    
    public enum CustomFlag
    {
        CUSTOM_FLAG_NONE = 0,
        CUSTOM_FLAG_UNO = 1,
        CUSTOM_FLAG_ZWEI = 2,
        CUSTOM_FLAG_SHI_OR_YON = 4,
        CUSTOM_FLAG_LOTS_O_BITS = 240,
    }
    
    public partial class hkbCustomTestGeneratorComplexTypes : hkbCustomTestGeneratorSimpleTypes
    {
        public override uint Signature { get => 1956015663; }
        
        public hkReferencedObject m_complexTypeHkObjectPtr;
        public bool m_complexHiddenTypeCopyStart;
        public Quaternion m_complexTypeHkQuaternion;
        public Vector4 m_complexTypeHkVector4;
        public CustomEnum m_complexTypeEnumHkInt8;
        public CustomEnum m_complexTypeEnumHkInt16;
        public CustomEnum m_complexTypeEnumHkInt32;
        public CustomEnum m_complexTypeEnumHkUint8;
        public CustomEnum m_complexTypeEnumHkUint16;
        public CustomEnum m_complexTypeEnumHkUint32;
        public CustomEnum m_complexTypeEnumHkInt8InvalidCheck;
        public CustomEnum m_complexTypeEnumHkInt16InvalidCheck;
        public CustomEnum m_complexTypeEnumHkInt32InvalidCheck;
        public CustomEnum m_complexTypeEnumHkUint8InvalidCheck;
        public CustomEnum m_complexTypeEnumHkUint16InvalidCheck;
        public CustomEnum m_complexTypeEnumHkUint32InvalidCheck;
        public sbyte m_complexTypeFlagsHkInt8;
        public short m_complexTypeFlagsHkInt16;
        public int m_complexTypeFlagsHkInt32;
        public byte m_complexTypeFlagsHkUint8;
        public ushort m_complexTypeFlagsHkUint16;
        public uint m_complexTypeFlagsHkUint32;
        public sbyte m_complexTypeFlagsHkInt8InvalidCheck;
        public short m_complexTypeFlagsHkInt16InvalidCheck;
        public int m_complexTypeFlagsHkInt32InvalidCheck;
        public byte m_complexTypeFlagsHkUint8InvalidCheck;
        public ushort m_complexTypeFlagsHkUint16InvalidCheck;
        public uint m_complexTypeFlagsHkUint32InvalidCheck;
        public bool m_complexHiddenTypeCopyEnd;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_complexTypeHkObjectPtr = des.ReadClassPointer<hkReferencedObject>(br);
            m_complexHiddenTypeCopyStart = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
            m_complexTypeHkQuaternion = des.ReadQuaternion(br);
            m_complexTypeHkVector4 = des.ReadVector4(br);
            m_complexTypeEnumHkInt8 = (CustomEnum)br.ReadSByte();
            br.ReadByte();
            m_complexTypeEnumHkInt16 = (CustomEnum)br.ReadInt16();
            m_complexTypeEnumHkInt32 = (CustomEnum)br.ReadInt32();
            m_complexTypeEnumHkUint8 = (CustomEnum)br.ReadByte();
            br.ReadByte();
            m_complexTypeEnumHkUint16 = (CustomEnum)br.ReadUInt16();
            m_complexTypeEnumHkUint32 = (CustomEnum)br.ReadUInt32();
            m_complexTypeEnumHkInt8InvalidCheck = (CustomEnum)br.ReadSByte();
            br.ReadByte();
            m_complexTypeEnumHkInt16InvalidCheck = (CustomEnum)br.ReadInt16();
            m_complexTypeEnumHkInt32InvalidCheck = (CustomEnum)br.ReadInt32();
            m_complexTypeEnumHkUint8InvalidCheck = (CustomEnum)br.ReadByte();
            br.ReadByte();
            m_complexTypeEnumHkUint16InvalidCheck = (CustomEnum)br.ReadUInt16();
            m_complexTypeEnumHkUint32InvalidCheck = (CustomEnum)br.ReadUInt32();
            m_complexTypeFlagsHkInt8 = br.ReadSByte();
            br.ReadByte();
            m_complexTypeFlagsHkInt16 = br.ReadInt16();
            m_complexTypeFlagsHkInt32 = br.ReadInt32();
            m_complexTypeFlagsHkUint8 = br.ReadByte();
            br.ReadByte();
            m_complexTypeFlagsHkUint16 = br.ReadUInt16();
            m_complexTypeFlagsHkUint32 = br.ReadUInt32();
            m_complexTypeFlagsHkInt8InvalidCheck = br.ReadSByte();
            br.ReadByte();
            m_complexTypeFlagsHkInt16InvalidCheck = br.ReadInt16();
            m_complexTypeFlagsHkInt32InvalidCheck = br.ReadInt32();
            m_complexTypeFlagsHkUint8InvalidCheck = br.ReadByte();
            br.ReadByte();
            m_complexTypeFlagsHkUint16InvalidCheck = br.ReadUInt16();
            m_complexTypeFlagsHkUint32InvalidCheck = br.ReadUInt32();
            m_complexHiddenTypeCopyEnd = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkReferencedObject>(bw, m_complexTypeHkObjectPtr);
            bw.WriteBoolean(m_complexHiddenTypeCopyStart);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            s.WriteQuaternion(bw, m_complexTypeHkQuaternion);
            s.WriteVector4(bw, m_complexTypeHkVector4);
            bw.WriteSByte((sbyte)m_complexTypeEnumHkInt8);
            bw.WriteByte(0);
            bw.WriteInt16((short)m_complexTypeEnumHkInt16);
            bw.WriteInt32((int)m_complexTypeEnumHkInt32);
            bw.WriteByte((byte)m_complexTypeEnumHkUint8);
            bw.WriteByte(0);
            bw.WriteUInt16((ushort)m_complexTypeEnumHkUint16);
            bw.WriteUInt32((uint)m_complexTypeEnumHkUint32);
            bw.WriteSByte((sbyte)m_complexTypeEnumHkInt8InvalidCheck);
            bw.WriteByte(0);
            bw.WriteInt16((short)m_complexTypeEnumHkInt16InvalidCheck);
            bw.WriteInt32((int)m_complexTypeEnumHkInt32InvalidCheck);
            bw.WriteByte((byte)m_complexTypeEnumHkUint8InvalidCheck);
            bw.WriteByte(0);
            bw.WriteUInt16((ushort)m_complexTypeEnumHkUint16InvalidCheck);
            bw.WriteUInt32((uint)m_complexTypeEnumHkUint32InvalidCheck);
            bw.WriteSByte(m_complexTypeFlagsHkInt8);
            bw.WriteByte(0);
            bw.WriteInt16(m_complexTypeFlagsHkInt16);
            bw.WriteInt32(m_complexTypeFlagsHkInt32);
            bw.WriteByte(m_complexTypeFlagsHkUint8);
            bw.WriteByte(0);
            bw.WriteUInt16(m_complexTypeFlagsHkUint16);
            bw.WriteUInt32(m_complexTypeFlagsHkUint32);
            bw.WriteSByte(m_complexTypeFlagsHkInt8InvalidCheck);
            bw.WriteByte(0);
            bw.WriteInt16(m_complexTypeFlagsHkInt16InvalidCheck);
            bw.WriteInt32(m_complexTypeFlagsHkInt32InvalidCheck);
            bw.WriteByte(m_complexTypeFlagsHkUint8InvalidCheck);
            bw.WriteByte(0);
            bw.WriteUInt16(m_complexTypeFlagsHkUint16InvalidCheck);
            bw.WriteUInt32(m_complexTypeFlagsHkUint32InvalidCheck);
            bw.WriteBoolean(m_complexHiddenTypeCopyEnd);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
