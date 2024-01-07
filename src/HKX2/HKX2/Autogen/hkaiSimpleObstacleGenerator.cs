using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiSimpleObstacleGenerator : hkaiObstacleGenerator
    {
        public override uint Signature { get => 530878806; }
        
        public hkAabb m_localAabb;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_localAabb = new hkAabb();
            m_localAabb.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_localAabb.Write(s, bw);
        }
    }
}
