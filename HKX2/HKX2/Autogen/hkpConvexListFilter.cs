using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum ConvexListCollisionType
    {
        TREAT_CONVEX_LIST_AS_NORMAL = 0,
        TREAT_CONVEX_LIST_AS_LIST = 1,
        TREAT_CONVEX_LIST_AS_CONVEX = 2,
    }
    
    public partial class hkpConvexListFilter : hkReferencedObject
    {
        public override uint Signature { get => 15155222; }
        
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
        }
    }
}
