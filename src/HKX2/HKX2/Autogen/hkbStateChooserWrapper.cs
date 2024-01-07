using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbStateChooserWrapper : hkbCustomIdSelector
    {
        public override uint Signature { get => 3884430078; }
        
        public hkbStateChooser m_wrappedChooser;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_wrappedChooser = des.ReadClassPointer<hkbStateChooser>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkbStateChooser>(bw, m_wrappedChooser);
        }
    }
}
