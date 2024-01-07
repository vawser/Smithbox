using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkMemoryResourceHandleExternalLink : IHavokObject
    {
        public virtual uint Signature { get => 826593660; }
        
        public string m_memberName;
        public string m_externalId;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_memberName = des.ReadStringPointer(br);
            m_externalId = des.ReadStringPointer(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteStringPointer(bw, m_memberName);
            s.WriteStringPointer(bw, m_externalId);
        }
    }
}
