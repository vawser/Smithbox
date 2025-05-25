using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbStringCondition : hkbCondition
    {
        public override uint Signature { get => 2482243061; }
        
        public string m_conditionString;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_conditionString = des.ReadStringPointer(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_conditionString);
        }
    }
}
