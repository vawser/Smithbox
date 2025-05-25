using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkxEnvironment : hkReferencedObject
    {
        public override uint Signature { get => 2572384914; }
        
        public List<hkxEnvironmentVariable> m_variables;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_variables = des.ReadClassArray<hkxEnvironmentVariable>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkxEnvironmentVariable>(bw, m_variables);
        }
    }
}
