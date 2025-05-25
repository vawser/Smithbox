using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclCopyVerticesOperator : hclOperator
    {
        public override uint Signature { get => 3873113932; }
        
        public uint m_inputBufferIdx;
        public uint m_outputBufferIdx;
        public uint m_numberOfVertices;
        public uint m_startVertexIn;
        public uint m_startVertexOut;
        public bool m_copyNormals;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_inputBufferIdx = br.ReadUInt32();
            m_outputBufferIdx = br.ReadUInt32();
            m_numberOfVertices = br.ReadUInt32();
            m_startVertexIn = br.ReadUInt32();
            m_startVertexOut = br.ReadUInt32();
            m_copyNormals = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt32(m_inputBufferIdx);
            bw.WriteUInt32(m_outputBufferIdx);
            bw.WriteUInt32(m_numberOfVertices);
            bw.WriteUInt32(m_startVertexIn);
            bw.WriteUInt32(m_startVertexOut);
            bw.WriteBoolean(m_copyNormals);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
