using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpBinaryAction : hkpAction
    {
        public override uint Signature { get => 1140005523; }
        
        public hkpEntity m_entityA;
        public hkpEntity m_entityB;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_entityA = des.ReadClassPointer<hkpEntity>(br);
            m_entityB = des.ReadClassPointer<hkpEntity>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkpEntity>(bw, m_entityA);
            s.WriteClassPointer<hkpEntity>(bw, m_entityB);
        }
    }
}
