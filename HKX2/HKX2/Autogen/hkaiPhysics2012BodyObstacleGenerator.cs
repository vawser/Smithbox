using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiPhysics2012BodyObstacleGenerator : hkaiObstacleGenerator
    {
        public override uint Signature { get => 729055412; }
        
        public float m_velocityThreshold;
        public hkpRigidBody m_rigidBody;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_velocityThreshold = br.ReadSingle();
            br.ReadUInt32();
            m_rigidBody = des.ReadClassPointer<hkpRigidBody>(br);
            br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_velocityThreshold);
            bw.WriteUInt32(0);
            s.WriteClassPointer<hkpRigidBody>(bw, m_rigidBody);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
        }
    }
}
