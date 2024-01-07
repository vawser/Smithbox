using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclUpdateSomeVertexFramesOperatorTriangle : IHavokObject
    {
        public virtual uint Signature { get => 814311393; }
        
        public ushort m_indices_0;
        public ushort m_indices_1;
        public ushort m_indices_2;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_indices_0 = br.ReadUInt16();
            m_indices_1 = br.ReadUInt16();
            m_indices_2 = br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt16(m_indices_0);
            bw.WriteUInt16(m_indices_1);
            bw.WriteUInt16(m_indices_2);
        }
    }
}
