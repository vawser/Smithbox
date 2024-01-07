using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkaiNavMeshGenerationSettingsOverrideSettings : IHavokObject
    {
        public virtual uint Signature { get => 431992409; }
        
        public hkaiVolume m_volume;
        public int m_material;
        public CharacterWidthUsage m_characterWidthUsage;
        public float m_maxWalkableSlope;
        public hkaiNavMeshEdgeMatchingParameters m_edgeMatchingParams;
        public hkaiNavMeshSimplificationUtilsSettings m_simplificationSettings;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_volume = des.ReadClassPointer<hkaiVolume>(br);
            m_material = br.ReadInt32();
            m_characterWidthUsage = (CharacterWidthUsage)br.ReadByte();
            br.ReadUInt16();
            br.ReadByte();
            m_maxWalkableSlope = br.ReadSingle();
            m_edgeMatchingParams = new hkaiNavMeshEdgeMatchingParameters();
            m_edgeMatchingParams.Read(des, br);
            br.ReadUInt32();
            m_simplificationSettings = new hkaiNavMeshSimplificationUtilsSettings();
            m_simplificationSettings.Read(des, br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassPointer<hkaiVolume>(bw, m_volume);
            bw.WriteInt32(m_material);
            bw.WriteByte((byte)m_characterWidthUsage);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            bw.WriteSingle(m_maxWalkableSlope);
            m_edgeMatchingParams.Write(s, bw);
            bw.WriteUInt32(0);
            m_simplificationSettings.Write(s, bw);
        }
    }
}
