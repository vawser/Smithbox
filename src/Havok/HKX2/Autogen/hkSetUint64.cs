using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkSetUint64 : hkSetunsignedlonglonghkContainerHeapAllocatorhkMapOperationsunsignedlonglong
    {
        public override uint Signature { get => 3613704676; }
        
        
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
