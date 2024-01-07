using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclStorageSetupMeshSectionBoneInfluences : hkReferencedObject
    {
        public override uint Signature { get => 3703369081; }
        
        public List<uint> m_boneIndices;
        public List<float> m_weights;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_boneIndices = des.ReadUInt32Array(br);
            m_weights = des.ReadSingleArray(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteUInt32Array(bw, m_boneIndices);
            s.WriteSingleArray(bw, m_weights);
        }
    }
}
