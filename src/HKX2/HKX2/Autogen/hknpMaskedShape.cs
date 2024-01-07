using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpMaskedShape : hknpDecoratorShape
    {
        public override uint Signature { get => 2422915949; }
        
        public hknpShapeKeyMask m_mask;
        public hknpMaskedShapeMaskWrapper m_maskWrapper;
        public int m_maskSize;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_mask = des.ReadClassPointer<hknpShapeKeyMask>(br);
            m_maskWrapper = new hknpMaskedShapeMaskWrapper();
            m_maskWrapper.Read(des, br);
            br.ReadUInt64();
            br.ReadUInt64();
            m_maskSize = br.ReadInt32();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hknpShapeKeyMask>(bw, m_mask);
            m_maskWrapper.Write(s, bw);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteInt32(m_maskSize);
            bw.WriteUInt32(0);
        }
    }
}
