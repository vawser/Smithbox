using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public enum BindingType
    {
        BINDING_TYPE_VARIABLE = 0,
        BINDING_TYPE_CHARACTER_PROPERTY = 1,
    }
    
    public enum InternalBindingFlags
    {
        FLAG_NONE = 0,
        FLAG_OUTPUT = 1,
    }
    
    public partial class hkbVariableBindingSetBinding : IHavokObject
    {
        public virtual uint Signature { get => 1297690482; }
        
        public string m_memberPath;
        public int m_variableIndex;
        public sbyte m_bitIndex;
        public BindingType m_bindingType;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            m_memberPath = des.ReadStringPointer(br);
            br.ReadUInt64();
            br.ReadUInt64();
            br.ReadUInt32();
            m_variableIndex = br.ReadInt32();
            m_bitIndex = br.ReadSByte();
            m_bindingType = (BindingType)br.ReadSByte();
            br.ReadUInt32();
            br.ReadUInt16();
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            s.WriteStringPointer(bw, m_memberPath);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            bw.WriteUInt32(0);
            bw.WriteInt32(m_variableIndex);
            bw.WriteSByte(m_bitIndex);
            bw.WriteSByte((sbyte)m_bindingType);
            bw.WriteUInt32(0);
            bw.WriteUInt16(0);
        }
    }
}
