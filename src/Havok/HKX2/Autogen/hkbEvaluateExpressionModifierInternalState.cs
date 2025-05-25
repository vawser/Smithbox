using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbEvaluateExpressionModifierInternalState : hkReferencedObject
    {
        public override uint Signature { get => 2587923771; }
        
        public List<hkbEvaluateExpressionModifierInternalExpressionData> m_internalExpressionsData;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_internalExpressionsData = des.ReadClassArray<hkbEvaluateExpressionModifierInternalExpressionData>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkbEvaluateExpressionModifierInternalExpressionData>(bw, m_internalExpressionsData);
        }
    }
}
