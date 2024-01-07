using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiOverlappingTrianglesSettings : IHavokObject
    {
        public virtual uint Signature { get => 863465528; }
        
        public float m_coplanarityTolerance;
        public float m_raycastLengthMultiplier;
        public WalkableTriangleSettings m_walkableTriangleSettings;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_coplanarityTolerance = br.ReadSingle();
            m_raycastLengthMultiplier = br.ReadSingle();
            m_walkableTriangleSettings = (WalkableTriangleSettings)br.ReadByte();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteSingle(m_coplanarityTolerance);
            bw.WriteSingle(m_raycastLengthMultiplier);
            bw.WriteByte((byte)m_walkableTriangleSettings);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
