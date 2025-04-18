using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum MaxConstraintSetSize
    {
        MAX_CONSTRAINT_SET_SIZE = 128,
    }
    
    public partial class hclConstraintSet : hkReferencedObject
    {
        public override uint Signature { get => 789956651; }
        
        public string m_name;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_name = des.ReadStringPointer(br);
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_name);
            bw.WriteUInt64(0);
        }
    }
}
