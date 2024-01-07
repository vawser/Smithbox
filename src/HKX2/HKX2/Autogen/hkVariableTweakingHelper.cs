using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkVariableTweakingHelper : IHavokObject
    {
        public virtual uint Signature { get => 3385990895; }
        
        public List<hkVariableTweakingHelperBoolVariableInfo> m_boolVariableInfo;
        public List<hkVariableTweakingHelperIntVariableInfo> m_intVariableInfo;
        public List<hkVariableTweakingHelperRealVariableInfo> m_realVariableInfo;
        public List<hkVariableTweakingHelperVector4VariableInfo> m_vector4VariableInfo;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_boolVariableInfo = des.ReadClassArray<hkVariableTweakingHelperBoolVariableInfo>(br);
            m_intVariableInfo = des.ReadClassArray<hkVariableTweakingHelperIntVariableInfo>(br);
            m_realVariableInfo = des.ReadClassArray<hkVariableTweakingHelperRealVariableInfo>(br);
            m_vector4VariableInfo = des.ReadClassArray<hkVariableTweakingHelperVector4VariableInfo>(br);
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
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteClassArray<hkVariableTweakingHelperBoolVariableInfo>(bw, m_boolVariableInfo);
            s.WriteClassArray<hkVariableTweakingHelperIntVariableInfo>(bw, m_intVariableInfo);
            s.WriteClassArray<hkVariableTweakingHelperRealVariableInfo>(bw, m_realVariableInfo);
            s.WriteClassArray<hkVariableTweakingHelperVector4VariableInfo>(bw, m_vector4VariableInfo);
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
