using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum ShapeTypeEnum
    {
        SPHERE = 0,
        CYLINDER = 1,
        TRIANGLE = 2,
        BOX = 3,
        CAPSULE = 4,
        CONVEX_VERTICES = 5,
        TRI_SAMPLED_HEIGHT_FIELD_COLLECTION = 6,
        TRI_SAMPLED_HEIGHT_FIELD_BV_TREE = 7,
        LIST = 8,
        MOPP = 9,
        CONVEX_TRANSLATE = 10,
        CONVEX_TRANSFORM = 11,
        SAMPLED_HEIGHT_FIELD = 12,
        EXTENDED_MESH = 13,
        TRANSFORM = 14,
        COMPRESSED_MESH = 15,
        STATIC_COMPOUND = 16,
        BV_COMPRESSED_MESH = 17,
        COLLECTION = 18,
        USER0 = 19,
        USER1 = 20,
        USER2 = 21,
        BV_TREE = 22,
        CONVEX = 23,
        CONVEX_PIECE = 24,
        MULTI_SPHERE = 25,
        CONVEX_LIST = 26,
        TRIANGLE_COLLECTION = 27,
        HEIGHT_FIELD = 28,
        SPHERE_REP = 29,
        BV = 30,
        PLANE = 31,
        PHANTOM_CALLBACK = 32,
        MULTI_RAY = 33,
        INVALID = 34,
        FIRST_SHAPE_TYPE = 0,
        MAX_SPU_SHAPE_TYPE = 22,
        MAX_PPU_SHAPE_TYPE = 35,
        ALL_SHAPE_TYPES = -1,
    }
    
    public partial class hkcdShapeType : IHavokObject
    {
        public virtual uint Signature { get => 1947441453; }
        
        
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
