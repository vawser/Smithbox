using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum NearbyBoundariesSearchType
    {
        SEARCH_NEIGHBORS = 0,
        SEARCH_FLOOD_FILL = 1,
    }
    
    public partial class hkaiAvoidanceProperties : hkReferencedObject
    {
        public override uint Signature { get => 2761548401; }
        
        public hkaiMovementProperties m_movementProperties;
        public NearbyBoundariesSearchType m_nearbyBoundariesSearchType;
        public hkAabb m_localSensorAabb;
        public float m_wallFollowingAngle;
        public float m_dodgingPenalty;
        public float m_velocityHysteresis;
        public float m_sidednessChangingPenalty;
        public float m_collisionPenalty;
        public float m_penetrationPenalty;
        public int m_maxNeighbors;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_movementProperties = new hkaiMovementProperties();
            m_movementProperties.Read(des, br);
            m_nearbyBoundariesSearchType = (NearbyBoundariesSearchType)br.ReadByte();
            br.ReadUInt64();
            br.ReadUInt16();
            br.ReadByte();
            m_localSensorAabb = new hkAabb();
            m_localSensorAabb.Read(des, br);
            m_wallFollowingAngle = br.ReadSingle();
            m_dodgingPenalty = br.ReadSingle();
            m_velocityHysteresis = br.ReadSingle();
            m_sidednessChangingPenalty = br.ReadSingle();
            m_collisionPenalty = br.ReadSingle();
            m_penetrationPenalty = br.ReadSingle();
            m_maxNeighbors = br.ReadInt32();
            br.ReadUInt32();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_movementProperties.Write(s, bw);
            bw.WriteByte((byte)m_nearbyBoundariesSearchType);
            bw.WriteUInt64(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            m_localSensorAabb.Write(s, bw);
            bw.WriteSingle(m_wallFollowingAngle);
            bw.WriteSingle(m_dodgingPenalty);
            bw.WriteSingle(m_velocityHysteresis);
            bw.WriteSingle(m_sidednessChangingPenalty);
            bw.WriteSingle(m_collisionPenalty);
            bw.WriteSingle(m_penetrationPenalty);
            bw.WriteInt32(m_maxNeighbors);
            bw.WriteUInt32(0);
        }
    }
}
