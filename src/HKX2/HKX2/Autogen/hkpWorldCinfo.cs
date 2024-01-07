using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum ContactPointGeneration
    {
        CONTACT_POINT_ACCEPT_ALWAYS = 0,
        CONTACT_POINT_REJECT_DUBIOUS = 1,
        CONTACT_POINT_REJECT_MANY = 2,
    }
    
    public enum BroadPhaseBorderBehaviour
    {
        BROADPHASE_BORDER_ASSERT = 0,
        BROADPHASE_BORDER_FIX_ENTITY = 1,
        BROADPHASE_BORDER_REMOVE_ENTITY = 2,
        BROADPHASE_BORDER_DO_NOTHING = 3,
    }
    
    public partial class hkpWorldCinfo : hkReferencedObject
    {
        public override uint Signature { get => 1542043532; }
        
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
            SOLVER_TYPE_MAX_ID = 10,
        }
        
        public enum SimulationType
        {
            SIMULATION_TYPE_INVALID = 0,
            SIMULATION_TYPE_DISCRETE = 1,
            SIMULATION_TYPE_CONTINUOUS = 2,
            SIMULATION_TYPE_MULTITHREADED = 3,
        }
        
        public enum BroadPhaseType
        {
            BROADPHASE_TYPE_SAP = 0,
            BROADPHASE_TYPE_TREE = 1,
            BROADPHASE_TYPE_HYBRID = 2,
        }
        
        public Vector4 m_gravity;
        public int m_broadPhaseQuerySize;
        public float m_contactRestingVelocity;
        public BroadPhaseType m_broadPhaseType;
        public BroadPhaseBorderBehaviour m_broadPhaseBorderBehaviour;
        public bool m_mtPostponeAndSortBroadPhaseBorderCallbacks;
        public hkAabb m_broadPhaseWorldAabb;
        public float m_collisionTolerance;
        public hkpCollisionFilter m_collisionFilter;
        public hkpConvexListFilter m_convexListFilter;
        public float m_expectedMaxLinearVelocity;
        public int m_sizeOfToiEventQueue;
        public float m_expectedMinPsiDeltaTime;
        public hkWorldMemoryAvailableWatchDog m_memoryWatchDog;
        public int m_broadPhaseNumMarkers;
        public ContactPointGeneration m_contactPointGeneration;
        public bool m_allowToSkipConfirmedCallbacks;
        public float m_solverTau;
        public float m_solverDamp;
        public int m_solverIterations;
        public int m_solverMicrosteps;
        public float m_maxConstraintViolation;
        public bool m_forceCoherentConstraintOrderingInSolver;
        public float m_snapCollisionToConvexEdgeThreshold;
        public float m_snapCollisionToConcaveEdgeThreshold;
        public bool m_enableToiWeldRejection;
        public bool m_enableDeprecatedWelding;
        public float m_iterativeLinearCastEarlyOutDistance;
        public int m_iterativeLinearCastMaxIterations;
        public byte m_deactivationNumInactiveFramesSelectFlag0;
        public byte m_deactivationNumInactiveFramesSelectFlag1;
        public byte m_deactivationIntegrateCounter;
        public bool m_shouldActivateOnRigidBodyTransformChange;
        public float m_deactivationReferenceDistance;
        public float m_toiCollisionResponseRotateNormal;
        public bool m_useCompoundSpuElf;
        public int m_maxSectorsPerMidphaseCollideTask;
        public int m_maxSectorsPerNarrowphaseCollideTask;
        public bool m_processToisMultithreaded;
        public int m_maxEntriesPerToiMidphaseCollideTask;
        public int m_maxEntriesPerToiNarrowphaseCollideTask;
        public int m_maxNumToiCollisionPairsSinglethreaded;
        public float m_numToisTillAllowedPenetrationSimplifiedToi;
        public float m_numToisTillAllowedPenetrationToi;
        public float m_numToisTillAllowedPenetrationToiHigher;
        public float m_numToisTillAllowedPenetrationToiForced;
        public bool m_enableDeactivation;
        public SimulationType m_simulationType;
        public bool m_enableSimulationIslands;
        public uint m_minDesiredIslandSize;
        public bool m_processActionsInSingleThread;
        public bool m_allowIntegrationOfIslandsWithoutConstraintsInASeparateJob;
        public float m_frameMarkerPsiSnap;
        public bool m_fireCollisionCallbacks;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_gravity = des.ReadVector4(br);
            m_broadPhaseQuerySize = br.ReadInt32();
            m_contactRestingVelocity = br.ReadSingle();
            m_broadPhaseType = (BroadPhaseType)br.ReadSByte();
            m_broadPhaseBorderBehaviour = (BroadPhaseBorderBehaviour)br.ReadSByte();
            m_mtPostponeAndSortBroadPhaseBorderCallbacks = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadByte();
            m_broadPhaseWorldAabb = new hkAabb();
            m_broadPhaseWorldAabb.Read(des, br);
            m_collisionTolerance = br.ReadSingle();
            br.ReadUInt32();
            m_collisionFilter = des.ReadClassPointer<hkpCollisionFilter>(br);
            m_convexListFilter = des.ReadClassPointer<hkpConvexListFilter>(br);
            m_expectedMaxLinearVelocity = br.ReadSingle();
            m_sizeOfToiEventQueue = br.ReadInt32();
            m_expectedMinPsiDeltaTime = br.ReadSingle();
            br.ReadUInt32();
            m_memoryWatchDog = des.ReadClassPointer<hkWorldMemoryAvailableWatchDog>(br);
            m_broadPhaseNumMarkers = br.ReadInt32();
            m_contactPointGeneration = (ContactPointGeneration)br.ReadSByte();
            m_allowToSkipConfirmedCallbacks = br.ReadBoolean();
            br.ReadUInt16();
            m_solverTau = br.ReadSingle();
            m_solverDamp = br.ReadSingle();
            m_solverIterations = br.ReadInt32();
            m_solverMicrosteps = br.ReadInt32();
            m_maxConstraintViolation = br.ReadSingle();
            m_forceCoherentConstraintOrderingInSolver = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_snapCollisionToConvexEdgeThreshold = br.ReadSingle();
            m_snapCollisionToConcaveEdgeThreshold = br.ReadSingle();
            m_enableToiWeldRejection = br.ReadBoolean();
            m_enableDeprecatedWelding = br.ReadBoolean();
            br.ReadUInt16();
            m_iterativeLinearCastEarlyOutDistance = br.ReadSingle();
            m_iterativeLinearCastMaxIterations = br.ReadInt32();
            m_deactivationNumInactiveFramesSelectFlag0 = br.ReadByte();
            m_deactivationNumInactiveFramesSelectFlag1 = br.ReadByte();
            m_deactivationIntegrateCounter = br.ReadByte();
            m_shouldActivateOnRigidBodyTransformChange = br.ReadBoolean();
            m_deactivationReferenceDistance = br.ReadSingle();
            m_toiCollisionResponseRotateNormal = br.ReadSingle();
            m_useCompoundSpuElf = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_maxSectorsPerMidphaseCollideTask = br.ReadInt32();
            m_maxSectorsPerNarrowphaseCollideTask = br.ReadInt32();
            m_processToisMultithreaded = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_maxEntriesPerToiMidphaseCollideTask = br.ReadInt32();
            m_maxEntriesPerToiNarrowphaseCollideTask = br.ReadInt32();
            m_maxNumToiCollisionPairsSinglethreaded = br.ReadInt32();
            m_numToisTillAllowedPenetrationSimplifiedToi = br.ReadSingle();
            m_numToisTillAllowedPenetrationToi = br.ReadSingle();
            m_numToisTillAllowedPenetrationToiHigher = br.ReadSingle();
            m_numToisTillAllowedPenetrationToiForced = br.ReadSingle();
            m_enableDeactivation = br.ReadBoolean();
            m_simulationType = (SimulationType)br.ReadSByte();
            m_enableSimulationIslands = br.ReadBoolean();
            br.ReadByte();
            m_minDesiredIslandSize = br.ReadUInt32();
            m_processActionsInSingleThread = br.ReadBoolean();
            m_allowIntegrationOfIslandsWithoutConstraintsInASeparateJob = br.ReadBoolean();
            br.ReadUInt16();
            m_frameMarkerPsiSnap = br.ReadSingle();
            m_fireCollisionCallbacks = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4(bw, m_gravity);
            bw.WriteInt32(m_broadPhaseQuerySize);
            bw.WriteSingle(m_contactRestingVelocity);
            bw.WriteSByte((sbyte)m_broadPhaseType);
            bw.WriteSByte((sbyte)m_broadPhaseBorderBehaviour);
            bw.WriteBoolean(m_mtPostponeAndSortBroadPhaseBorderCallbacks);
            bw.WriteUInt32(0);
            bw.WriteByte(0);
            m_broadPhaseWorldAabb.Write(s, bw);
            bw.WriteSingle(m_collisionTolerance);
            bw.WriteUInt32(0);
            s.WriteClassPointer<hkpCollisionFilter>(bw, m_collisionFilter);
            s.WriteClassPointer<hkpConvexListFilter>(bw, m_convexListFilter);
            bw.WriteSingle(m_expectedMaxLinearVelocity);
            bw.WriteInt32(m_sizeOfToiEventQueue);
            bw.WriteSingle(m_expectedMinPsiDeltaTime);
            bw.WriteUInt32(0);
            s.WriteClassPointer<hkWorldMemoryAvailableWatchDog>(bw, m_memoryWatchDog);
            bw.WriteInt32(m_broadPhaseNumMarkers);
            bw.WriteSByte((sbyte)m_contactPointGeneration);
            bw.WriteBoolean(m_allowToSkipConfirmedCallbacks);
            bw.WriteUInt16(0);
            bw.WriteSingle(m_solverTau);
            bw.WriteSingle(m_solverDamp);
            bw.WriteInt32(m_solverIterations);
            bw.WriteInt32(m_solverMicrosteps);
            bw.WriteSingle(m_maxConstraintViolation);
            bw.WriteBoolean(m_forceCoherentConstraintOrderingInSolver);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteSingle(m_snapCollisionToConvexEdgeThreshold);
            bw.WriteSingle(m_snapCollisionToConcaveEdgeThreshold);
            bw.WriteBoolean(m_enableToiWeldRejection);
            bw.WriteBoolean(m_enableDeprecatedWelding);
            bw.WriteUInt16(0);
            bw.WriteSingle(m_iterativeLinearCastEarlyOutDistance);
            bw.WriteInt32(m_iterativeLinearCastMaxIterations);
            bw.WriteByte(m_deactivationNumInactiveFramesSelectFlag0);
            bw.WriteByte(m_deactivationNumInactiveFramesSelectFlag1);
            bw.WriteByte(m_deactivationIntegrateCounter);
            bw.WriteBoolean(m_shouldActivateOnRigidBodyTransformChange);
            bw.WriteSingle(m_deactivationReferenceDistance);
            bw.WriteSingle(m_toiCollisionResponseRotateNormal);
            bw.WriteBoolean(m_useCompoundSpuElf);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteInt32(m_maxSectorsPerMidphaseCollideTask);
            bw.WriteInt32(m_maxSectorsPerNarrowphaseCollideTask);
            bw.WriteBoolean(m_processToisMultithreaded);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteInt32(m_maxEntriesPerToiMidphaseCollideTask);
            bw.WriteInt32(m_maxEntriesPerToiNarrowphaseCollideTask);
            bw.WriteInt32(m_maxNumToiCollisionPairsSinglethreaded);
            bw.WriteSingle(m_numToisTillAllowedPenetrationSimplifiedToi);
            bw.WriteSingle(m_numToisTillAllowedPenetrationToi);
            bw.WriteSingle(m_numToisTillAllowedPenetrationToiHigher);
            bw.WriteSingle(m_numToisTillAllowedPenetrationToiForced);
            bw.WriteBoolean(m_enableDeactivation);
            bw.WriteSByte((sbyte)m_simulationType);
            bw.WriteBoolean(m_enableSimulationIslands);
            bw.WriteByte(0);
            bw.WriteUInt32(m_minDesiredIslandSize);
            bw.WriteBoolean(m_processActionsInSingleThread);
            bw.WriteBoolean(m_allowIntegrationOfIslandsWithoutConstraintsInASeparateJob);
            bw.WriteUInt16(0);
            bw.WriteSingle(m_frameMarkerPsiSnap);
            bw.WriteBoolean(m_fireCollisionCallbacks);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
