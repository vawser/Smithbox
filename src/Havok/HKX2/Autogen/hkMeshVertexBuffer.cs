using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum LockResult
    {
        RESULT_FAILURE = 0,
        RESULT_SUCCESS = 1,
        RESULT_IN_USE = 2,
    }
    
    public partial class hkMeshVertexBuffer : hkReferencedObject
    {
        public override uint Signature { get => 444511801; }
        
        public enum Flags
        {
            ACCESS_READ = 1,
            ACCESS_WRITE = 2,
            ACCESS_READ_WRITE = 3,
            ACCESS_WRITE_DISCARD = 4,
            ACCESS_ELEMENT_ARRAY = 8,
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
