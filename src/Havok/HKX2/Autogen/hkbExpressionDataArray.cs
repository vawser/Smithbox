using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbExpressionDataArray : hkReferencedObject
    {
        public override uint Signature { get => 515884759; }
        
        public List<hkbExpressionData> m_expressionsData;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_expressionsData = des.ReadClassArray<hkbExpressionData>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkbExpressionData>(bw, m_expressionsData);
        }
    }
}
