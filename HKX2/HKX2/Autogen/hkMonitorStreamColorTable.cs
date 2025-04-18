using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkMonitorStreamColorTable : hkReferencedObject
    {
        public override uint Signature { get => 2650983547; }
        
        public List<hkMonitorStreamColorTableColorPair> m_colorPairs;
        public uint m_defaultColor;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_colorPairs = des.ReadClassArray<hkMonitorStreamColorTableColorPair>(br);
            m_defaultColor = br.ReadUInt32();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkMonitorStreamColorTableColorPair>(bw, m_colorPairs);
            bw.WriteUInt32(m_defaultColor);
            bw.WriteUInt32(0);
        }
    }
}
