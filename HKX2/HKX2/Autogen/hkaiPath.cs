using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum PathPointBits
    {
        EDGE_TYPE_USER_START = 1,
        EDGE_TYPE_USER_END = 2,
        EDGE_TYPE_SEGMENT_START = 4,
        EDGE_TYPE_SEGMENT_END = 8,
    }
    
    public enum ReferenceFrame
    {
        REFERENCE_FRAME_WORLD = 0,
        REFERENCE_FRAME_SECTION_LOCAL = 1,
        REFERENCE_FRAME_SECTION_FIXED = 2,
    }
    
    public partial class hkaiPath : hkReferencedObject
    {
        public override uint Signature { get => 2336010493; }
        
        public List<hkaiPathPathPoint> m_points;
        public ReferenceFrame m_referenceFrame;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_points = des.ReadClassArray<hkaiPathPathPoint>(br);
            m_referenceFrame = (ReferenceFrame)br.ReadByte();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkaiPathPathPoint>(bw, m_points);
            bw.WriteByte((byte)m_referenceFrame);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
