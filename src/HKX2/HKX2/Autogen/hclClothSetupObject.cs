using SoulsFormats;
using System.Collections.Generic;
using System.Numerics;

namespace HKX2
{
    public partial class hclClothSetupObject : hkReferencedObject
    {
        public override uint Signature { get => 3528681272; }
        
        public string m_name;
        public List<hclBufferSetupObject> m_bufferSetupObjects;
        public List<hclTransformSetSetupObject> m_transformSetSetupObjects;
        public List<hclSimClothSetupObject> m_simClothSetupObjects;
        public List<hclOperatorSetupObject> m_operatorSetupObjects;
        public List<hclClothStateSetupObject> m_clothStateSetupObjects;
        
        public override void Read(PackFileDeserializer des, BinaryReaderEx br)
        {
            base.Read(des, br);
            m_name = des.ReadStringPointer(br);
            m_bufferSetupObjects = des.ReadClassPointerArray<hclBufferSetupObject>(br);
            m_transformSetSetupObjects = des.ReadClassPointerArray<hclTransformSetSetupObject>(br);
            m_simClothSetupObjects = des.ReadClassPointerArray<hclSimClothSetupObject>(br);
            m_operatorSetupObjects = des.ReadClassPointerArray<hclOperatorSetupObject>(br);
            m_clothStateSetupObjects = des.ReadClassPointerArray<hclClothStateSetupObject>(br);
        }
        
        public override void Write(PackFileSerializer s, BinaryWriterEx bw)
        {
            base.Write(s, bw);
            s.WriteStringPointer(bw, m_name);
            s.WriteClassPointerArray<hclBufferSetupObject>(bw, m_bufferSetupObjects);
            s.WriteClassPointerArray<hclTransformSetSetupObject>(bw, m_transformSetSetupObjects);
            s.WriteClassPointerArray<hclSimClothSetupObject>(bw, m_simClothSetupObjects);
            s.WriteClassPointerArray<hclOperatorSetupObject>(bw, m_operatorSetupObjects);
            s.WriteClassPointerArray<hclClothStateSetupObject>(bw, m_clothStateSetupObjects);
        }
    }
}
