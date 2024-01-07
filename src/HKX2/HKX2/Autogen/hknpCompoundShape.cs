using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpCompoundShape : hknpCompositeShape
    {
        public override uint Signature { get => 612195993; }
        
        public hkFreeListArrayhknpShapeInstancehkHandleshort32767hknpShapeInstanceIdDiscriminant8hknpShapeInstance m_instances;
        public hkAabb m_aabb;
        public bool m_isMutable;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_instances = new hkFreeListArrayhknpShapeInstancehkHandleshort32767hknpShapeInstanceIdDiscriminant8hknpShapeInstance();
            m_instances.Read(des, br);
            br.ReadUInt64();
            m_aabb = new hkAabb();
            m_aabb.Read(des, br);
            m_isMutable = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_instances.Write(s, bw);
            bw.WriteUInt64(0);
            m_aabb.Write(s, bw);
            bw.WriteBoolean(m_isMutable);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
