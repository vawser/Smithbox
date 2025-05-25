using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbMoveCharacterModifier : hkbModifier
    {
        public override uint Signature { get => 2319497728; }
        
        public Vector4 m_offsetPerSecondMS;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_offsetPerSecondMS = des.ReadVector4(br);
            br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            s.WriteVector4(bw, m_offsetPerSecondMS);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
        }
    }
}
