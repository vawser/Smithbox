using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclAntiPinchConstraintSetPerParticle : IHavokObject
    {
        public virtual uint Signature { get => 1078087345; }
        
        public ushort m_particleIndex;
        public ushort m_referenceVertex;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_particleIndex = br.ReadUInt16();
            m_referenceVertex = br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt16(m_particleIndex);
            bw.WriteUInt16(m_referenceVertex);
        }
    }
}
