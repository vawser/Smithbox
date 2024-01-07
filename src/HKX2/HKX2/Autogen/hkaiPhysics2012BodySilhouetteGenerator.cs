using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiPhysics2012BodySilhouetteGenerator : hkaiPhysicsBodySilhouetteGeneratorBase
    {
        public override uint Signature { get => 2085043402; }
        
        public hkpRigidBody m_rigidBody;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_rigidBody = des.ReadClassPointer<hkpRigidBody>(br);
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkpRigidBody>(bw, m_rigidBody);
            bw.WriteUInt64(0);
        }
    }
}
