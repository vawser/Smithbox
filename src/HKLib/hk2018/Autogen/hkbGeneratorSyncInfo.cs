// Automatically Generated

namespace HKLib.hk2018;

public class hkbGeneratorSyncInfo : IHavokObject
{
    public readonly hkbGeneratorSyncInfo.SyncPoint[] m_syncPoints = new hkbGeneratorSyncInfo.SyncPoint[16];

    public float m_duration;

    public float m_localTime;

    public float m_playbackSpeed;

    public sbyte m_numSyncPoints;

    public bool m_isCyclic;

    public bool m_isMirrored;

    public bool m_isAdditive;

    public hkbGeneratorSyncInfo.ActiveInterval m_activeInterval = new();


    public class ActiveInterval : IHavokObject
    {
        public readonly hkbGeneratorSyncInfo.SyncPoint[] m_syncPoints = new hkbGeneratorSyncInfo.SyncPoint[2];

        public float m_fraction;

    }


    public class SyncPoint : IHavokObject
    {
        public int m_id;

        public float m_time;

    }


}

