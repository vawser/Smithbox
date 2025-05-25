using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbLeanRocketboxCharacterControllerInternalState : hkReferencedObject
    {
        public override uint Signature { get => 2006244339; }
        
        public int m_currPose;
        public int m_prevPose;
        public float m_characterAngle;
        public int m_plantedFootIdx;
        public float m_timeStep;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_currPose = br.ReadInt32();
            m_prevPose = br.ReadInt32();
            m_characterAngle = br.ReadSingle();
            m_plantedFootIdx = br.ReadInt32();
            m_timeStep = br.ReadSingle();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteInt32(m_currPose);
            bw.WriteInt32(m_prevPose);
            bw.WriteSingle(m_characterAngle);
            bw.WriteInt32(m_plantedFootIdx);
            bw.WriteSingle(m_timeStep);
            bw.WriteUInt32(0);
        }
    }
}
