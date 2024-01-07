using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiPlaneVolume : hkaiVolume
    {
        public override uint Signature { get => 361949590; }
        
        public List<Vector4> m_planes;
        public hkGeometry m_geometry;
        public bool m_isInverted;
        public hkAabb m_aabb;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_planes = des.ReadVector4Array(br);
            m_geometry = new hkGeometry();
            m_geometry.Read(des, br);
            m_isInverted = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
            m_aabb = new hkAabb();
            m_aabb.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4Array(bw, m_planes);
            m_geometry.Write(s, bw);
            bw.WriteBoolean(m_isInverted);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            m_aabb.Write(s, bw);
        }
    }
}
