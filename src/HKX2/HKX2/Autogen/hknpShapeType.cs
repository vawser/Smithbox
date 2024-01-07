using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpShapeType : IHavokObject
    {
        public virtual uint Signature { get => 926589731; }
        
        public enum Enum
        {
            CONVEX = 0,
            CONVEX_POLYTOPE = 1,
            SPHERE = 2,
            CAPSULE = 3,
            TRIANGLE = 4,
            COMPRESSED_MESH = 5,
            EXTERN_MESH = 6,
            STATIC_COMPOUND = 7,
            DYNAMIC_COMPOUND = 8,
            HEIGHT_FIELD = 9,
            COMPRESSED_HEIGHT_FIELD = 10,
            SCALED_CONVEX = 11,
            MASKED = 12,
            MASKED_COMPOUND = 13,
            LOD = 14,
            DUMMY = 15,
            USER_0 = 16,
            USER_1 = 17,
            USER_2 = 18,
            USER_3 = 19,
            NUM_SHAPE_TYPES = 20,
            INVALID = 21,
        }
        
        
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
