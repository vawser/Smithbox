using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpUnaryAction : hknpAction
    {
        public override uint Signature { get => 1699471770; }
        
        public uint m_bodyId;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_bodyId = br.ReadUInt32();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt32(m_bodyId);
            bw.WriteUInt32(0);
        }
    }
}
