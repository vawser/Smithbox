using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum StepThreading
    {
        STEP_SINGLE_THREADED = 0,
        STEP_MULTI_THREADED = 1,
    }
    
    public enum CharacterCallbackType
    {
        CALLBACK_PRECHARACTER_STEP = 0,
        CALLBACK_POSTCHARACTER_STEP = 1,
    }
    
    public enum PathType
    {
        PATH_TYPE_NAVMESH = 0,
        PATH_TYPE_NAVVOLUME = 1,
    }
    
    public partial class hkaiWorld : hkReferencedObject
    {
        public override uint Signature { get => 2975180790; }
        
        public Vector4 m_up;
        public hkaiStreamingCollection m_streamingCollection;
        public hkaiNavMeshCutter m_cutter;
        public bool m_performValidationChecks;
        public hkaiDynamicNavMeshQueryMediator m_dynamicNavMeshMediator;
        public hkaiDynamicNavVolumeMediator m_dynamicNavVolumeMediator;
        public hkaiOverlapManager m_overlapManager;
        public hkaiSilhouetteGenerationParameters m_silhouetteGenerationParameters;
        public float m_silhouetteExtrusion;
        public bool m_forceSilhouetteUpdates;
        public List<hkaiSilhouetteGenerator> m_silhouetteGenerators;
        public List<hkaiObstacleGenerator> m_obstacleGenerators;
        public hkaiAvoidancePairProperties m_avoidancePairProps;
        public int m_maxRequestsPerStep;
        public int m_maxEstimatedIterationsPerStep;
        public int m_priorityThreshold;
        public int m_numPathRequestsPerTask;
        public int m_numBehaviorUpdatesPerTask;
        public int m_numCharactersPerAvoidanceTask;
        public int m_maxPathSearchEdgesOut;
        public int m_maxPathSearchPointsOut;
        public bool m_precomputeNavMeshClearance;
        public hkaiPathfindingUtilFindPathInput m_defaultPathfindingInput;
        public hkaiVolumePathfindingUtilFindPathInput m_defaultVolumePathfindingInput;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_up = des.ReadVector4(br);
            m_streamingCollection = des.ReadClassPointer<hkaiStreamingCollection>(br);
            m_cutter = des.ReadClassPointer<hkaiNavMeshCutter>(br);
            m_performValidationChecks = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
            m_dynamicNavMeshMediator = des.ReadClassPointer<hkaiDynamicNavMeshQueryMediator>(br);
            m_dynamicNavVolumeMediator = des.ReadClassPointer<hkaiDynamicNavVolumeMediator>(br);
            m_overlapManager = des.ReadClassPointer<hkaiOverlapManager>(br);
            m_silhouetteGenerationParameters = new hkaiSilhouetteGenerationParameters();
            m_silhouetteGenerationParameters.Read(des, br);
            m_silhouetteExtrusion = br.ReadSingle();
            m_forceSilhouetteUpdates = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt16();
            br.ReadByte();
            m_silhouetteGenerators = des.ReadClassPointerArray<hkaiSilhouetteGenerator>(br);
            m_obstacleGenerators = des.ReadClassPointerArray<hkaiObstacleGenerator>(br);
            m_avoidancePairProps = des.ReadClassPointer<hkaiAvoidancePairProperties>(br);
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt32();
            m_maxRequestsPerStep = br.ReadInt32();
            m_maxEstimatedIterationsPerStep = br.ReadInt32();
            m_priorityThreshold = br.ReadInt32();
            m_numPathRequestsPerTask = br.ReadInt32();
            m_numBehaviorUpdatesPerTask = br.ReadInt32();
            m_numCharactersPerAvoidanceTask = br.ReadInt32();
            m_maxPathSearchEdgesOut = br.ReadInt32();
            m_maxPathSearchPointsOut = br.ReadInt32();
            m_precomputeNavMeshClearance = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadUInt16();
            br.ReadByte();
            m_defaultPathfindingInput = new hkaiPathfindingUtilFindPathInput();
            m_defaultPathfindingInput.Read(des, br);
            m_defaultVolumePathfindingInput = new hkaiVolumePathfindingUtilFindPathInput();
            m_defaultVolumePathfindingInput.Read(des, br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteVector4(bw, m_up);
            s.WriteClassPointer<hkaiStreamingCollection>(bw, m_streamingCollection);
            s.WriteClassPointer<hkaiNavMeshCutter>(bw, m_cutter);
            bw.WriteBoolean(m_performValidationChecks);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            s.WriteClassPointer<hkaiDynamicNavMeshQueryMediator>(bw, m_dynamicNavMeshMediator);
            s.WriteClassPointer<hkaiDynamicNavVolumeMediator>(bw, m_dynamicNavVolumeMediator);
            s.WriteClassPointer<hkaiOverlapManager>(bw, m_overlapManager);
            m_silhouetteGenerationParameters.Write(s, bw);
            bw.WriteSingle(m_silhouetteExtrusion);
            bw.WriteBoolean(m_forceSilhouetteUpdates);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            s.WriteClassPointerArray<hkaiSilhouetteGenerator>(bw, m_silhouetteGenerators);
            s.WriteClassPointerArray<hkaiObstacleGenerator>(bw, m_obstacleGenerators);
            s.WriteClassPointer<hkaiAvoidancePairProperties>(bw, m_avoidancePairProps);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteInt32(m_maxRequestsPerStep);
            bw.WriteInt32(m_maxEstimatedIterationsPerStep);
            bw.WriteInt32(m_priorityThreshold);
            bw.WriteInt32(m_numPathRequestsPerTask);
            bw.WriteInt32(m_numBehaviorUpdatesPerTask);
            bw.WriteInt32(m_numCharactersPerAvoidanceTask);
            bw.WriteInt32(m_maxPathSearchEdgesOut);
            bw.WriteInt32(m_maxPathSearchPointsOut);
            bw.WriteBoolean(m_precomputeNavMeshClearance);
            bw.WriteUInt64(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            m_defaultPathfindingInput.Write(s, bw);
            m_defaultVolumePathfindingInput.Write(s, bw);
        }
    }
}
