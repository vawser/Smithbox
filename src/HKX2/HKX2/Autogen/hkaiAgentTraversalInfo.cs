using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiAgentTraversalInfo : IHavokObject
    {
        public virtual uint Signature { get => 3554359924; }
        
        public float m_diameter;
        public uint m_filterInfo;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_diameter = br.ReadSingle();
            m_filterInfo = br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_diameter);
            bw.WriteUInt32(m_filterInfo);
        }
    }
}
