using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpTyremarkPoint : IHavokObject
    {
        public virtual uint Signature { get => 1807205864; }
        
        public Vector4 m_pointLeft;
        public Vector4 m_pointRight;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_pointLeft = des.ReadVector4(br);
            m_pointRight = des.ReadVector4(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_pointLeft);
            s.WriteVector4(bw, m_pointRight);
        }
    }
}
