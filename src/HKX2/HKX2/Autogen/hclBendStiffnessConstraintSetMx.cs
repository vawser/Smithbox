using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclBendStiffnessConstraintSetMx : hclConstraintSet
    {
        public override uint Signature { get => 2368095515; }
        
        public List<hclBendStiffnessConstraintSetMxBatch> m_batches;
        public List<hclBendStiffnessConstraintSetMxSingle> m_singles;
        public bool m_useRestPoseConfig;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_batches = des.ReadClassArray<hclBendStiffnessConstraintSetMxBatch>(br);
            m_singles = des.ReadClassArray<hclBendStiffnessConstraintSetMxSingle>(br);
            m_useRestPoseConfig = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hclBendStiffnessConstraintSetMxBatch>(bw, m_batches);
            s.WriteClassArray<hclBendStiffnessConstraintSetMxSingle>(bw, m_singles);
            bw.WriteBoolean(m_useRestPoseConfig);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
