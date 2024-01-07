using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiAdaptiveRanger : IHavokObject
    {
        public virtual uint Signature { get => 529054584; }
        
        public float m_curRange;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_curRange = br.ReadSingle();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_curRange);
        }
    }
}
