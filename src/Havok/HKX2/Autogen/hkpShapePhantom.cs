using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpShapePhantom : hkpPhantom
    {
        public override uint Signature { get => 2107198013; }
        
        public hkMotionState m_motionState;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_motionState = new hkMotionState();
            m_motionState.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            m_motionState.Write(s, bw);
        }
    }
}
