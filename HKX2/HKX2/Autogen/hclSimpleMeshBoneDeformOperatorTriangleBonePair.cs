using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclSimpleMeshBoneDeformOperatorTriangleBonePair : IHavokObject
    {
        public virtual uint Signature { get => 2799224437; }
        
        public ushort m_boneOffset;
        public ushort m_triangleOffset;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_boneOffset = br.ReadUInt16();
            m_triangleOffset = br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt16(m_boneOffset);
            bw.WriteUInt16(m_triangleOffset);
        }
    }
}
