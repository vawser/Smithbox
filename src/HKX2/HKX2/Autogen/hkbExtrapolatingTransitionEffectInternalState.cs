using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbExtrapolatingTransitionEffectInternalState : hkReferencedObject
    {
        public override uint Signature { get => 3101182128; }
        
        public hkbGeneratorSyncInfo m_fromGeneratorSyncInfo;
        public hkbGeneratorPartitionInfo m_fromGeneratorPartitionInfo;
        public Matrix4x4 m_worldFromModel;
        public Matrix4x4 m_motion;
        public List<Matrix4x4> m_pose;
        public List<Matrix4x4> m_additivePose;
        public List<float> m_boneWeights;
        public float m_toGeneratorDuration;
        public bool m_isFromGeneratorActive;
        public bool m_gotPose;
        public bool m_gotAdditivePose;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_fromGeneratorSyncInfo = new hkbGeneratorSyncInfo();
            m_fromGeneratorSyncInfo.Read(des, br);
            m_fromGeneratorPartitionInfo = new hkbGeneratorPartitionInfo();
            m_fromGeneratorPartitionInfo.Read(des, br);
            br.ReadUInt32();
            m_worldFromModel = des.ReadQSTransform(br);
            m_motion = des.ReadQSTransform(br);
            m_pose = des.ReadQSTransformArray(br);
            m_additivePose = des.ReadQSTransformArray(br);
            m_boneWeights = des.ReadSingleArray(br);
            m_toGeneratorDuration = br.ReadSingle();
            m_isFromGeneratorActive = br.ReadBoolean();
            m_gotPose = br.ReadBoolean();
            m_gotAdditivePose = br.ReadBoolean();
            br.ReadUInt64();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_fromGeneratorSyncInfo.Write(s, bw);
            m_fromGeneratorPartitionInfo.Write(s, bw);
            bw.WriteUInt32(0);
            s.WriteQSTransform(bw, m_worldFromModel);
            s.WriteQSTransform(bw, m_motion);
            s.WriteQSTransformArray(bw, m_pose);
            s.WriteQSTransformArray(bw, m_additivePose);
            s.WriteSingleArray(bw, m_boneWeights);
            bw.WriteSingle(m_toGeneratorDuration);
            bw.WriteBoolean(m_isFromGeneratorActive);
            bw.WriteBoolean(m_gotPose);
            bw.WriteBoolean(m_gotAdditivePose);
            bw.WriteUInt64(0);
            bw.WriteByte(0);
        }
    }
}
