using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclClothStateSetupObject : hkReferencedObject
    {
        public override uint Signature { get => 4195376119; }
        
        public string m_name;
        public List<hclOperatorSetupObject> m_operatorSetupObjects;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_name = des.ReadStringPointer(br);
            m_operatorSetupObjects = des.ReadClassPointerArray<hclOperatorSetupObject>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_name);
            s.WriteClassPointerArray<hclOperatorSetupObject>(bw, m_operatorSetupObjects);
        }
    }
}
