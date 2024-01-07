using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbJigglerModifierInternalState : hkReferencedObject
    {
        public override uint Signature { get => 2514428783; }
        
        public List<Vector4> m_currentVelocitiesWS;
        public List<Vector4> m_currentPositions;
        public float m_timeStep;
        public bool m_initNextModify;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_currentVelocitiesWS = des.ReadVector4Array(br);
            m_currentPositions = des.ReadVector4Array(br);
            m_timeStep = br.ReadSingle();
            m_initNextModify = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4Array(bw, m_currentVelocitiesWS);
            s.WriteVector4Array(bw, m_currentPositions);
            bw.WriteSingle(m_timeStep);
            bw.WriteBoolean(m_initNextModify);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
