using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum ConstraintPriority
    {
        PRIORITY_INVALID = 0,
        PRIORITY_PSI = 1,
        PRIORITY_SIMPLIFIED_TOI_UNUSED = 2,
        PRIORITY_TOI = 3,
        PRIORITY_TOI_HIGHER = 4,
        PRIORITY_TOI_FORCED = 5,
        NUM_PRIORITIES = 6,
    }
    
    public enum InstanceType
    {
        TYPE_NORMAL = 0,
        TYPE_CHAIN = 1,
        TYPE_DISABLE_SPU = 2,
    }
    
    public enum AddReferences
    {
        DO_NOT_ADD_REFERENCES = 0,
        DO_ADD_REFERENCES = 1,
    }
    
    public enum CloningMode
    {
        CLONE_SHALLOW_IF_NOT_CONSTRAINED_TO_WORLD = 0,
        CLONE_DATAS_WITH_MOTORS = 1,
        CLONE_FORCE_SHALLOW = 2,
    }
    
    public enum OnDestructionRemapInfo
    {
        ON_DESTRUCTION_REMAP = 0,
        ON_DESTRUCTION_REMOVE = 1,
        ON_DESTRUCTION_RESET_REMOVE = 2,
    }
    
    public partial class hkpConstraintInstance : hkReferencedObject
    {
        public override uint Signature { get => 3662473502; }
        
        public hkpConstraintData m_data;
        public hkpModifierConstraintAtom m_constraintModifiers;
        public hkpEntity m_entities_0;
        public hkpEntity m_entities_1;
        public ConstraintPriority m_priority;
        public bool m_wantRuntime;
        public OnDestructionRemapInfo m_destructionRemapInfo;
        public string m_name;
        public ulong m_userData;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            br.ReadUInt64();
            m_data = des.ReadClassPointer<hkpConstraintData>(br);
            m_constraintModifiers = des.ReadClassPointer<hkpModifierConstraintAtom>(br);
            m_entities_0 = des.ReadClassPointer<hkpEntity>(br);
            m_entities_1 = des.ReadClassPointer<hkpEntity>(br);
            m_priority = (ConstraintPriority)br.ReadByte();
            m_wantRuntime = br.ReadBoolean();
            m_destructionRemapInfo = (OnDestructionRemapInfo)br.ReadByte();
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt32();
            br.ReadByte();
            m_name = des.ReadStringPointer(br);
            m_userData = br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt64();
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            bw.WriteUInt64(0);
            s.WriteClassPointer<hkpConstraintData>(bw, m_data);
            s.WriteClassPointer<hkpModifierConstraintAtom>(bw, m_constraintModifiers);
            s.WriteClassPointer<hkpEntity>(bw, m_entities_0);
            s.WriteClassPointer<hkpEntity>(bw, m_entities_1);
            bw.WriteByte((byte)m_priority);
            bw.WriteBoolean(m_wantRuntime);
            bw.WriteByte((byte)m_destructionRemapInfo);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteByte(0);
            s.WriteStringPointer(bw, m_name);
            bw.WriteUInt64(m_userData);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
        }
    }
}
