using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkxAnimatedQuaternion : hkReferencedObject
    {
        public override uint Signature { get => 83194395; }
        
        public List<float> m_quaternions;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_quaternions = des.ReadSingleArray(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteSingleArray(bw, m_quaternions);
        }
    }
}
