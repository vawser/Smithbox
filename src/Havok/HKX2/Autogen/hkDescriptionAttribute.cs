using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkDescriptionAttribute : IHavokObject
    {
        public virtual uint Signature { get => 3925432202; }
        
        public string m_string;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_string = des.ReadStringPointer(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteStringPointer(bw, m_string);
        }
    }
}
