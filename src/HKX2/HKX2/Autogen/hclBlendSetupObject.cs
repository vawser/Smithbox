using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclBlendSetupObject : hclOperatorSetupObject
    {
        public override uint Signature { get => 3725915741; }
        
        public string m_name;
        public hclBufferSetupObject m_A;
        public hclBufferSetupObject m_B;
        public hclBufferSetupObject m_C;
        public hclVertexSelectionInput m_vertexSelection;
        public hclVertexFloatInput m_blendWeights;
        public bool m_mapToScurve;
        public bool m_blendNormals;
        public bool m_blendTangents;
        public bool m_blendBitangents;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_name = des.ReadStringPointer(br);
            m_A = des.ReadClassPointer<hclBufferSetupObject>(br);
            m_B = des.ReadClassPointer<hclBufferSetupObject>(br);
            m_C = des.ReadClassPointer<hclBufferSetupObject>(br);
            m_vertexSelection = new hclVertexSelectionInput();
            m_vertexSelection.Read(des, br);
            m_blendWeights = new hclVertexFloatInput();
            m_blendWeights.Read(des, br);
            m_mapToScurve = br.ReadBoolean();
            m_blendNormals = br.ReadBoolean();
            m_blendTangents = br.ReadBoolean();
            m_blendBitangents = br.ReadBoolean();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_name);
            s.WriteClassPointer<hclBufferSetupObject>(bw, m_A);
            s.WriteClassPointer<hclBufferSetupObject>(bw, m_B);
            s.WriteClassPointer<hclBufferSetupObject>(bw, m_C);
            m_vertexSelection.Write(s, bw);
            m_blendWeights.Write(s, bw);
            bw.WriteBoolean(m_mapToScurve);
            bw.WriteBoolean(m_blendNormals);
            bw.WriteBoolean(m_blendTangents);
            bw.WriteBoolean(m_blendBitangents);
            bw.WriteUInt32(0);
        }
    }
}
