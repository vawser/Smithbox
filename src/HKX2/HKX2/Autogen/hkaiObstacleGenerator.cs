using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiObstacleGenerator : hkReferencedObject
    {
        public override uint Signature { get => 221806277; }
        
        public bool m_useSpheres;
        public bool m_useBoundaries;
        public bool m_clipBoundaries;
        public Matrix4x4 m_transform;
        public List<hkaiAvoidanceSolverSphereObstacle> m_spheres;
        public List<hkaiAvoidanceSolverBoundaryObstacle> m_boundaries;
        public ulong m_userData;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_useSpheres = br.ReadBoolean();
            m_useBoundaries = br.ReadBoolean();
            m_clipBoundaries = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadByte();
            m_transform = des.ReadTransform(br);
            m_spheres = des.ReadClassArray<hkaiAvoidanceSolverSphereObstacle>(br);
            m_boundaries = des.ReadClassArray<hkaiAvoidanceSolverBoundaryObstacle>(br);
            m_userData = br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteBoolean(m_useSpheres);
            bw.WriteBoolean(m_useBoundaries);
            bw.WriteBoolean(m_clipBoundaries);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteByte(0);
            s.WriteTransform(bw, m_transform);
            s.WriteClassArray<hkaiAvoidanceSolverSphereObstacle>(bw, m_spheres);
            s.WriteClassArray<hkaiAvoidanceSolverBoundaryObstacle>(bw, m_boundaries);
            bw.WriteUInt64(m_userData);
            bw.WriteUInt64(0);
        }
    }
}
