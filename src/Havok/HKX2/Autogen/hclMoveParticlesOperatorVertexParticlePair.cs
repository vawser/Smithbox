using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclMoveParticlesOperatorVertexParticlePair : IHavokObject
    {
        public virtual uint Signature { get => 3737834567; }
        
        public ushort m_vertexIndex;
        public ushort m_particleIndex;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_vertexIndex = br.ReadUInt16();
            m_particleIndex = br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt16(m_vertexIndex);
            bw.WriteUInt16(m_particleIndex);
        }
    }
}
