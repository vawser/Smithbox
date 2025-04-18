using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpLinLimitConstraintAtom : hkpConstraintAtom
    {
        public override uint Signature { get => 1554835741; }
        
        public byte m_axisIndex;
        public float m_min;
        public float m_max;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_axisIndex = br.ReadByte();
            br.ReadByte();
            m_min = br.ReadSingle();
            m_max = br.ReadSingle();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteByte(m_axisIndex);
            bw.WriteByte(0);
            bw.WriteSingle(m_min);
            bw.WriteSingle(m_max);
            bw.WriteUInt32(0);
        }
    }
}
