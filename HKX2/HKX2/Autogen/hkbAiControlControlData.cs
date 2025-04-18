using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbAiControlControlData : IHavokObject
    {
        public virtual uint Signature { get => 408861304; }
        
        public hkbAiControlControlDataBlendable m_blendable;
        public hkbAiControlControlDataNonBlendable m_nonBlendable;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_blendable = new hkbAiControlControlDataBlendable();
            m_blendable.Read(des, br);
            m_nonBlendable = new hkbAiControlControlDataNonBlendable();
            m_nonBlendable.Read(des, br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_blendable.Write(s, bw);
            m_nonBlendable.Write(s, bw);
        }
    }
}
