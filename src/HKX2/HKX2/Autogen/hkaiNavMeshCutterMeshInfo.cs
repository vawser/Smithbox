using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiNavMeshCutterMeshInfo : IHavokObject
    {
        public virtual uint Signature { get => 3865013252; }
        
        public int m_originalNumFaces;
        public int m_originalNumEdges;
        public int m_originalNumVertices;
        public List<int> m_magic;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_originalNumFaces = br.ReadInt32();
            m_originalNumEdges = br.ReadInt32();
            m_originalNumVertices = br.ReadInt32();
            br.ReadUInt32();
            m_magic = des.ReadInt32Array(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt32(m_originalNumFaces);
            bw.WriteInt32(m_originalNumEdges);
            bw.WriteInt32(m_originalNumVertices);
            bw.WriteUInt32(0);
            s.WriteInt32Array(bw, m_magic);
        }
    }
}
