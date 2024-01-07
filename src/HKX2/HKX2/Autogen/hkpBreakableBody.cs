using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpBreakableBody : hkReferencedObject
    {
        public override uint Signature { get => 2470556408; }
        
        public hkpBreakableBodyController m_controller;
        public hkpBreakableShape m_breakableShape;
        public byte m_bodyTypeAndFlags;
        public short m_constraintStrength;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_controller = des.ReadClassPointer<hkpBreakableBodyController>(br);
            m_breakableShape = des.ReadClassPointer<hkpBreakableShape>(br);
            m_bodyTypeAndFlags = br.ReadByte();
            br.ReadByte();
            m_constraintStrength = br.ReadInt16();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkpBreakableBodyController>(bw, m_controller);
            s.WriteClassPointer<hkpBreakableShape>(bw, m_breakableShape);
            bw.WriteByte(m_bodyTypeAndFlags);
            bw.WriteByte(0);
            bw.WriteInt16(m_constraintStrength);
            bw.WriteUInt32(0);
        }
    }
}
