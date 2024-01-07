using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpBinaryAction : hknpAction
    {
        public override uint Signature { get => 1464012291; }
        
        public uint m_bodyIdA;
        public uint m_bodyIdB;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_bodyIdA = br.ReadUInt32();
            m_bodyIdB = br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt32(m_bodyIdA);
            bw.WriteUInt32(m_bodyIdB);
        }
    }
}
