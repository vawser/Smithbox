using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpMaterialPalette : hkReferencedObject
    {
        public override uint Signature { get => 416898539; }
        
        public List<hknpMaterialDescriptor> m_entries;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_entries = des.ReadClassArray<hknpMaterialDescriptor>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hknpMaterialDescriptor>(bw, m_entries);
        }
    }
}
