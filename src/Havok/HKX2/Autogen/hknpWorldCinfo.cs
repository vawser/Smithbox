using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum LeavingBroadPhaseBehavior
    {
        ON_LEAVING_BROAD_PHASE_DO_NOTHING = 0,
        ON_LEAVING_BROAD_PHASE_REMOVE_BODY = 1,
        ON_LEAVING_BROAD_PHASE_FREEZE_BODY = 2,
    }
    
    public partial class hknpWorldCinfo : IHavokObject
    {
        public virtual uint Signature { get => 1660997442; }
        
        public enum SimulationType
        {
            SIMULATION_TYPE_SINGLE_THREADED = 0,
            SIMULATION_TYPE_MULTI_THREADED = 1,
        }
        
        public enum SolverType
        {
            SOLVER_TYPE_INVALID = 0,
            SOLVER_TYPE_2ITERS_SOFT = 1,
            SOLVER_TYPE_2ITERS_MEDIUM = 2,
            SOLVER_TYPE_2ITERS_HARD = 3,
            SOLVER_TYPE_4ITERS_SOFT = 4,
            SOLVER_TYPE_4ITERS_MEDIUM = 5,
            SOLVER_TYPE_4ITERS_HARD = 6,
            SOLVER_TYPE_8ITERS_SOFT = 7,
            SOLVER_TYPE_8ITERS_MEDIUM = 8,
            SOLVER_TYPE_8ITERS_HARD = 9,
            SOLVER_TYPE_MAX = 10,
        }
        
        public int m_bodyBufferCapacity;
        public int m_motionBufferCapacity;
        public int m_constraintBufferCapacity;
        public hknpMaterialLibrary m_materialLibrary;
        public hknpMotionPropertiesLibrary m_motionPropertiesLibrary;
        public hknpBodyQualityLibrary m_qualityLibrary;
        public SimulationType m_simulationType;
        public int m_numSplitterCells;
        public Vector4 m_gravity;
        public bool m_enableContactCaching;
        public bool m_mergeEventsBeforeDispatch;
        public LeavingBroadPhaseBehavior m_leavingBroadPhaseBehavior;
        public hkAabb m_broadPhaseAabb;
        public hknpBroadPhaseConfig m_broadPhaseConfig;
        public hknpCollisionFilter m_collisionFilter;
        public hknpShapeTagCodec m_shapeTagCodec;
        public float m_collisionTolerance;
        public float m_relativeCollisionAccuracy;
        public bool m_enableWeldingForDefaultObjects;
        public bool m_enableWeldingForCriticalObjects;
        public float m_solverTau;
        public float m_solverDamp;
        public int m_solverIterations;
        public int m_solverMicrosteps;
        public float m_defaultSolverTimestep;
        public float m_maxApproachSpeedForHighQualitySolver;
        public bool m_enableDeactivation;
        public bool m_deleteCachesOnDeactivation;
        public int m_largeIslandSize;
        public bool m_enableSolverDynamicScheduling;
        public int m_contactSolverType;
        public float m_unitScale;
        public bool m_applyUnitScaleToStaticConstants;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_bodyBufferCapacity = br.ReadInt32();
            br.ReadUInt64();
            br.ReadUInt32();
            m_motionBufferCapacity = br.ReadInt32();
            br.ReadUInt64();
            br.ReadUInt32();
            m_constraintBufferCapacity = br.ReadInt32();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt32();
            m_materialLibrary = des.ReadClassPointer<hknpMaterialLibrary>(br);
            m_motionPropertiesLibrary = des.ReadClassPointer<hknpMotionPropertiesLibrary>(br);
            m_qualityLibrary = des.ReadClassPointer<hknpBodyQualityLibrary>(br);
            m_simulationType = (SimulationType)br.ReadByte();
            br.ReadUInt16();
            br.ReadByte();
            m_numSplitterCells = br.ReadInt32();
            br.ReadUInt64();
            m_gravity = des.ReadVector4(br);
            m_enableContactCaching = br.ReadBoolean();
            m_mergeEventsBeforeDispatch = br.ReadBoolean();
            m_leavingBroadPhaseBehavior = (LeavingBroadPhaseBehavior)br.ReadByte();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadByte();
            m_broadPhaseAabb = new hkAabb();
            m_broadPhaseAabb.Read(des, br);
            m_broadPhaseConfig = des.ReadClassPointer<hknpBroadPhaseConfig>(br);
            m_collisionFilter = des.ReadClassPointer<hknpCollisionFilter>(br);
            m_shapeTagCodec = des.ReadClassPointer<hknpShapeTagCodec>(br);
            m_collisionTolerance = br.ReadSingle();
            m_relativeCollisionAccuracy = br.ReadSingle();
            m_enableWeldingForDefaultObjects = br.ReadBoolean();
            m_enableWeldingForCriticalObjects = br.ReadBoolean();
            br.ReadUInt16();
            m_solverTau = br.ReadSingle();
            m_solverDamp = br.ReadSingle();
            m_solverIterations = br.ReadInt32();
            m_solverMicrosteps = br.ReadInt32();
            m_defaultSolverTimestep = br.ReadSingle();
            m_maxApproachSpeedForHighQualitySolver = br.ReadSingle();
            m_enableDeactivation = br.ReadBoolean();
            m_deleteCachesOnDeactivation = br.ReadBoolean();
            br.ReadUInt16();
            m_largeIslandSize = br.ReadInt32();
            m_enableSolverDynamicScheduling = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_contactSolverType = br.ReadInt32();
            m_unitScale = br.ReadSingle();
            m_applyUnitScaleToStaticConstants = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteInt32(m_bodyBufferCapacity);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteInt32(m_motionBufferCapacity);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteInt32(m_constraintBufferCapacity);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            s.WriteClassPointer<hknpMaterialLibrary>(bw, m_materialLibrary);
            s.WriteClassPointer<hknpMotionPropertiesLibrary>(bw, m_motionPropertiesLibrary);
            s.WriteClassPointer<hknpBodyQualityLibrary>(bw, m_qualityLibrary);
            bw.WriteByte((byte)m_simulationType);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteInt32(m_numSplitterCells);
            bw.WriteUInt64(0);
            s.WriteVector4(bw, m_gravity);
            bw.WriteBoolean(m_enableContactCaching);
            bw.WriteBoolean(m_mergeEventsBeforeDispatch);
            bw.WriteByte((byte)m_leavingBroadPhaseBehavior);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteByte(0);
            m_broadPhaseAabb.Write(s, bw);
            s.WriteClassPointer<hknpBroadPhaseConfig>(bw, m_broadPhaseConfig);
            s.WriteClassPointer<hknpCollisionFilter>(bw, m_collisionFilter);
            s.WriteClassPointer<hknpShapeTagCodec>(bw, m_shapeTagCodec);
            bw.WriteSingle(m_collisionTolerance);
            bw.WriteSingle(m_relativeCollisionAccuracy);
            bw.WriteBoolean(m_enableWeldingForDefaultObjects);
            bw.WriteBoolean(m_enableWeldingForCriticalObjects);
            bw.WriteUInt16(0);
            bw.WriteSingle(m_solverTau);
            bw.WriteSingle(m_solverDamp);
            bw.WriteInt32(m_solverIterations);
            bw.WriteInt32(m_solverMicrosteps);
            bw.WriteSingle(m_defaultSolverTimestep);
            bw.WriteSingle(m_maxApproachSpeedForHighQualitySolver);
            bw.WriteBoolean(m_enableDeactivation);
            bw.WriteBoolean(m_deleteCachesOnDeactivation);
            bw.WriteUInt16(0);
            bw.WriteInt32(m_largeIslandSize);
            bw.WriteBoolean(m_enableSolverDynamicScheduling);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteInt32(m_contactSolverType);
            bw.WriteSingle(m_unitScale);
            bw.WriteBoolean(m_applyUnitScaleToStaticConstants);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
