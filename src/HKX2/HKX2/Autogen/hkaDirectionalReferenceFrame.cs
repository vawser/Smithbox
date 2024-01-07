using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaDirectionalReferenceFrame : hkaParameterizedReferenceFrame
    {
        public override uint Signature { get => 3058674048; }
        
        public Vector4 m_movementDir;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_movementDir = des.ReadVector4(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4(bw, m_movementDir);
        }
    }
}
