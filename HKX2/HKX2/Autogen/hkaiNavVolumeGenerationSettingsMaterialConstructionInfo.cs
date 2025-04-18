using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiNavVolumeGenerationSettingsMaterialConstructionInfo : IHavokObject
    {
        public virtual uint Signature { get => 1286768366; }
        
        public int m_materialIndex;
        public uint m_flags;
        public int m_resolution;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_materialIndex = br.ReadInt32();
            m_flags = br.ReadUInt32();
            m_resolution = br.ReadInt32();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt32(m_materialIndex);
            bw.WriteUInt32(m_flags);
            bw.WriteInt32(m_resolution);
        }
    }
}
