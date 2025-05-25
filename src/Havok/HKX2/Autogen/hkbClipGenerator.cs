using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbClipGenerator : hkbGenerator
    {
        public override uint Signature { get => 223136246; }
        
        public enum PlaybackMode
        {
            MODE_SINGLE_PLAY = 0,
            MODE_LOOPING = 1,
            MODE_USER_CONTROLLED = 2,
            MODE_PING_PONG = 3,
            MODE_COUNT = 4,
        }
        
        public enum ClipFlags
        {
            FLAG_CONTINUE_MOTION_AT_END = 1,
            FLAG_SYNC_HALF_CYCLE_IN_PING_PONG_MODE = 2,
            FLAG_MIRROR = 4,
            FLAG_FORCE_DENSE_POSE = 8,
            FLAG_DONT_CONVERT_ANNOTATIONS_TO_TRIGGERS = 16,
            FLAG_IGNORE_MOTION = 32,
        }
        
        public string m_animationBundleName;
        public string m_animationName;
        public hkbClipTriggerArray m_triggers;
        public uint m_userPartitionMask;
        public float m_cropStartAmountLocalTime;
        public float m_cropEndAmountLocalTime;
        public float m_startTime;
        public float m_playbackSpeed;
        public float m_enforcedDuration;
        public float m_userControlledTimeFraction;
        public short m_animationBindingIndex;
        public PlaybackMode m_mode;
        public sbyte m_flags;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_animationBundleName = des.ReadStringPointer(br);
            m_animationName = des.ReadStringPointer(br);
            m_triggers = des.ReadClassPointer<hkbClipTriggerArray>(br);
            m_userPartitionMask = br.ReadUInt32();
            m_cropStartAmountLocalTime = br.ReadSingle();
            m_cropEndAmountLocalTime = br.ReadSingle();
            m_startTime = br.ReadSingle();
            m_playbackSpeed = br.ReadSingle();
            m_enforcedDuration = br.ReadSingle();
            m_userControlledTimeFraction = br.ReadSingle();
            m_animationBindingIndex = br.ReadInt16();
            m_mode = (PlaybackMode)br.ReadSByte();
            m_flags = br.ReadSByte();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_animationBundleName);
            s.WriteStringPointer(bw, m_animationName);
            s.WriteClassPointer<hkbClipTriggerArray>(bw, m_triggers);
            bw.WriteUInt32(m_userPartitionMask);
            bw.WriteSingle(m_cropStartAmountLocalTime);
            bw.WriteSingle(m_cropEndAmountLocalTime);
            bw.WriteSingle(m_startTime);
            bw.WriteSingle(m_playbackSpeed);
            bw.WriteSingle(m_enforcedDuration);
            bw.WriteSingle(m_userControlledTimeFraction);
            bw.WriteInt16(m_animationBindingIndex);
            bw.WriteSByte((sbyte)m_mode);
            bw.WriteSByte(m_flags);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
        }
    }
}
