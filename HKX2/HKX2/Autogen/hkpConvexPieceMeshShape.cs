using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpConvexPieceMeshShape : hkpShapeCollection
    {
        public override uint Signature { get => 3189527626; }
        
        public hkpConvexPieceStreamData m_convexPieceStream;
        public hkpShapeCollection m_displayMesh;
        public float m_radius;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_convexPieceStream = des.ReadClassPointer<hkpConvexPieceStreamData>(br);
            m_displayMesh = des.ReadClassPointer<hkpShapeCollection>(br);
            m_radius = br.ReadSingle();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkpConvexPieceStreamData>(bw, m_convexPieceStream);
            s.WriteClassPointer<hkpShapeCollection>(bw, m_displayMesh);
            bw.WriteSingle(m_radius);
            bw.WriteUInt32(0);
        }
    }
}
