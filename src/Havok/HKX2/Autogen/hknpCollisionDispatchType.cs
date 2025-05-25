using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum Enum
    {
        NONE = 0,
        CONVEX = 1,
        COMPOSITE = 2,
        DISTANCE_FIELD = 3,
        USER = 4,
        NUM_TYPES = 5,
    }
    
    public partial class hknpCollisionDispatchType : IHavokObject
    {
        public virtual uint Signature { get => 817970547; }
        
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteByte(0);
        }
    }
}
