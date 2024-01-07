using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkResourceBase : hkReferencedObject
    {
        public override uint Signature { get => 591704041; }
        
        public enum Type
        {
            TYPE_RESOURCE = 0,
            TYPE_CONTAINER = 1,
        }
        
        
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
