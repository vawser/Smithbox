using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclSimClothSetupObject : hkReferencedObject
    {
        public override uint Signature { get => 74754597; }
        
        public string m_name;
        public hclSimulationSetupMesh m_simulationMesh;
        public hclTransformSetSetupObject m_collidableTransformSet;
        public Vector4 m_gravity;
        public float m_globalDampingPerSecond;
        public bool m_doNormals;
        public bool m_specifyDensity;
        public hclVertexFloatInput m_vertexDensity;
        public bool m_rescaleMass;
        public float m_totalMass;
        public hclVertexFloatInput m_particleMass;
        public hclVertexFloatInput m_particleRadius;
        public hclVertexFloatInput m_particleFriction;
        public hclVertexSelectionInput m_fixedParticles;
        public bool m_enablePinchDetection;
        public hclVertexSelectionInput m_pinchDetectionEnabledParticles;
        public float m_toAnimPeriod;
        public float m_toSimPeriod;
        public bool m_drivePinchedParticlesToReferenceMesh;
        public hclBufferSetupObject m_pinchReferenceBufferSetup;
        public float m_collisionTolerance;
        public hclVertexSelectionInput m_landscapeCollisionParticleSelection;
        public float m_landscapeCollisionParticleRadius;
        public bool m_enableStuckParticleDetection;
        public float m_stuckParticlesStretchFactor;
        public bool m_enableLandscapePinchDetection;
        public sbyte m_landscapePinchDetectionPriority;
        public float m_landscapePinchDetectionRadius;
        public bool m_enableTransferMotion;
        public hclSimClothSetupObjectTransferMotionSetupData m_transferMotionSetupData;
        public List<hclConstraintSetSetupObject> m_constraintSetSetups;
        public List<hclSimClothSetupObjectPerInstanceCollidable> m_perInstanceCollidables;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_name = des.ReadStringPointer(br);
            m_simulationMesh = des.ReadClassPointer<hclSimulationSetupMesh>(br);
            m_collidableTransformSet = des.ReadClassPointer<hclTransformSetSetupObject>(br);
            br.ReadUInt64();
            m_gravity = des.ReadVector4(br);
            m_globalDampingPerSecond = br.ReadSingle();
            m_doNormals = br.ReadBoolean();
            m_specifyDensity = br.ReadBoolean();
            br.ReadUInt16();
            m_vertexDensity = new hclVertexFloatInput();
            m_vertexDensity.Read(des, br);
            m_rescaleMass = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_totalMass = br.ReadSingle();
            m_particleMass = new hclVertexFloatInput();
            m_particleMass.Read(des, br);
            m_particleRadius = new hclVertexFloatInput();
            m_particleRadius.Read(des, br);
            m_particleFriction = new hclVertexFloatInput();
            m_particleFriction.Read(des, br);
            m_fixedParticles = new hclVertexSelectionInput();
            m_fixedParticles.Read(des, br);
            m_enablePinchDetection = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
            m_pinchDetectionEnabledParticles = new hclVertexSelectionInput();
            m_pinchDetectionEnabledParticles.Read(des, br);
            m_toAnimPeriod = br.ReadSingle();
            m_toSimPeriod = br.ReadSingle();
            m_drivePinchedParticlesToReferenceMesh = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
            m_pinchReferenceBufferSetup = des.ReadClassPointer<hclBufferSetupObject>(br);
            m_collisionTolerance = br.ReadSingle();
            br.ReadUInt32();
            m_landscapeCollisionParticleSelection = new hclVertexSelectionInput();
            m_landscapeCollisionParticleSelection.Read(des, br);
            m_landscapeCollisionParticleRadius = br.ReadSingle();
            m_enableStuckParticleDetection = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_stuckParticlesStretchFactor = br.ReadSingle();
            m_enableLandscapePinchDetection = br.ReadBoolean();
            m_landscapePinchDetectionPriority = br.ReadSByte();
            br.ReadUInt16();
            m_landscapePinchDetectionRadius = br.ReadSingle();
            m_enableTransferMotion = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_transferMotionSetupData = new hclSimClothSetupObjectTransferMotionSetupData();
            m_transferMotionSetupData.Read(des, br);
            m_constraintSetSetups = des.ReadClassPointerArray<hclConstraintSetSetupObject>(br);
            m_perInstanceCollidables = des.ReadClassArray<hclSimClothSetupObjectPerInstanceCollidable>(br);
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_name);
            s.WriteClassPointer<hclSimulationSetupMesh>(bw, m_simulationMesh);
            s.WriteClassPointer<hclTransformSetSetupObject>(bw, m_collidableTransformSet);
            bw.WriteUInt64(0);
            s.WriteVector4(bw, m_gravity);
            bw.WriteSingle(m_globalDampingPerSecond);
            bw.WriteBoolean(m_doNormals);
            bw.WriteBoolean(m_specifyDensity);
            bw.WriteUInt16(0);
            m_vertexDensity.Write(s, bw);
            bw.WriteBoolean(m_rescaleMass);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteSingle(m_totalMass);
            m_particleMass.Write(s, bw);
            m_particleRadius.Write(s, bw);
            m_particleFriction.Write(s, bw);
            m_fixedParticles.Write(s, bw);
            bw.WriteBoolean(m_enablePinchDetection);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            m_pinchDetectionEnabledParticles.Write(s, bw);
            bw.WriteSingle(m_toAnimPeriod);
            bw.WriteSingle(m_toSimPeriod);
            bw.WriteBoolean(m_drivePinchedParticlesToReferenceMesh);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            s.WriteClassPointer<hclBufferSetupObject>(bw, m_pinchReferenceBufferSetup);
            bw.WriteSingle(m_collisionTolerance);
            bw.WriteUInt32(0);
            m_landscapeCollisionParticleSelection.Write(s, bw);
            bw.WriteSingle(m_landscapeCollisionParticleRadius);
            bw.WriteBoolean(m_enableStuckParticleDetection);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteSingle(m_stuckParticlesStretchFactor);
            bw.WriteBoolean(m_enableLandscapePinchDetection);
            bw.WriteSByte(m_landscapePinchDetectionPriority);
            bw.WriteUInt16(0);
            bw.WriteSingle(m_landscapePinchDetectionRadius);
            bw.WriteBoolean(m_enableTransferMotion);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            m_transferMotionSetupData.Write(s, bw);
            s.WriteClassPointerArray<hclConstraintSetSetupObject>(bw, m_constraintSetSetups);
            s.WriteClassArray<hclSimClothSetupObjectPerInstanceCollidable>(bw, m_perInstanceCollidables);
            bw.WriteUInt64(0);
        }
    }
}
