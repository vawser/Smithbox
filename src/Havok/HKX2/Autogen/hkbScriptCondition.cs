using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbScriptCondition : hkbCondition
    {
        public override uint Signature { get => 1981099063; }
        
        public string m_conditionScript;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_conditionScript = des.ReadStringPointer(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_conditionScript);
        }
    }
}
