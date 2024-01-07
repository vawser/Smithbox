using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpStaticCompoundShape : hknpCompoundShape
    {
        public override uint Signature { get => 1176555804; }
        
        public hknpStaticCompoundShapeData m_boundingVolumeData;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_boundingVolumeData = des.ReadClassPointer<hknpStaticCompoundShapeData>(br);
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hknpStaticCompoundShapeData>(bw, m_boundingVolumeData);
            bw.WriteUInt64(0);
        }
    }
}
