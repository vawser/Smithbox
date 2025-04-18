using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbGeneratorSyncInfo : IHavokObject
    {
        public virtual uint Signature { get => 2261737732; }
        
        public hkbGeneratorSyncInfoSyncPoint m_syncPoints_0;
        public hkbGeneratorSyncInfoSyncPoint m_syncPoints_1;
        public hkbGeneratorSyncInfoSyncPoint m_syncPoints_2;
        public hkbGeneratorSyncInfoSyncPoint m_syncPoints_3;
        public hkbGeneratorSyncInfoSyncPoint m_syncPoints_4;
        public hkbGeneratorSyncInfoSyncPoint m_syncPoints_5;
        public hkbGeneratorSyncInfoSyncPoint m_syncPoints_6;
        public hkbGeneratorSyncInfoSyncPoint m_syncPoints_7;
        public hkbGeneratorSyncInfoSyncPoint m_syncPoints_8;
        public hkbGeneratorSyncInfoSyncPoint m_syncPoints_9;
        public hkbGeneratorSyncInfoSyncPoint m_syncPoints_10;
        public hkbGeneratorSyncInfoSyncPoint m_syncPoints_11;
        public hkbGeneratorSyncInfoSyncPoint m_syncPoints_12;
        public hkbGeneratorSyncInfoSyncPoint m_syncPoints_13;
        public hkbGeneratorSyncInfoSyncPoint m_syncPoints_14;
        public hkbGeneratorSyncInfoSyncPoint m_syncPoints_15;
        public float m_duration;
        public float m_localTime;
        public float m_playbackSpeed;
        public sbyte m_numSyncPoints;
        public bool m_isCyclic;
        public bool m_isMirrored;
        public bool m_isAdditive;
        public hkbGeneratorSyncInfoActiveInterval m_activeInterval;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_syncPoints_0 = new hkbGeneratorSyncInfoSyncPoint();
            m_syncPoints_0.Read(des, br);
            m_syncPoints_1 = new hkbGeneratorSyncInfoSyncPoint();
            m_syncPoints_1.Read(des, br);
            m_syncPoints_2 = new hkbGeneratorSyncInfoSyncPoint();
            m_syncPoints_2.Read(des, br);
            m_syncPoints_3 = new hkbGeneratorSyncInfoSyncPoint();
            m_syncPoints_3.Read(des, br);
            m_syncPoints_4 = new hkbGeneratorSyncInfoSyncPoint();
            m_syncPoints_4.Read(des, br);
            m_syncPoints_5 = new hkbGeneratorSyncInfoSyncPoint();
            m_syncPoints_5.Read(des, br);
            m_syncPoints_6 = new hkbGeneratorSyncInfoSyncPoint();
            m_syncPoints_6.Read(des, br);
            m_syncPoints_7 = new hkbGeneratorSyncInfoSyncPoint();
            m_syncPoints_7.Read(des, br);
            m_syncPoints_8 = new hkbGeneratorSyncInfoSyncPoint();
            m_syncPoints_8.Read(des, br);
            m_syncPoints_9 = new hkbGeneratorSyncInfoSyncPoint();
            m_syncPoints_9.Read(des, br);
            m_syncPoints_10 = new hkbGeneratorSyncInfoSyncPoint();
            m_syncPoints_10.Read(des, br);
            m_syncPoints_11 = new hkbGeneratorSyncInfoSyncPoint();
            m_syncPoints_11.Read(des, br);
            m_syncPoints_12 = new hkbGeneratorSyncInfoSyncPoint();
            m_syncPoints_12.Read(des, br);
            m_syncPoints_13 = new hkbGeneratorSyncInfoSyncPoint();
            m_syncPoints_13.Read(des, br);
            m_syncPoints_14 = new hkbGeneratorSyncInfoSyncPoint();
            m_syncPoints_14.Read(des, br);
            m_syncPoints_15 = new hkbGeneratorSyncInfoSyncPoint();
            m_syncPoints_15.Read(des, br);
            m_duration = br.ReadSingle();
            m_localTime = br.ReadSingle();
            m_playbackSpeed = br.ReadSingle();
            m_numSyncPoints = br.ReadSByte();
            m_isCyclic = br.ReadBoolean();
            m_isMirrored = br.ReadBoolean();
            m_isAdditive = br.ReadBoolean();
            m_activeInterval = new hkbGeneratorSyncInfoActiveInterval();
            m_activeInterval.Read(des, br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            m_syncPoints_0.Write(s, bw);
            m_syncPoints_1.Write(s, bw);
            m_syncPoints_2.Write(s, bw);
            m_syncPoints_3.Write(s, bw);
            m_syncPoints_4.Write(s, bw);
            m_syncPoints_5.Write(s, bw);
            m_syncPoints_6.Write(s, bw);
            m_syncPoints_7.Write(s, bw);
            m_syncPoints_8.Write(s, bw);
            m_syncPoints_9.Write(s, bw);
            m_syncPoints_10.Write(s, bw);
            m_syncPoints_11.Write(s, bw);
            m_syncPoints_12.Write(s, bw);
            m_syncPoints_13.Write(s, bw);
            m_syncPoints_14.Write(s, bw);
            m_syncPoints_15.Write(s, bw);
            bw.WriteSingle(m_duration);
            bw.WriteSingle(m_localTime);
            bw.WriteSingle(m_playbackSpeed);
            bw.WriteSByte(m_numSyncPoints);
            bw.WriteBoolean(m_isCyclic);
            bw.WriteBoolean(m_isMirrored);
            bw.WriteBoolean(m_isAdditive);
            m_activeInterval.Write(s, bw);
        }
    }
}
