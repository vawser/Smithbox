using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkMonitorStreamColorTableColorPair : IHavokObject
    {
        public virtual uint Signature { get => 361311283; }
        
        public string m_colorName;
        public uint m_color;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_colorName = des.ReadStringPointer(br);
            m_color = br.ReadUInt32();
            br.ReadUInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteStringPointer(bw, m_colorName);
            bw.WriteUInt32(m_color);
            bw.WriteUInt32(0);
        }
    }
}
