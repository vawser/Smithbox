using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpVehicleDefaultBrakeWheelBrakingProperties : IHavokObject
    {
        public virtual uint Signature { get => 536533361; }
        
        public float m_maxBreakingTorque;
        public float m_minPedalInputToBlock;
        public bool m_isConnectedToHandbrake;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_maxBreakingTorque = br.ReadSingle();
            m_minPedalInputToBlock = br.ReadSingle();
            m_isConnectedToHandbrake = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_maxBreakingTorque);
            bw.WriteSingle(m_minPedalInputToBlock);
            bw.WriteBoolean(m_isConnectedToHandbrake);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
