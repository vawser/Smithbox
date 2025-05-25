using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbExpressionCondition : hkbCondition
    {
        public override uint Signature { get => 2024183078; }
        
        public string m_expression;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_expression = des.ReadStringPointer(br);
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_expression);
            bw.WriteUInt64(0);
        }
    }
}
