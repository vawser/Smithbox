using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbBlenderGeneratorChildInternalState : IHavokObject
    {
        public virtual uint Signature { get => 4285736896; }
        
        public bool m_isActive;
        public bool m_syncNextFrame;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_isActive = br.ReadBoolean();
            m_syncNextFrame = br.ReadBoolean();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteBoolean(m_isActive);
            bw.WriteBoolean(m_syncNextFrame);
        }
    }
}
