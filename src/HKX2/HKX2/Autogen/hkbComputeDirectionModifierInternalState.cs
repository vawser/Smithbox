using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbComputeDirectionModifierInternalState : hkReferencedObject
    {
        public override uint Signature { get => 3034672341; }
        
        public Vector4 m_pointOut;
        public float m_groundAngleOut;
        public float m_upAngleOut;
        public bool m_computedOutput;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_pointOut = des.ReadVector4(br);
            m_groundAngleOut = br.ReadSingle();
            m_upAngleOut = br.ReadSingle();
            m_computedOutput = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4(bw, m_pointOut);
            bw.WriteSingle(m_groundAngleOut);
            bw.WriteSingle(m_upAngleOut);
            bw.WriteBoolean(m_computedOutput);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
