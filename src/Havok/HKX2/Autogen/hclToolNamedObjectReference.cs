using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclToolNamedObjectReference : IHavokObject
    {
        public virtual uint Signature { get => 3490533769; }
        
        public string m_pluginName;
        public string m_objectName;
        public uint m_hash;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_pluginName = des.ReadStringPointer(br);
            m_objectName = des.ReadStringPointer(br);
            m_hash = br.ReadUInt32();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteStringPointer(bw, m_pluginName);
            s.WriteStringPointer(bw, m_objectName);
            bw.WriteUInt32(m_hash);
            bw.WriteUInt32(0);
        }
    }
}
