using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum DetailLevel
    {
        DETAIL_INVALID = 0,
        DETAIL_FULL = 1,
        DETAIL_OBB = 2,
        DETAIL_CONVEX_HULL = 3,
    }
    
    public enum PointCloudFlagBits
    {
        LOCAL_POINTS_CHANGED = 1,
    }
    
    public partial class hkaiPointCloudSilhouetteGenerator : hkaiSilhouetteGenerator
    {
        public override uint Signature { get => 3740816178; }
        
        public hkAabb m_localAabb;
        public List<Vector4> m_localPoints;
        public List<int> m_silhouetteSizes;
        public float m_weldTolerance;
        public DetailLevel m_silhouetteDetailLevel;
        public byte m_flags;
        public bool m_localPointsChanged;
        public bool m_isEnabled;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_localAabb = new hkAabb();
            m_localAabb.Read(des, br);
            m_localPoints = des.ReadVector4Array(br);
            m_silhouetteSizes = des.ReadInt32Array(br);
            m_weldTolerance = br.ReadSingle();
            m_silhouetteDetailLevel = (DetailLevel)br.ReadByte();
            m_flags = br.ReadByte();
            m_localPointsChanged = br.ReadBoolean();
            m_isEnabled = br.ReadBoolean();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_localAabb.Write(s, bw);
            s.WriteVector4Array(bw, m_localPoints);
            s.WriteInt32Array(bw, m_silhouetteSizes);
            bw.WriteSingle(m_weldTolerance);
            bw.WriteByte((byte)m_silhouetteDetailLevel);
            bw.WriteByte(m_flags);
            bw.WriteBoolean(m_localPointsChanged);
            bw.WriteBoolean(m_isEnabled);
            bw.WriteUInt64(0);
        }
    }
}
