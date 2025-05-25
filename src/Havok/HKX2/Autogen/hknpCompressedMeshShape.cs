using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpCompressedMeshShape : hknpCompositeShape
    {
        public override uint Signature { get => 1600181558; }
        
        public hknpCompressedMeshShapeData m_data;
        public hkBitField m_quadIsFlat;
        public hkBitField m_triangleIsInterior;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_data = des.ReadClassPointer<hknpCompressedMeshShapeData>(br);
            m_quadIsFlat = new hkBitField();
            m_quadIsFlat.Read(des, br);
            m_triangleIsInterior = new hkBitField();
            m_triangleIsInterior.Read(des, br);
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hknpCompressedMeshShapeData>(bw, m_data);
            m_quadIsFlat.Write(s, bw);
            m_triangleIsInterior.Write(s, bw);
            bw.WriteUInt64(0);
        }
    }
}
