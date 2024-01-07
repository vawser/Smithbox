using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclMeshBoneDeformOperator : hclOperator
    {
        public override uint Signature { get => 4040380508; }
        
        public uint m_inputBufferIdx;
        public uint m_outputTransformSetIdx;
        public List<hclMeshBoneDeformOperatorTriangleBonePair> m_triangleBonePairs;
        public List<ushort> m_triangleBoneStartForBone;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_inputBufferIdx = br.ReadUInt32();
            m_outputTransformSetIdx = br.ReadUInt32();
            m_triangleBonePairs = des.ReadClassArray<hclMeshBoneDeformOperatorTriangleBonePair>(br);
            m_triangleBoneStartForBone = des.ReadUInt16Array(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt32(m_inputBufferIdx);
            bw.WriteUInt32(m_outputTransformSetIdx);
            s.WriteClassArray<hclMeshBoneDeformOperatorTriangleBonePair>(bw, m_triangleBonePairs);
            s.WriteUInt16Array(bw, m_triangleBoneStartForBone);
        }
    }
}
