using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum ConstraintSource
    {
        NO_CONSTRAINTS = 0,
        REFERENCE_POSE = 1,
        CURRENT_POSE = 2,
    }
    
    public partial class hkaSkeletonMapper : hkReferencedObject
    {
        public override uint Signature { get => 2900984988; }
        
        public hkaSkeletonMapperData m_mapping;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_mapping = new hkaSkeletonMapperData();
            m_mapping.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_mapping.Write(s, bw);
        }
    }
}
