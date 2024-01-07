using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkDocumentationAttribute : IHavokObject
    {
        public virtual uint Signature { get => 1661918622; }
        
        public string m_docsSectionTag;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_docsSectionTag = des.ReadStringPointer(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteStringPointer(bw, m_docsSectionTag);
        }
    }
}
