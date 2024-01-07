using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpDecoratorShape : hknpShape
    {
        public override uint Signature { get => 430716898; }
        
        public hknpShape m_coreShape;
        public int m_coreShapeSize;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_coreShape = des.ReadClassPointer<hknpShape>(br);
            m_coreShapeSize = br.ReadInt32();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hknpShape>(bw, m_coreShape);
            bw.WriteInt32(m_coreShapeSize);
            bw.WriteUInt32(0);
        }
    }
}
