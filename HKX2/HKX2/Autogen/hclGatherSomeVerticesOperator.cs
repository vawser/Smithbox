using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclGatherSomeVerticesOperator : hclOperator
    {
        public override uint Signature { get => 2237234416; }
        
        public List<hclGatherSomeVerticesOperatorVertexPair> m_vertexPairs;
        public uint m_inputBufferIdx;
        public uint m_outputBufferIdx;
        public bool m_gatherNormals;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_vertexPairs = des.ReadClassArray<hclGatherSomeVerticesOperatorVertexPair>(br);
            m_inputBufferIdx = br.ReadUInt32();
            m_outputBufferIdx = br.ReadUInt32();
            m_gatherNormals = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hclGatherSomeVerticesOperatorVertexPair>(bw, m_vertexPairs);
            bw.WriteUInt32(m_inputBufferIdx);
            bw.WriteUInt32(m_outputBufferIdx);
            bw.WriteBoolean(m_gatherNormals);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
