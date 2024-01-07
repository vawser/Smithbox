using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpVehicleDefaultAnalogDriverInput : hknpVehicleDriverInput
    {
        public override uint Signature { get => 2512154512; }
        
        public float m_slopeChangePointX;
        public float m_initialSlope;
        public float m_deadZone;
        public bool m_autoReverse;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_slopeChangePointX = br.ReadSingle();
            m_initialSlope = br.ReadSingle();
            m_deadZone = br.ReadSingle();
            m_autoReverse = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_slopeChangePointX);
            bw.WriteSingle(m_initialSlope);
            bw.WriteSingle(m_deadZone);
            bw.WriteBoolean(m_autoReverse);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
