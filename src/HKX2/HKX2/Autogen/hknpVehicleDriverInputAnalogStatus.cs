using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpVehicleDriverInputAnalogStatus : hknpVehicleDriverInputStatus
    {
        public override uint Signature { get => 3791005235; }
        
        public float m_positionX;
        public float m_positionY;
        public bool m_handbrakeButtonPressed;
        public bool m_reverseButtonPressed;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_positionX = br.ReadSingle();
            m_positionY = br.ReadSingle();
            m_handbrakeButtonPressed = br.ReadBoolean();
            m_reverseButtonPressed = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteSingle(m_positionX);
            bw.WriteSingle(m_positionY);
            bw.WriteBoolean(m_handbrakeButtonPressed);
            bw.WriteBoolean(m_reverseButtonPressed);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
        }
    }
}
