using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpCollisionFilter : hkReferencedObject
    {
        public override uint Signature { get => 477198788; }
        
        public enum Type
        {
            TYPE_UNKNOWN = 0,
            TYPE_CONSTRAINT = 1,
            TYPE_GROUP = 2,
            TYPE_PAIR = 3,
            TYPE_USER = 4,
        }
        
        public Type m_type;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_type = (Type)br.ReadByte();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteByte((byte)m_type);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
