using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiPhysicsBodySilhouetteGeneratorBase : hkaiPointCloudSilhouetteGenerator
    {
        public override uint Signature { get => 2485748655; }
        
        public Vector4 m_linearVelocityAndThreshold;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_linearVelocityAndThreshold = des.ReadVector4(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4(bw, m_linearVelocityAndThreshold);
        }
    }
}
