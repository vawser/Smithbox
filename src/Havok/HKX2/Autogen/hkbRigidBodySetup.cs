using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbRigidBodySetup : IHavokObject
    {
        public virtual uint Signature { get => 990392213; }
        
        public enum Type
        {
            INVALID = -1,
            KEYFRAMED = 0,
            DYNAMIC = 1,
            FIXED = 2,
        }
        
        public uint m_collisionFilterInfo;
        public Type m_type;
        public hkbShapeSetup m_shapeSetup;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_collisionFilterInfo = br.ReadUInt32();
            m_type = (Type)br.ReadSByte();
            br.ReadUInt16();
            br.ReadByte();
            m_shapeSetup = new hkbShapeSetup();
            m_shapeSetup.Read(des, br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt32(m_collisionFilterInfo);
            bw.WriteSByte((sbyte)m_type);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            m_shapeSetup.Write(s, bw);
        }
    }
}
