using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpStorageExtendedMeshShapeMaterial : hkpMeshMaterial
    {
        public override uint Signature { get => 748939526; }
        
        public short m_restitution;
        public short m_friction;
        public ulong m_userData;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_restitution = br.ReadInt16();
            m_friction = br.ReadInt16();
            m_userData = br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteInt16(m_restitution);
            bw.WriteInt16(m_friction);
            bw.WriteUInt64(m_userData);
        }
    }
}
