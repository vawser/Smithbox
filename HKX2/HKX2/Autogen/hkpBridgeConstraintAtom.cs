using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpBridgeConstraintAtom : hkpConstraintAtom
    {
        public override uint Signature { get => 3157217341; }
        
        public hkpConstraintData m_constraintData;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadUInt16();
            m_constraintData = des.ReadClassPointer<hkpConstraintData>(br);
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            s.WriteClassPointer<hkpConstraintData>(bw, m_constraintData);
            bw.WriteUInt64(0);
        }
    }
}
