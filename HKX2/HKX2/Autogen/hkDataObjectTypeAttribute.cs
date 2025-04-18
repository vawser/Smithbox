using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkDataObjectTypeAttribute : IHavokObject
    {
        public virtual uint Signature { get => 507008955; }
        
        public string m_typeName;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_typeName = des.ReadStringPointer(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteStringPointer(bw, m_typeName);
        }
    }
}
