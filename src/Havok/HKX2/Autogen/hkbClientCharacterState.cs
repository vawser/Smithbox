using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbClientCharacterState : hkReferencedObject
    {
        public override uint Signature { get => 77299067; }
        
        public List<ulong> m_deformableSkinIds;
        public List<ulong> m_rigidSkinIds;
        public List<short> m_externalEventIds;
        public List<hkbAuxiliaryNodeInfo> m_auxiliaryInfo;
        public List<short> m_activeEventIds;
        public List<short> m_activeVariableIds;
        public ulong m_characterId;
        public string m_instanceName;
        public string m_templateName;
        public string m_fullPathToProject;
        public string m_localScriptsPath;
        public string m_remoteScriptsPath;
        public hkbBehaviorGraphData m_behaviorData;
        public hkbBehaviorGraphInternalState m_behaviorInternalState;
        public bool m_visible;
        public float m_elapsedSimulationTime;
        public hkaSkeleton m_skeleton;
        public Matrix4x4 m_worldFromModel;
        public List<Matrix4x4> m_poseModelSpace;
        public List<Matrix4x4> m_rigidAttachmentTransforms;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_deformableSkinIds = des.ReadUInt64Array(br);
            m_rigidSkinIds = des.ReadUInt64Array(br);
            m_externalEventIds = des.ReadInt16Array(br);
            m_auxiliaryInfo = des.ReadClassPointerArray<hkbAuxiliaryNodeInfo>(br);
            m_activeEventIds = des.ReadInt16Array(br);
            m_activeVariableIds = des.ReadInt16Array(br);
            m_characterId = br.ReadUInt64();
            m_instanceName = des.ReadStringPointer(br);
            m_templateName = des.ReadStringPointer(br);
            m_fullPathToProject = des.ReadStringPointer(br);
            m_localScriptsPath = des.ReadStringPointer(br);
            m_remoteScriptsPath = des.ReadStringPointer(br);
            m_behaviorData = des.ReadClassPointer<hkbBehaviorGraphData>(br);
            m_behaviorInternalState = des.ReadClassPointer<hkbBehaviorGraphInternalState>(br);
            br.ReadUInt64();
            m_visible = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_elapsedSimulationTime = br.ReadSingle();
            m_skeleton = des.ReadClassPointer<hkaSkeleton>(br);
            br.ReadUInt64();
            m_worldFromModel = des.ReadQSTransform(br);
            m_poseModelSpace = des.ReadQSTransformArray(br);
            m_rigidAttachmentTransforms = des.ReadQSTransformArray(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteUInt64Array(bw, m_deformableSkinIds);
            s.WriteUInt64Array(bw, m_rigidSkinIds);
            s.WriteInt16Array(bw, m_externalEventIds);
            s.WriteClassPointerArray<hkbAuxiliaryNodeInfo>(bw, m_auxiliaryInfo);
            s.WriteInt16Array(bw, m_activeEventIds);
            s.WriteInt16Array(bw, m_activeVariableIds);
            bw.WriteUInt64(m_characterId);
            s.WriteStringPointer(bw, m_instanceName);
            s.WriteStringPointer(bw, m_templateName);
            s.WriteStringPointer(bw, m_fullPathToProject);
            s.WriteStringPointer(bw, m_localScriptsPath);
            s.WriteStringPointer(bw, m_remoteScriptsPath);
            s.WriteClassPointer<hkbBehaviorGraphData>(bw, m_behaviorData);
            s.WriteClassPointer<hkbBehaviorGraphInternalState>(bw, m_behaviorInternalState);
            bw.WriteUInt64(0);
            bw.WriteBoolean(m_visible);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteSingle(m_elapsedSimulationTime);
            s.WriteClassPointer<hkaSkeleton>(bw, m_skeleton);
            bw.WriteUInt64(0);
            s.WriteQSTransform(bw, m_worldFromModel);
            s.WriteQSTransformArray(bw, m_poseModelSpace);
            s.WriteQSTransformArray(bw, m_rigidAttachmentTransforms);
        }
    }
}
