using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbLookAtModifierInternalState : hkReferencedObject
    {
        public override uint Signature { get => 586253050; }
        
        public Vector4 m_lookAtLastTargetWS;
        public float m_lookAtWeight;
        public bool m_isTargetInsideLimitCone;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_lookAtLastTargetWS = des.ReadVector4(br);
            m_lookAtWeight = br.ReadSingle();
            m_isTargetInsideLimitCone = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4(bw, m_lookAtLastTargetWS);
            bw.WriteSingle(m_lookAtWeight);
            bw.WriteBoolean(m_isTargetInsideLimitCone);
            bw.WriteUInt64(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
