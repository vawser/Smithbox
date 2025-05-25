using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hknpBodyQuality : IHavokObject
    {
        public virtual uint Signature { get => 1292877147; }
        
        public enum FlagsEnum
        {
            DROP_MANIFOLD_CACHE = 1,
            FORCE_GSK_EXECUTION = 2,
            FORCE_GSK_SINGLE_POINT_MANIFOLD = 4,
            ALLOW_INTERIOR_TRIANGLE_COLLISIONS = 8,
            ENABLE_SILHOUETTE_MANIFOLDS = 16,
            USE_HIGHER_QUALITY_CONTACT_SOLVING = 64,
            ENABLE_NEIGHBOR_WELDING = 128,
            ENABLE_MOTION_WELDING = 256,
            ENABLE_TRIANGLE_WELDING = 512,
            ANY_WELDING = 896,
            ENABLE_CONTACT_CACHING = 1024,
            REQUEST_LINEAR_ONLY_COLLISION_LOOK_AHEAD = 2048,
            FORCE_LINEAR_ONLY_COLLISION_LOOK_AHEAD = 4096,
            ENABLE_FRICTION_ESTIMATION = 8192,
            FIRST_NON_CACHABLE_FLAG = 65536,
            CLIP_ANGULAR_VELOCITY = 65536,
            USE_DISCRETE_AABB_EXPANSION = 131072,
            MERGE_FRICTION_JACOBIANS = 262144,
        }
        
        public int m_priority;
        public uint m_supportedFlags;
        public uint m_requestedFlags;
        public float m_contactCachingRelativeMovementThreshold;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_priority = br.ReadInt32();
            m_supportedFlags = br.ReadUInt32();
            m_requestedFlags = br.ReadUInt32();
            m_contactCachingRelativeMovementThreshold = br.ReadSingle();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt32(m_priority);
            bw.WriteUInt32(m_supportedFlags);
            bw.WriteUInt32(m_requestedFlags);
            bw.WriteSingle(m_contactCachingRelativeMovementThreshold);
        }
    }
}
