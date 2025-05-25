using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum ReplanningAI
    {
        Enable = 0,
        Disable = 1,
    }
    
    public enum ChangeTypeOfSelectedIndexAfterActivate
    {
        NONE = 0,
        SELF_TRANSITION = 1,
        UPDATE = 2,
    }
    
    public partial class CustomManualSelectorGenerator : hkbGenerator
    {
        public override uint Signature { get => 4285204516; }
        
        public enum OffsetType
        {
            WeaponCategoryRight = 13,
            WeaponCategoryLeft = 14,
            IdleCategory = 11,
            OffsetNone = 0,
            AnimIdOffset = 15,
            WeaponCategoryHandStyle = 16,
        }
        
        public enum AnimeEndEventType
        {
            FireNextStateEvent = 0,
            FireStateEndEvent = 1,
            FireIdleEvent = 2,
            None = 3,
        }
        
        public List<hkbGenerator> m_generators;
        public OffsetType m_offsetType;
        public int m_animId;
        public AnimeEndEventType m_animeEndEventType;
        public bool m_enableScript;
        public bool m_enableTae;
        public ChangeTypeOfSelectedIndexAfterActivate m_changeTypeOfSelectedIndexAfterActivate;
        public hkbTransitionEffect m_generatorChangedTransitionEffect;
        public int m_checkAnimEndSlotNo;
        public ReplanningAI m_replanningAI;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_generators = des.ReadClassPointerArray<hkbGenerator>(br);
            m_offsetType = (OffsetType)br.ReadInt32();
            m_animId = br.ReadInt32();
            m_animeEndEventType = (AnimeEndEventType)br.ReadInt32();
            m_enableScript = br.ReadBoolean();
            m_enableTae = br.ReadBoolean();
            m_changeTypeOfSelectedIndexAfterActivate = (ChangeTypeOfSelectedIndexAfterActivate)br.ReadByte();
            br.ReadByte();
            m_generatorChangedTransitionEffect = des.ReadClassPointer<hkbTransitionEffect>(br);
            m_checkAnimEndSlotNo = br.ReadInt32();
            m_replanningAI = (ReplanningAI)br.ReadByte();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt16();
            br.ReadByte();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassPointerArray<hkbGenerator>(bw, m_generators);
            bw.WriteInt32((int)m_offsetType);
            bw.WriteInt32(m_animId);
            bw.WriteInt32((int)m_animeEndEventType);
            bw.WriteBoolean(m_enableScript);
            bw.WriteBoolean(m_enableTae);
            bw.WriteByte((byte)m_changeTypeOfSelectedIndexAfterActivate);
            bw.WriteByte(0);
            s.WriteClassPointer<hkbTransitionEffect>(bw, m_generatorChangedTransitionEffect);
            bw.WriteInt32(m_checkAnimEndSlotNo);
            bw.WriteByte((byte)m_replanningAI);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt16(0);
            bw.WriteByte(0);
        }
    }
}
