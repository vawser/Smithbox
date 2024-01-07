using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpCharacterProxyCinfo : hkpCharacterControllerCinfo
    {
        public override uint Signature { get => 3408002903; }
        
        public Vector4 m_position;
        public Vector4 m_velocity;
        public float m_dynamicFriction;
        public float m_staticFriction;
        public float m_keepContactTolerance;
        public Vector4 m_up;
        public float m_extraUpStaticFriction;
        public float m_extraDownStaticFriction;
        public hkpShapePhantom m_shapePhantom;
        public float m_keepDistance;
        public float m_contactAngleSensitivity;
        public uint m_userPlanes;
        public float m_maxCharacterSpeedForSolver;
        public float m_characterStrength;
        public float m_characterMass;
        public float m_maxSlope;
        public float m_penetrationRecoverySpeed;
        public int m_maxCastIterations;
        public bool m_refreshManifoldInCheckSupport;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_position = des.ReadVector4(br);
            m_velocity = des.ReadVector4(br);
            m_dynamicFriction = br.ReadSingle();
            m_staticFriction = br.ReadSingle();
            m_keepContactTolerance = br.ReadSingle();
            br.ReadUInt32();
            m_up = des.ReadVector4(br);
            m_extraUpStaticFriction = br.ReadSingle();
            m_extraDownStaticFriction = br.ReadSingle();
            m_shapePhantom = des.ReadClassPointer<hkpShapePhantom>(br);
            m_keepDistance = br.ReadSingle();
            m_contactAngleSensitivity = br.ReadSingle();
            m_userPlanes = br.ReadUInt32();
            m_maxCharacterSpeedForSolver = br.ReadSingle();
            m_characterStrength = br.ReadSingle();
            m_characterMass = br.ReadSingle();
            m_maxSlope = br.ReadSingle();
            m_penetrationRecoverySpeed = br.ReadSingle();
            m_maxCastIterations = br.ReadInt32();
            m_refreshManifoldInCheckSupport = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4(bw, m_position);
            s.WriteVector4(bw, m_velocity);
            bw.WriteSingle(m_dynamicFriction);
            bw.WriteSingle(m_staticFriction);
            bw.WriteSingle(m_keepContactTolerance);
            bw.WriteUInt32(0);
            s.WriteVector4(bw, m_up);
            bw.WriteSingle(m_extraUpStaticFriction);
            bw.WriteSingle(m_extraDownStaticFriction);
            s.WriteClassPointer<hkpShapePhantom>(bw, m_shapePhantom);
            bw.WriteSingle(m_keepDistance);
            bw.WriteSingle(m_contactAngleSensitivity);
            bw.WriteUInt32(m_userPlanes);
            bw.WriteSingle(m_maxCharacterSpeedForSolver);
            bw.WriteSingle(m_characterStrength);
            bw.WriteSingle(m_characterMass);
            bw.WriteSingle(m_maxSlope);
            bw.WriteSingle(m_penetrationRecoverySpeed);
            bw.WriteInt32(m_maxCastIterations);
            bw.WriteBoolean(m_refreshManifoldInCheckSupport);
            bw.WriteUInt64(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
