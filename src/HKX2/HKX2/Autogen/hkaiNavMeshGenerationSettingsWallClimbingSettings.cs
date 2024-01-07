using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiNavMeshGenerationSettingsWallClimbingSettings : IHavokObject
    {
        public virtual uint Signature { get => 367014563; }
        
        public bool m_enableWallClimbing;
        public bool m_excludeWalkableFaces;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_enableWallClimbing = br.ReadBoolean();
            m_excludeWalkableFaces = br.ReadBoolean();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteBoolean(m_enableWallClimbing);
            bw.WriteBoolean(m_excludeWalkableFaces);
        }
    }
}
