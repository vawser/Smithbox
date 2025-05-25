using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclSimClothSetupObjectPerInstanceCollidable : IHavokObject
    {
        public virtual uint Signature { get => 361781863; }
        
        public hclCollidable m_collidable;
        public hclVertexSelectionInput m_collidingParticles;
        public string m_drivingBoneName;
        public bool m_pinchDetectionEnabled;
        public sbyte m_pinchDetectionPriority;
        public float m_pinchDetectionRadius;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_collidable = des.ReadClassPointer<hclCollidable>(br);
            m_collidingParticles = new hclVertexSelectionInput();
            m_collidingParticles.Read(des, br);
            m_drivingBoneName = des.ReadStringPointer(br);
            m_pinchDetectionEnabled = br.ReadBoolean();
            m_pinchDetectionPriority = br.ReadSByte();
            br.ReadUInt16();
            m_pinchDetectionRadius = br.ReadSingle();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassPointer<hclCollidable>(bw, m_collidable);
            m_collidingParticles.Write(s, bw);
            s.WriteStringPointer(bw, m_drivingBoneName);
            bw.WriteBoolean(m_pinchDetectionEnabled);
            bw.WriteSByte(m_pinchDetectionPriority);
            bw.WriteUInt16(0);
            bw.WriteSingle(m_pinchDetectionRadius);
        }
    }
}
