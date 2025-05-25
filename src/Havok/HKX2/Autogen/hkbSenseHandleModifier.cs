using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum SensingMode
    {
        SENSE_IN_NEARBY_RIGID_BODIES = 0,
        SENSE_IN_RIGID_BODIES_OUTSIDE_THIS_CHARACTER = 1,
        SENSE_IN_OTHER_CHARACTER_RIGID_BODIES = 2,
        SENSE_IN_THIS_CHARACTER_RIGID_BODIES = 3,
        SENSE_IN_GIVEN_CHARACTER_RIGID_BODIES = 4,
        SENSE_IN_GIVEN_RIGID_BODY = 5,
        SENSE_IN_OTHER_CHARACTER_SKELETON = 6,
        SENSE_IN_THIS_CHARACTER_SKELETON = 7,
        SENSE_IN_GIVEN_CHARACTER_SKELETON = 8,
        SENSE_IN_GIVEN_LOCAL_FRAME_GROUP = 9,
    }
    
    public partial class hkbSenseHandleModifier : hkbModifier
    {
        public override uint Signature { get => 1337469111; }
        
        public Vector4 m_sensorLocalOffset;
        public List<hkbSenseHandleModifierRange> m_ranges;
        public hkbHandle m_handleOut;
        public hkbHandle m_handleIn;
        public string m_localFrameName;
        public string m_sensorLocalFrameName;
        public float m_minDistance;
        public float m_maxDistance;
        public float m_distanceOut;
        public uint m_collisionFilterInfo;
        public short m_sensorRagdollBoneIndex;
        public short m_sensorAnimationBoneIndex;
        public SensingMode m_sensingMode;
        public bool m_extrapolateSensorPosition;
        public bool m_keepFirstSensedHandle;
        public bool m_foundHandleOut;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            m_sensorLocalOffset = des.ReadVector4(br);
            m_ranges = des.ReadClassArray<hkbSenseHandleModifierRange>(br);
            m_handleOut = des.ReadClassPointer<hkbHandle>(br);
            m_handleIn = des.ReadClassPointer<hkbHandle>(br);
            m_localFrameName = des.ReadStringPointer(br);
            m_sensorLocalFrameName = des.ReadStringPointer(br);
            m_minDistance = br.ReadSingle();
            m_maxDistance = br.ReadSingle();
            m_distanceOut = br.ReadSingle();
            m_collisionFilterInfo = br.ReadUInt32();
            m_sensorRagdollBoneIndex = br.ReadInt16();
            m_sensorAnimationBoneIndex = br.ReadInt16();
            m_sensingMode = (SensingMode)br.ReadSByte();
            m_extrapolateSensorPosition = br.ReadBoolean();
            m_keepFirstSensedHandle = br.ReadBoolean();
            m_foundHandleOut = br.ReadBoolean();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            s.WriteVector4(bw, m_sensorLocalOffset);
            s.WriteClassArray<hkbSenseHandleModifierRange>(bw, m_ranges);
            s.WriteClassPointer<hkbHandle>(bw, m_handleOut);
            s.WriteClassPointer<hkbHandle>(bw, m_handleIn);
            s.WriteStringPointer(bw, m_localFrameName);
            s.WriteStringPointer(bw, m_sensorLocalFrameName);
            bw.WriteSingle(m_minDistance);
            bw.WriteSingle(m_maxDistance);
            bw.WriteSingle(m_distanceOut);
            bw.WriteUInt32(m_collisionFilterInfo);
            bw.WriteInt16(m_sensorRagdollBoneIndex);
            bw.WriteInt16(m_sensorAnimationBoneIndex);
            bw.WriteSByte((sbyte)m_sensingMode);
            bw.WriteBoolean(m_extrapolateSensorPosition);
            bw.WriteBoolean(m_keepFirstSensedHandle);
            bw.WriteBoolean(m_foundHandleOut);
            bw.WriteUInt64(0);
        }
    }
}
