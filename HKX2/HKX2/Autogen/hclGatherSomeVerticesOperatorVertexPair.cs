using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclGatherSomeVerticesOperatorVertexPair : IHavokObject
    {
        public virtual uint Signature { get => 473302732; }
        
        public ushort m_indexInput;
        public ushort m_indexOutput;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_indexInput = br.ReadUInt16();
            m_indexOutput = br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt16(m_indexInput);
            bw.WriteUInt16(m_indexOutput);
        }
    }
}
