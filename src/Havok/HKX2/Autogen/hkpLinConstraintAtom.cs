using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpLinConstraintAtom : hkpConstraintAtom
    {
        public override uint Signature { get => 695691591; }
        
        public byte m_axisIndex;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_axisIndex = br.ReadByte();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteByte(m_axisIndex);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteByte(0);
        }
    }
}
