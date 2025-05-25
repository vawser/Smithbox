using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbEventsFromRangeModifierInternalState : hkReferencedObject
    {
        public override uint Signature { get => 3983878176; }
        
        public List<bool> m_wasActiveInPreviousFrame;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_wasActiveInPreviousFrame = des.ReadBooleanArray(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteBooleanArray(bw, m_wasActiveInPreviousFrame);
        }
    }
}
