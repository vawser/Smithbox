using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclSkinSetupObject : hclOperatorSetupObject
    {
        public override uint Signature { get => 3352518069; }
        
        public string m_name;
        public hclTransformSetSetupObject m_transformSetSetup;
        public hclBufferSetupObject m_referenceBufferSetup;
        public hclBufferSetupObject m_outputBufferSetup;
        public hclVertexSelectionInput m_vertexSelection;
        public bool m_skinNormals;
        public bool m_skinTangents;
        public bool m_skinBiTangents;
        public bool m_useDualQuaternionMethod;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_name = des.ReadStringPointer(br);
            m_transformSetSetup = des.ReadClassPointer<hclTransformSetSetupObject>(br);
            m_referenceBufferSetup = des.ReadClassPointer<hclBufferSetupObject>(br);
            m_outputBufferSetup = des.ReadClassPointer<hclBufferSetupObject>(br);
            m_vertexSelection = new hclVertexSelectionInput();
            m_vertexSelection.Read(des, br);
            m_skinNormals = br.ReadBoolean();
            m_skinTangents = br.ReadBoolean();
            m_skinBiTangents = br.ReadBoolean();
            m_useDualQuaternionMethod = br.ReadBoolean();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_name);
            s.WriteClassPointer<hclTransformSetSetupObject>(bw, m_transformSetSetup);
            s.WriteClassPointer<hclBufferSetupObject>(bw, m_referenceBufferSetup);
            s.WriteClassPointer<hclBufferSetupObject>(bw, m_outputBufferSetup);
            m_vertexSelection.Write(s, bw);
            bw.WriteBoolean(m_skinNormals);
            bw.WriteBoolean(m_skinTangents);
            bw.WriteBoolean(m_skinBiTangents);
            bw.WriteBoolean(m_useDualQuaternionMethod);
            bw.WriteUInt32(0);
        }
    }
}
