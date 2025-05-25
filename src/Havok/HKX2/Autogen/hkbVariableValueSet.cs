using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hkbVariableValueSet : hkReferencedObject
    {
        public override uint Signature { get => 3948903973; }
        
        public List<hkbVariableValue> m_wordVariableValues;
        public List<Vector4> m_quadVariableValues;
        public List<hkReferencedObject> m_variantVariableValues;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_wordVariableValues = des.ReadClassArray<hkbVariableValue>(br);
            m_quadVariableValues = des.ReadVector4Array(br);
            m_variantVariableValues = des.ReadClassPointerArray<hkReferencedObject>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteClassArray<hkbVariableValue>(bw, m_wordVariableValues);
            s.WriteVector4Array(bw, m_quadVariableValues);
            s.WriteClassPointerArray<hkReferencedObject>(bw, m_variantVariableValues);
        }
    }
}
