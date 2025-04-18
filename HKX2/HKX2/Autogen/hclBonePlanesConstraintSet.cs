using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclBonePlanesConstraintSet : hclConstraintSet
    {
        public override uint Signature { get => 1522845641; }
        
        public List<hclBonePlanesConstraintSetBonePlane> m_bonePlanes;
        public uint m_transformSetIndex;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_bonePlanes = des.ReadClassArray<hclBonePlanesConstraintSetBonePlane>(br);
            m_transformSetIndex = br.ReadUInt32();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hclBonePlanesConstraintSetBonePlane>(bw, m_bonePlanes);
            bw.WriteUInt32(m_transformSetIndex);
            bw.WriteUInt32(0);
        }
    }
}
