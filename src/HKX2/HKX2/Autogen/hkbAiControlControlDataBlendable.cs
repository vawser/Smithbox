using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbAiControlControlDataBlendable : IHavokObject
    {
        public virtual uint Signature { get => 3146587068; }
        
        public float m_desiredSpeed;
        public float m_maximumSpeed;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_desiredSpeed = br.ReadSingle();
            m_maximumSpeed = br.ReadSingle();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_desiredSpeed);
            bw.WriteSingle(m_maximumSpeed);
        }
    }
}
