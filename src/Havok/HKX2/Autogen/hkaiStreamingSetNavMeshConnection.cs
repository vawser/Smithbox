using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiStreamingSetNavMeshConnection : IHavokObject
    {
        public virtual uint Signature { get => 2672106245; }
        
        public int m_faceIndex;
        public int m_edgeIndex;
        public int m_oppositeFaceIndex;
        public int m_oppositeEdgeIndex;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_faceIndex = br.ReadInt32();
            m_edgeIndex = br.ReadInt32();
            m_oppositeFaceIndex = br.ReadInt32();
            m_oppositeEdgeIndex = br.ReadInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt32(m_faceIndex);
            bw.WriteInt32(m_edgeIndex);
            bw.WriteInt32(m_oppositeFaceIndex);
            bw.WriteInt32(m_oppositeEdgeIndex);
        }
    }
}
