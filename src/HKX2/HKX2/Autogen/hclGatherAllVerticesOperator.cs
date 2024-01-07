using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclGatherAllVerticesOperator : hclOperator
    {
        public override uint Signature { get => 3664999062; }
        
        public List<short> m_vertexInputFromVertexOutput;
        public uint m_inputBufferIdx;
        public uint m_outputBufferIdx;
        public bool m_gatherNormals;
        public bool m_partialGather;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_vertexInputFromVertexOutput = des.ReadInt16Array(br);
            m_inputBufferIdx = br.ReadUInt32();
            m_outputBufferIdx = br.ReadUInt32();
            m_gatherNormals = br.ReadBoolean();
            m_partialGather = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteInt16Array(bw, m_vertexInputFromVertexOutput);
            bw.WriteUInt32(m_inputBufferIdx);
            bw.WriteUInt32(m_outputBufferIdx);
            bw.WriteBoolean(m_gatherNormals);
            bw.WriteBoolean(m_partialGather);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
        }
    }
}
