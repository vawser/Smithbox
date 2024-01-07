using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpAngFrictionConstraintAtom : hkpConstraintAtom
    {
        public override uint Signature { get => 2314667299; }
        
        public byte m_isEnabled;
        public byte m_firstFrictionAxis;
        public byte m_numFrictionAxes;
        public float m_maxFrictionTorque;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_isEnabled = br.ReadByte();
            m_firstFrictionAxis = br.ReadByte();
            m_numFrictionAxes = br.ReadByte();
            br.ReadUInt16();
            br.ReadByte();
            m_maxFrictionTorque = br.ReadSingle();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteByte(m_isEnabled);
            bw.WriteByte(m_firstFrictionAxis);
            bw.WriteByte(m_numFrictionAxes);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteSingle(m_maxFrictionTorque);
            bw.WriteUInt32(0);
        }
    }
}
