using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum DeprecatedFlagValues
    {
        DEPRECATED_SIZE_8 = 8,
        DEPRECATED_ENUM_8 = 8,
        DEPRECATED_SIZE_16 = 16,
        DEPRECATED_ENUM_16 = 16,
        DEPRECATED_SIZE_32 = 32,
        DEPRECATED_ENUM_32 = 32,
    }
    
    public partial class hkClassMember : IHavokObject
    {
        public virtual uint Signature { get => 2968495897; }
        
        public enum Type
        {
            TYPE_VOID = 0,
            TYPE_BOOL = 1,
            TYPE_CHAR = 2,
            TYPE_INT8 = 3,
            TYPE_UINT8 = 4,
            TYPE_INT16 = 5,
            TYPE_UINT16 = 6,
            TYPE_INT32 = 7,
            TYPE_UINT32 = 8,
            TYPE_INT64 = 9,
            TYPE_UINT64 = 10,
            TYPE_REAL = 11,
            TYPE_VECTOR4 = 12,
            TYPE_QUATERNION = 13,
            TYPE_MATRIX3 = 14,
            TYPE_ROTATION = 15,
            TYPE_QSTRANSFORM = 16,
            TYPE_MATRIX4 = 17,
            TYPE_TRANSFORM = 18,
            TYPE_ZERO = 19,
            TYPE_POINTER = 20,
            TYPE_FUNCTIONPOINTER = 21,
            TYPE_ARRAY = 22,
            TYPE_INPLACEARRAY = 23,
            TYPE_ENUM = 24,
            TYPE_STRUCT = 25,
            TYPE_SIMPLEARRAY = 26,
            TYPE_HOMOGENEOUSARRAY = 27,
            TYPE_VARIANT = 28,
            TYPE_CSTRING = 29,
            TYPE_ULONG = 30,
            TYPE_FLAGS = 31,
            TYPE_HALF = 32,
            TYPE_STRINGPTR = 33,
            TYPE_RELARRAY = 34,
            TYPE_MAX = 35,
        }
        
        public enum FlagValues
        {
            FLAGS_NONE = 0,
            ALIGN_8 = 128,
            ALIGN_16 = 256,
            NOT_OWNED = 512,
            SERIALIZE_IGNORED = 1024,
            ALIGN_32 = 2048,
            ALIGN_REAL = 256,
        }
        
        public string m_name;
        public hkClass m_class;
        public hkClassEnum m_enum;
        public Type m_type;
        public Type m_subtype;
        public short m_cArraySize;
        public ushort m_flags;
        public ushort m_offset;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_name = des.ReadStringPointer(br);
            m_class = des.ReadClassPointer<hkClass>(br);
            m_enum = des.ReadClassPointer<hkClassEnum>(br);
            m_type = (Type)br.ReadByte();
            m_subtype = (Type)br.ReadByte();
            m_cArraySize = br.ReadInt16();
            m_flags = br.ReadUInt16();
            m_offset = br.ReadUInt16();
            br.ReadUInt64();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteStringPointer(bw, m_name);
            s.WriteClassPointer<hkClass>(bw, m_class);
            s.WriteClassPointer<hkClassEnum>(bw, m_enum);
            bw.WriteByte((byte)m_type);
            bw.WriteByte((byte)m_subtype);
            bw.WriteInt16(m_cArraySize);
            bw.WriteUInt16(m_flags);
            bw.WriteUInt16(m_offset);
            bw.WriteUInt64(0);
        }
    }
}
