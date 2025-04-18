using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpSerializedDisplayMarkerList : hkReferencedObject
    {
        public override uint Signature { get => 3481255883; }
        
        public List<hkpSerializedDisplayMarker> m_markers;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_markers = des.ReadClassPointerArray<hkpSerializedDisplayMarker>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hkpSerializedDisplayMarker>(bw, m_markers);
        }
    }
}
