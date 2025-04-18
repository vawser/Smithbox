using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum SimulationControlCommand
    {
        COMMAND_PLAY = 0,
        COMMAND_PAUSE = 1,
        COMMAND_STEP = 2,
        COMMAND_STOP = 3,
        COMMAND_ACCUMULATE_MOTION = 4,
        COMMAND_DO_NOT_ACCUMULATE_MOTION = 5,
    }
    
    public partial class hkbSimulationControlCommand : hkReferencedObject
    {
        public override uint Signature { get => 3375019626; }
        
        public SimulationControlCommand m_command;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_command = (SimulationControlCommand)br.ReadByte();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteByte((byte)m_command);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
