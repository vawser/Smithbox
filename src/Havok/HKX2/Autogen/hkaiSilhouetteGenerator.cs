using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum GeneratorType
    {
        GENERATOR_PHYSICS_2012_BODY = 0,
        GENERATOR_POINT_CLOUD = 1,
        GENERATOR_STORAGE_POINT_CLOUD_REMOVED = 2,
        GENERATOR_PHYSICS_BODY_BASE = 3,
        GENERATOR_PHYSICS_BODY = 4,
        GENERATOR_USER_0 = 5,
        GENERATOR_USER_1 = 6,
        GENERATOR_USER_2 = 7,
        GENERATOR_USER_3 = 8,
        GENERATOR_MAX = 9,
    }
    
    public enum ForceGenerateOntoPpuReasons
    {
        FORCE_PPU_USER_REQUEST = 8,
        FORCE_PPU_SILHOUETTE_COMPLEXITY_REQUEST = 16,
    }
    
    public enum ExpansionFlags
    {
        AABB_EXPAND_NONE = 0,
        AABB_EXTRUDE_DOWN = 1,
        AABB_EXPAND_PLANAR = 2,
        AABB_EXPAND_UNIFORM = 4,
    }
    
    public partial class hkaiSilhouetteGenerator : hkReferencedObject
    {
        public override uint Signature { get => 191222524; }
        
        public ulong m_userData;
        public float m_lazyRecomputeDisplacementThreshold;
        public GeneratorType m_type;
        public byte m_forceGenerateOntoPpu;
        public int m_materialId;
        public hkaiConvexSilhouetteSet m_cachedSilhouettes;
        public hkQTransform m_transform;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_userData = br.ReadUInt64();
            m_lazyRecomputeDisplacementThreshold = br.ReadSingle();
            m_type = (GeneratorType)br.ReadByte();
            m_forceGenerateOntoPpu = br.ReadByte();
            br.ReadUInt16();
            m_materialId = br.ReadInt32();
            br.ReadUInt32();
            m_cachedSilhouettes = des.ReadClassPointer<hkaiConvexSilhouetteSet>(br);
            m_transform = new hkQTransform();
            m_transform.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(m_userData);
            bw.WriteSingle(m_lazyRecomputeDisplacementThreshold);
            bw.WriteByte((byte)m_type);
            bw.WriteByte(m_forceGenerateOntoPpu);
            bw.WriteUInt16(0);
            bw.WriteInt32(m_materialId);
            bw.WriteUInt32(0);
            s.WriteClassPointer<hkaiConvexSilhouetteSet>(bw, m_cachedSilhouettes);
            m_transform.Write(s, bw);
        }
    }
}
