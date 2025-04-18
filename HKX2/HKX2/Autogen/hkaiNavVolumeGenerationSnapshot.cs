using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiNavVolumeGenerationSnapshot : IHavokObject
    {
        public virtual uint Signature { get => 3980025567; }
        
        public hkGeometry m_geometry;
        public hkaiNavVolumeGenerationSettings m_settings;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_geometry = new hkGeometry();
            m_geometry.Read(des, br);
            m_settings = new hkaiNavVolumeGenerationSettings();
            m_settings.Read(des, br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_geometry.Write(s, bw);
            m_settings.Write(s, bw);
        }
    }
}
