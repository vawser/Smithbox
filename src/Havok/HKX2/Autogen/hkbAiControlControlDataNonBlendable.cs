using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbAiControlControlDataNonBlendable : IHavokObject
    {
        public virtual uint Signature { get => 2809465809; }
        
        public bool m_canControl;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_canControl = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteBoolean(m_canControl);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
