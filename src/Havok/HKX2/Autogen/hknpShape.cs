using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum ScaleMode
    {
        SCALE_SURFACE = 0,
        SCALE_VERTICES = 1,
    }
    
    public enum ConvexRadiusDisplayMode
    {
        CONVEX_RADIUS_DISPLAY_NONE = 0,
        CONVEX_RADIUS_DISPLAY_PLANAR = 1,
        CONVEX_RADIUS_DISPLAY_ROUNDED = 2,
    }
    
    public partial class hknpShape : hkReferencedObject
    {
        public override uint Signature { get => 3679627963; }
        
        public enum FlagsEnum
        {
            IS_CONVEX_SHAPE = 1,
            IS_CONVEX_POLYTOPE_SHAPE = 2,
            IS_COMPOSITE_SHAPE = 4,
            IS_HEIGHT_FIELD_SHAPE = 8,
            USE_SINGLE_POINT_MANIFOLD = 16,
            IS_INTERIOR_TRIANGLE = 32,
            SUPPORTS_COLLISIONS_WITH_INTERIOR_TRIANGLES = 64,
            USE_NORMAL_TO_FIND_SUPPORT_PLANE = 128,
            USE_SMALL_FACE_INDICES = 256,
            NO_GET_SHAPE_KEYS_ON_SPU = 512,
            SHAPE_NOT_SUPPORTED_ON_SPU = 1024,
            IS_TRIANGLE_OR_QUAD_SHAPE = 2048,
            IS_QUAD_SHAPE = 4096,
        }
        
        public ushort m_flags;
        public byte m_numShapeKeyBits;
        public Enum m_dispatchType;
        public float m_convexRadius;
        public ulong m_userData;
        public hkRefCountedProperties m_properties;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_flags = br.ReadUInt16();
            m_numShapeKeyBits = br.ReadByte();
            m_dispatchType = (Enum)br.ReadByte();
            m_convexRadius = br.ReadSingle();
            m_userData = br.ReadUInt64();
            m_properties = des.ReadClassPointer<hkRefCountedProperties>(br);
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt16(m_flags);
            bw.WriteByte(m_numShapeKeyBits);
            bw.WriteByte((byte)m_dispatchType);
            bw.WriteSingle(m_convexRadius);
            bw.WriteUInt64(m_userData);
            s.WriteClassPointer<hkRefCountedProperties>(bw, m_properties);
            bw.WriteUInt64(0);
        }
    }
}
