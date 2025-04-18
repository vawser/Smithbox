using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclMeshBoneDeformOperatorTriangleBonePair : IHavokObject
    {
        public virtual uint Signature { get => 634414815; }
        
        public Matrix4x4 m_localBoneTransform;
        public float m_weight;
        public ushort m_triangleIndex;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_localBoneTransform = des.ReadMatrix4(br);
            m_weight = br.ReadSingle();
            m_triangleIndex = br.ReadUInt16();
            br.ReadUInt64();
            br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteMatrix4(bw, m_localBoneTransform);
            bw.WriteSingle(m_weight);
            bw.WriteUInt16(m_triangleIndex);
            bw.WriteUInt64(0);
            bw.WriteUInt16(0);
        }
    }
}
