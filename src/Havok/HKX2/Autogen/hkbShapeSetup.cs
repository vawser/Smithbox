using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbShapeSetup : IHavokObject
    {
        public virtual uint Signature { get => 3623847614; }
        
        public enum Type
        {
            CAPSULE = 0,
            FILE = 1,
        }
        
        public float m_capsuleHeight;
        public float m_capsuleRadius;
        public string m_fileName;
        public Type m_type;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_capsuleHeight = br.ReadSingle();
            m_capsuleRadius = br.ReadSingle();
            m_fileName = des.ReadStringPointer(br);
            m_type = (Type)br.ReadSByte();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_capsuleHeight);
            bw.WriteSingle(m_capsuleRadius);
            s.WriteStringPointer(bw, m_fileName);
            bw.WriteSByte((sbyte)m_type);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
