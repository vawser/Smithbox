using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiNavMeshGenerationSnapshot : IHavokObject
    {
        public virtual uint Signature { get => 1142877440; }
        
        public hkGeometry m_geometry;
        public hkaiNavMeshGenerationSettings m_settings;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_geometry = new hkGeometry();
            m_geometry.Read(des, br);
            m_settings = new hkaiNavMeshGenerationSettings();
            m_settings.Read(des, br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_geometry.Write(s, bw);
            m_settings.Write(s, bw);
        }
    }
}
