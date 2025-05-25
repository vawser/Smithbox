using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclSimClothDataOverridableSimulationInfo : IHavokObject
    {
        public virtual uint Signature { get => 943429711; }
        
        public Vector4 m_gravity;
        public float m_globalDampingPerSecond;
        public float m_collisionTolerance;
        public uint m_subSteps;
        public bool m_pinchDetectionEnabled;
        public bool m_landscapeCollisionEnabled;
        public bool m_transferMotionEnabled;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_gravity = des.ReadVector4(br);
            m_globalDampingPerSecond = br.ReadSingle();
            m_collisionTolerance = br.ReadSingle();
            m_subSteps = br.ReadUInt32();
            m_pinchDetectionEnabled = br.ReadBoolean();
            m_landscapeCollisionEnabled = br.ReadBoolean();
            m_transferMotionEnabled = br.ReadBoolean();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteVector4(bw, m_gravity);
            bw.WriteSingle(m_globalDampingPerSecond);
            bw.WriteSingle(m_collisionTolerance);
            bw.WriteUInt32(m_subSteps);
            bw.WriteBoolean(m_pinchDetectionEnabled);
            bw.WriteBoolean(m_landscapeCollisionEnabled);
            bw.WriteBoolean(m_transferMotionEnabled);
            bw.WriteByte(0);
        }
    }
}
