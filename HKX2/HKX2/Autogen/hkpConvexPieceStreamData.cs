using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpConvexPieceStreamData : hkReferencedObject
    {
        public override uint Signature { get => 968978824; }
        
        public List<uint> m_convexPieceStream;
        public List<uint> m_convexPieceOffsets;
        public List<uint> m_convexPieceSingleTriangles;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_convexPieceStream = des.ReadUInt32Array(br);
            m_convexPieceOffsets = des.ReadUInt32Array(br);
            m_convexPieceSingleTriangles = des.ReadUInt32Array(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteUInt32Array(bw, m_convexPieceStream);
            s.WriteUInt32Array(bw, m_convexPieceOffsets);
            s.WriteUInt32Array(bw, m_convexPieceSingleTriangles);
        }
    }
}
