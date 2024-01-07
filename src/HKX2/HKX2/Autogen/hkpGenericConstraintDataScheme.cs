using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkpGenericConstraintDataScheme : IHavokObject
    {
        public virtual uint Signature { get => 301821804; }
        
        public List<Vector4> m_data;
        public List<int> m_commands;
        public List<hkpConstraintMotor> m_motors;
        
        public virtual void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            br.ReadUInt64();
            br.ReadUInt64();
            m_data = des.ReadVector4Array(br);
            m_commands = des.ReadInt32Array(br);
            br.ReadUInt64();
            br.ReadUInt64();
            m_motors = des.ReadClassPointerArray<hkpConstraintMotor>(br);
        }
        
        public virtual void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            s.WriteVector4Array(bw, m_data);
            s.WriteInt32Array(bw, m_commands);
            bw.WriteUInt64(0);
            bw.WriteUInt64(0);
            s.WriteClassPointerArray<hkpConstraintMotor>(bw, m_motors);
        }
    }
}
