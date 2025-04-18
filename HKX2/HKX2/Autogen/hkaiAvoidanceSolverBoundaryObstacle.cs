using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiAvoidanceSolverBoundaryObstacle : IHavokObject
    {
        public virtual uint Signature { get => 4292610917; }
        
        public Vector4 m_start;
        public Vector4 m_end;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_start = des.ReadVector4(br);
            m_end = des.ReadVector4(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_start);
            s.WriteVector4(bw, m_end);
        }
    }
}
