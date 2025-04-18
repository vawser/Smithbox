using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclSimpleMeshBoneDeformOperator : hclOperator
    {
        public override uint Signature { get => 2161735327; }
        
        public uint m_inputBufferIdx;
        public uint m_outputTransformSetIdx;
        public List<hclSimpleMeshBoneDeformOperatorTriangleBonePair> m_triangleBonePairs;
        public List<Matrix4x4> m_localBoneTransforms;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_inputBufferIdx = br.ReadUInt32();
            m_outputTransformSetIdx = br.ReadUInt32();
            m_triangleBonePairs = des.ReadClassArray<hclSimpleMeshBoneDeformOperatorTriangleBonePair>(br);
            m_localBoneTransforms = des.ReadMatrix4Array(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt32(m_inputBufferIdx);
            bw.WriteUInt32(m_outputTransformSetIdx);
            s.WriteClassArray<hclSimpleMeshBoneDeformOperatorTriangleBonePair>(bw, m_triangleBonePairs);
            s.WriteMatrix4Array(bw, m_localBoneTransforms);
        }
    }
}
