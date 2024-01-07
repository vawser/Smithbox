using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclBendStiffnessConstraintSet : hclConstraintSet
    {
        public override uint Signature { get => 288448265; }
        
        public List<hclBendStiffnessConstraintSetLink> m_links;
        public bool m_useRestPoseConfig;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_links = des.ReadClassArray<hclBendStiffnessConstraintSetLink>(br);
            m_useRestPoseConfig = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hclBendStiffnessConstraintSetLink>(bw, m_links);
            bw.WriteBoolean(m_useRestPoseConfig);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
