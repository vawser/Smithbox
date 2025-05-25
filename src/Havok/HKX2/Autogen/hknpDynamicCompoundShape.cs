using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpDynamicCompoundShape : hknpCompoundShape
    {
        public override uint Signature { get => 1176555804; }
        
        public hknpDynamicCompoundShapeData m_boundingVolumeData;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_boundingVolumeData = des.ReadClassPointer<hknpDynamicCompoundShapeData>(br);
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hknpDynamicCompoundShapeData>(bw, m_boundingVolumeData);
            bw.WriteUInt64(0);
        }
    }
}
