using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbRocketboxCharacterControllerInternalState : hkReferencedObject
    {
        public override uint Signature { get => 1772527852; }
        
        public bool m_rapidTurnRequest;
        public int m_currPose;
        public int m_prevPose;
        public float m_noVelocityTimer;
        public float m_linearSpeedModifier;
        public float m_characterAngle;
        public int m_plantedFootIdx;
        public float m_timeStep;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_rapidTurnRequest = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_currPose = br.ReadInt32();
            m_prevPose = br.ReadInt32();
            m_noVelocityTimer = br.ReadSingle();
            m_linearSpeedModifier = br.ReadSingle();
            m_characterAngle = br.ReadSingle();
            m_plantedFootIdx = br.ReadInt32();
            m_timeStep = br.ReadSingle();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteBoolean(m_rapidTurnRequest);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteInt32(m_currPose);
            bw.WriteInt32(m_prevPose);
            bw.WriteSingle(m_noVelocityTimer);
            bw.WriteSingle(m_linearSpeedModifier);
            bw.WriteSingle(m_characterAngle);
            bw.WriteInt32(m_plantedFootIdx);
            bw.WriteSingle(m_timeStep);
        }
    }
}
