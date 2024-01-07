using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum MaterialFlagsBits
    {
        MATERIAL_NONE = 0,
        MATERIAL_BLOCKING = 1,
        MATERIAL_DEFAULT = 1,
    }
    
    public enum CellWidthToResolutionRounding
    {
        ROUND_TO_ZERO = 0,
        ROUND_TO_NEAREST = 1,
    }
    
    public partial class hkaiNavVolumeGenerationSettings : hkReferencedObject
    {
        public override uint Signature { get => 3417771885; }
        
        public hkAabb m_volumeAabb;
        public float m_maxHorizontalRange;
        public float m_maxVerticalRange;
        public Vector4 m_up;
        public float m_characterHeight;
        public float m_characterDepth;
        public float m_characterWidth;
        public float m_cellWidth;
        public CellWidthToResolutionRounding m_resolutionRoundingMode;
        public hkaiNavVolumeGenerationSettingsChunkSettings m_chunkSettings;
        public float m_border;
        public bool m_useBorderCells;
        public hkaiNavVolumeGenerationSettingsMergingSettings m_mergingSettings;
        public float m_minRegionVolume;
        public float m_minDistanceToSeedPoints;
        public List<Vector4> m_regionSeedPoints;
        public hkaiNavVolumeGenerationSettingsMaterialConstructionInfo m_defaultConstructionInfo;
        public List<hkaiNavVolumeGenerationSettingsMaterialConstructionInfo> m_materialMap;
        public List<hkaiCarver> m_carvers;
        public List<hkaiMaterialPainter> m_painters;
        public bool m_saveInputSnapshot;
        public string m_snapshotFilename;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_volumeAabb = new hkAabb();
            m_volumeAabb.Read(des, br);
            m_maxHorizontalRange = br.ReadSingle();
            m_maxVerticalRange = br.ReadSingle();
            br.ReadUInt64();
            m_up = des.ReadVector4(br);
            m_characterHeight = br.ReadSingle();
            m_characterDepth = br.ReadSingle();
            m_characterWidth = br.ReadSingle();
            m_cellWidth = br.ReadSingle();
            m_resolutionRoundingMode = (CellWidthToResolutionRounding)br.ReadByte();
            br.ReadByte();
            m_chunkSettings = new hkaiNavVolumeGenerationSettingsChunkSettings();
            m_chunkSettings.Read(des, br);
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadUInt16();
            m_border = br.ReadSingle();
            m_useBorderCells = br.ReadBoolean();
            br.ReadUInt16();
            br.ReadByte();
            m_mergingSettings = new hkaiNavVolumeGenerationSettingsMergingSettings();
            m_mergingSettings.Read(des, br);
            m_minRegionVolume = br.ReadSingle();
            m_minDistanceToSeedPoints = br.ReadSingle();
            br.ReadUInt32();
            m_regionSeedPoints = des.ReadVector4Array(br);
            m_defaultConstructionInfo = new hkaiNavVolumeGenerationSettingsMaterialConstructionInfo();
            m_defaultConstructionInfo.Read(des, br);
            br.ReadUInt32();
            m_materialMap = des.ReadClassArray<hkaiNavVolumeGenerationSettingsMaterialConstructionInfo>(br);
            m_carvers = des.ReadClassPointerArray<hkaiCarver>(br);
            m_painters = des.ReadClassPointerArray<hkaiMaterialPainter>(br);
            m_saveInputSnapshot = br.ReadBoolean();
            br.ReadUInt32();
            br.ReadUInt16();
            br.ReadByte();
            m_snapshotFilename = des.ReadStringPointer(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            m_volumeAabb.Write(s, bw);
            bw.WriteSingle(m_maxHorizontalRange);
            bw.WriteSingle(m_maxVerticalRange);
            bw.WriteUInt64(0);
            s.WriteVector4(bw, m_up);
            bw.WriteSingle(m_characterHeight);
            bw.WriteSingle(m_characterDepth);
            bw.WriteSingle(m_characterWidth);
            bw.WriteSingle(m_cellWidth);
            bw.WriteByte((byte)m_resolutionRoundingMode);
            bw.WriteByte(0);
            m_chunkSettings.Write(s, bw);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteSingle(m_border);
            bw.WriteBoolean(m_useBorderCells);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            m_mergingSettings.Write(s, bw);
            bw.WriteSingle(m_minRegionVolume);
            bw.WriteSingle(m_minDistanceToSeedPoints);
            bw.WriteUInt32(0);
            s.WriteVector4Array(bw, m_regionSeedPoints);
            m_defaultConstructionInfo.Write(s, bw);
            bw.WriteUInt32(0);
            s.WriteClassArray<hkaiNavVolumeGenerationSettingsMaterialConstructionInfo>(bw, m_materialMap);
            s.WriteClassPointerArray<hkaiCarver>(bw, m_carvers);
            s.WriteClassPointerArray<hkaiMaterialPainter>(bw, m_painters);
            bw.WriteBoolean(m_saveInputSnapshot);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
            s.WriteStringPointer(bw, m_snapshotFilename);
        }
    }
}
