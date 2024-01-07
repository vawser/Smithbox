using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpConstraintChainInstanceAction : hkpAction
    {
        public override uint Signature { get => 3001999328; }
        
        public hkpConstraintChainInstance m_constraintInstance;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_constraintInstance = des.ReadClassPointer<hkpConstraintChainInstance>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointer<hkpConstraintChainInstance>(bw, m_constraintInstance);
        }
    }
}
